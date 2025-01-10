using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Win32;

class Program
{
    static void Main(string[] args)
    {
        string currentDir = Directory.GetCurrentDirectory();
        string currentPath = currentDir.Replace("!", "");

        if (currentPath != currentDir)
        {
            // Handle path with exclamation marks
            return;
        }

        if (!IsAdministrator())
        {
            // Request administrative privileges
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.UseShellExecute = true;
            processInfo.WorkingDirectory = Environment.CurrentDirectory;
            processInfo.FileName = Environment.GetCommandLineArgs()[0];
            processInfo.Verb = "runas";

            try
            {
                Process.Start(processInfo);
            }
            catch
            {
                // Unable to grant administrative privileges
            }

            return;
        }

        if (!Directory.Exists("Temp"))
        {
            Directory.CreateDirectory("Temp");
        }

        int winBuild = GetWindowsBuild();
        if (winBuild < 9200)
        {
            // Handle unsupported Windows version
            return;
        }

        if (!CheckPowershell())
        {
            // Handle missing PowerShell
            return;
        }

        if (!CheckPowershellCmdlets())
        {
            // Handle missing PowerShell cmdlets
            return;
        }

        string model = "1"; // Replace with the desired model
        string devSpec = "B";
        bool hasCameraBtn = true;
        bool largeStorage = true;

        string mainOS = GetMainOSPath();
        if (string.IsNullOrEmpty(mainOS))
        {
            // Handle invalid MainOS path
            return;
        }

        bool dualboot = false; // Replace with the desired value
        int win10SizeMB = 8192; // Replace with the desired value
        bool debugEnabled = false; // Replace with the desired value
        int chargeThreshold = 0; // Replace with the desired value

        // Perform installation steps
        PerformInstallation(mainOS, model, devSpec, hasCameraBtn, largeStorage, dualboot, win10SizeMB, debugEnabled, chargeThreshold);
    }

    static bool IsAdministrator()
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    static int GetWindowsBuild()
    {
        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
        {
            if (key != null)
            {
                object value = key.GetValue("CurrentBuild");
                if (value != null)
                {
                    return Convert.ToInt32(value);
                }
            }
        }

        return 0;
    }

    static bool CheckPowershell()
    {
        try
        {
            Process.Start("powershell.exe", "/?");
            return true;
        }
        catch
        {
            return false;
        }
    }

    static bool CheckPowershellCmdlets()
    {
        Process process = new Process();
        process.StartInfo.FileName = "powershell.exe";
        process.StartInfo.Arguments = "-C \"(Get-Command).name\"";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        string[] requiredCmdlets = { "Get-Date", "Get-Volume", "Get-Partition", "New-Partition", "Resize-Partition", "Remove-Partition", "Get-CimInstance", "Get-PartitionSupportedSize", "Add-PartitionAccessPath" };

        foreach (string cmdlet in requiredCmdlets)
        {
            if (!output.Contains(cmdlet))
            {
                return false;
            }
        }

        return true;
    }

    static string GetMainOSPath()
    {
        Console.Write("Enter MainOS Path: ");
        string mainOS = Console.ReadLine();

        if (!Directory.Exists(Path.Combine(mainOS, "EFIESP")) || !Directory.Exists(Path.Combine(mainOS, "Data")))
        {
            return string.Empty;
        }

        return mainOS;
    }

    static void PerformInstallation(string mainOS, string model, string devSpec, bool hasCameraBtn, bool largeStorage, bool dualboot, int win10SizeMB, bool debugEnabled, int chargeThreshold)
    {
        // Check partition for errors
        CheckDisk(mainOS);

        if (dualboot)
        {
            Directory.CreateDirectory(Path.Combine(mainOS, "Windows10"));

            if (devSpec == "A")
            {
                CreateVHDX(Path.Combine(mainOS, "Data", "Windows10.vhdx"), win10SizeMB);
            }
            else
            {
                ResizePartition(mainOS, win10SizeMB);
                CreatePartition(mainOS);
            }
        }
        else
        {
            RemovePartition(mainOS);
            ResizePartition(mainOS, 0);
        }

        string win10Drive = dualboot ? Path.Combine(mainOS, "Windows10") : mainOS;

        FormatPartition(win10Drive);

        InstallWindows10(win10Drive);

        InstallDrivers(win10Drive, model);

        EnablePageFile(win10Drive);

        MountEFIESPAndDPP(mainOS, win10Drive);

        InstallMassStorageModeUI(mainOS);

        AddBCDEntry(mainOS, win10Drive, devSpec, dualboot, hasCameraBtn, debugEnabled, chargeThreshold);

        SetupESP(mainOS, win10Drive);

        if (devSpec == "A" && dualboot)
        {
            UnmountVHDX(Path.Combine(mainOS, "Data", "Windows10.vhdx"));
        }
    }

    static void CheckDisk(string mainOS)
    {
        Process process = new Process();
        process.StartInfo.FileName = "chkdsk.exe";
        process.StartInfo.Arguments = $"/f /x {Path.Combine(mainOS, "Data")}";
        process.Start();
        process.WaitForExit();

        process = new Process();
        process.StartInfo.FileName = "chkdsk.exe";
        process.StartInfo.Arguments = $"/f /x {mainOS}";
        process.Start();
        process.WaitForExit();
    }

    static void CreateVHDX(string vhdxPath, int sizeMB)
    {
        Process process = new Process();
        process.StartInfo.FileName = "vhdxtool.exe";
        process.StartInfo.Arguments = $"create -f \"{vhdxPath}\" -s {sizeMB}MB -v";
        process.Start();
        process.WaitForExit();

        process = new Process();
        process.StartInfo.FileName = "diskpart.exe";
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();

        using (StreamWriter sw = process.StandardInput)
        {
            sw.WriteLine($"sel vdisk file={vhdxPath}");
            sw.WriteLine("attach vdisk");
            sw.WriteLine("convert gpt");
            sw.WriteLine("create par pri");
            sw.WriteLine("format quick fs=ntfs");
            sw.WriteLine($"assign mount={Path.Combine(mainOS, "Windows10")}");
        }

        process.WaitForExit();
    }

    static void ResizePartition(string mainOS, int win10SizeMB)
    {
        Process process = new Process();
        process.StartInfo.FileName = "powershell.exe";
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();

        using (StreamWriter sw = process.StandardInput)
        {
            if (win10SizeMB == 0)
            {
                sw.WriteLine($"Resize-Partition -DriveLetter {Path.GetPathRoot(mainOS)} -Size (Get-PartitionSupportedSize -DriveLetter {Path.GetPathRoot(mainOS)}).sizeMax");
            }
            else
            {
                sw.WriteLine($"$Partition = Get-Partition -DriveLetter {Path.GetPathRoot(mainOS)}");
                sw.WriteLine($"$DataPartSizeMB = [Math]::Floor(($Partition.Size / 1MB) - {win10SizeMB})");
                sw.WriteLine($"Resize-Partition -DiskNumber $($Partition.DiskNumber) -PartitionNumber $($Partition.PartitionNumber) -Size {win10SizeMB}MB");
            }
        }

        process.WaitForExit();
    }

    static void CreatePartition(string mainOS)
    {
        Process process = new Process();
        process.StartInfo.FileName = "powershell.exe";
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();

        using (StreamWriter sw = process.StandardInput)
        {
            sw.WriteLine($"$Partition = Get-Partition -DriveLetter {Path.GetPathRoot(mainOS)}");
            sw.WriteLine("New-Partition -DiskNumber $($Partition.DiskNumber) -UseMaximumSize | Add-PartitionAccessPath -AccessPath '$($env:SystemDrive)\\Windows10'");
        }

        process.WaitForExit();
    }

    static void RemovePartition(string mainOS)
    {
        Process process = new Process();
        process.StartInfo.FileName = "powershell.exe";
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();

        using (StreamWriter sw = process.StandardInput)
        {
            sw.WriteLine($"$Partition = Get-Partition -DriveLetter {Path.GetPathRoot(mainOS)}");
            sw.WriteLine("Remove-Partition -DiskNumber $($Partition.DiskNumber) -PartitionNumber $($Partition.PartitionNumber) -Confirm:$false");
        }

        process.WaitForExit();
    }

    static void FormatPartition(string win10Drive)
    {
        Process process = new Process();
        process.StartInfo.FileName = "format.exe";
        process.StartInfo.Arguments = $"/FS:NTFS /V:Windows10 /Q /C /Y {win10Drive}";
        process.Start();
        process.WaitForExit();
    }

    static void InstallWindows10(string win10Drive)
    {
        Process process = new Process();
        process.StartInfo.FileName = "dism.exe";
        process.StartInfo.Arguments = $"/Apply-Image /ImageFile:\"./install.wim\" /Index:1 /ApplyDir:{win10Drive}\\ {(GetWindowsBuild() < 10240 ? "" : "/Compact")}";
        process.Start();
        process.WaitForExit();
    }

    static void InstallDrivers(string win10Drive, string model)
    {
        Process process = new Process();
        process.StartInfo.FileName = "dism.exe";
        process.StartInfo.Arguments = $"/Image:{win10Drive}\\ /Add-Driver /Driver:\"./Drivers/{model}\" /Recurse";
        process.Start();
        process.WaitForExit();
    }

    static void EnablePageFile(string win10Drive)
    {
        RegistryKey rtsystemKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default).OpenSubKey("SYSTEM", true);
        using (RegistryKey controlSet001Key = rtsystemKey.OpenSubKey("ControlSet001", true))
        {
            using (RegistryKey memoryManagementKey = controlSet001Key.OpenSubKey("Control\\Session Manager\\Memory Management", true))
            {
                memoryManagementKey.SetValue("PagingFiles", new string[] { $"{Path.Combine(win10Drive, "pagefile.sys")} 512 768" }, RegistryValueKind.MultiString);
            }
        }
    }

    static void MountEFIESPAndDPP(string mainOS, string win10Drive)
    {
        Directory.CreateDirectory(Path.Combine(win10Drive, "EFIESP"));
        Directory.CreateDirectory(Path.Combine(win10Drive, "DPP"));

        Process process = new Process();
        process.StartInfo.FileName = "diskpart.exe";
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();

        using (StreamWriter sw = process.StandardInput)
        {
            sw.WriteLine($"sel dis {GetDiskNumber(mainOS)}");
            sw.WriteLine($"sel par {GetPartitionNumber(mainOS, "EFIESP")}");
            sw.WriteLine($"assign mount={Path.Combine(win10Drive, "EFIESP")}");
            sw.WriteLine($"sel par {GetPartitionNumber(mainOS, "DPP")}");
            sw.WriteLine($"assign mount={Path.Combine(win10Drive, "DPP")}");
        }

        process.WaitForExit();
    }

    static void InstallMassStorageModeUI(string mainOS)
    {
        string sourceDir = Path.Combine(Directory.GetCurrentDirectory(), "Files", "MassStorage");
        string targetDir = Path.Combine(mainOS, "EFIESP", "Windows", "System32", "Boot", "ui");
        CopyDirectory(sourceDir, targetDir);
    }

    static void AddBCDEntry(string mainOS, string win10Drive, string devSpec, bool dualboot, bool hasCameraBtn, bool debugEnabled, int chargeThreshold)
{
    string bcdLoc = Path.Combine(mainOS, "EFIESP", "EFI", "Microsoft", "Boot", "BCD");
    Guid id = new Guid("{703c511b-98f3-4630-b752-6d177cbfb89c}");

    RunBCDEdit(bcdLoc, $"/create {id} /d \"Windows 10 ARM\" /application osloader");

    if (devSpec == "A")
    {
        if (dualboot)
        {
            RunBCDEdit(bcdLoc, $"/set {id} device vhd=[{Path.Combine(mainOS, "Data")}]\\Windows10.vhdx");
            RunBCDEdit(bcdLoc, $"/set {id} osdevice vhd=[{Path.Combine(mainOS, "Data")}]\\Windows10.vhdx");
        }
        else
        {
            RunBCDEdit(bcdLoc, $"/set {id} device partition={win10Drive}");
            RunBCDEdit(bcdLoc, $"/set {id} osdevice partition={win10Drive}");
        }
    }
    else
    {
        RunBCDEdit(bcdLoc, $"/set {id} device partition={win10Drive}");
        RunBCDEdit(bcdLoc, $"/set {id} osdevice partition={win10Drive}");
    }

    RunBCDEdit(bcdLoc, $"/set {id} path \"\\Windows\\System32\\winload.efi\"");
    RunBCDEdit(bcdLoc, $"/set {id} systemroot \"\\Windows\"");
    RunBCDEdit(bcdLoc, $"/set {id} locale en-US");
    RunBCDEdit(bcdLoc, $"/set {id} testsigning Yes");
    RunBCDEdit(bcdLoc, $"/set {id} nointegritychecks Yes");
    RunBCDEdit(bcdLoc, $"/set {id} inherit {{bootloadersettings}}");
    RunBCDEdit(bcdLoc, $"/set {id} bootmenupolicy Standard");
    RunBCDEdit(bcdLoc, $"/set {id} detecthal Yes");
    RunBCDEdit(bcdLoc, $"/set {id} winpe No");
    RunBCDEdit(bcdLoc, $"/set {id} ems No");

    if (debugEnabled)
    {
        RunBCDEdit(bcdLoc, $"/set {id} debug Yes");
    }
    else
    {
        RunBCDEdit(bcdLoc, $"/set {id} debug No");
    }

    RunBCDEdit(bcdLoc, $"/set {{dbgsettings}} debugtype USB");
    RunBCDEdit(bcdLoc, $"/set {{dbgsettings}} targetname \"WOATARGET\"");

    if (dualboot)
    {
        RunBCDEdit(bcdLoc, $"/set {{default}} description \"Windows Phone\"");
        RunBCDEdit(bcdLoc, $"/set {{bootmgr}} displayorder {id} {{default}}");

        if (hasCameraBtn)
        {
            RunBCDEdit(bcdLoc, $"/deletevalue {{bootmgr}} customactions");
        }
        else
        {
            RunBCDEdit(bcdLoc, $"/set {{bootmgr}} customactions 0x1000048000001 0x54000001 0x1000050000001 0x54000002");
            RunBCDEdit(bcdLoc, $"/set {{bootmgr}} custom:0x54000001 {id}");
        }
    }
    else
    {
        RunBCDEdit(bcdLoc, $"/set {{bootmgr}} default {id}");
        RunBCDEdit(bcdLoc, $"/set {{bootmgr}} displayorder {id}");
    }

    RunBCDEdit(bcdLoc, $"/set {{bootmgr}} nointegritychecks Yes");
    RunBCDEdit(bcdLoc, $"/set {{bootmgr}} testsigning Yes");
    RunBCDEdit(bcdLoc, $"/set {{bootmgr}} booterrorux Standard");
    RunBCDEdit(bcdLoc, $"/set {{bootmgr}} displaybootmenu Yes");
    RunBCDEdit(bcdLoc, $"/set {{bootmgr}} timeout 5");

    if (!dualboot)
    {
        RunBCDEdit(bcdLoc, $"/set {{globalsettings}} chargethreshold {chargeThreshold}");
    }
}

static void SetupESP(string mainOS, string win10Drive)
{
    Directory.CreateDirectory(Path.Combine(mainOS, "EFIESP", "EFI", "Microsoft", "Recovery"));

    RunBCDEdit($"/createstore {Path.Combine(mainOS, "EFIESP", "EFI", "Microsoft", "Recovery", "BCD")}");

    Process process = new Process();
    process.StartInfo.FileName = "diskpart.exe";
    process.StartInfo.RedirectStandardInput = true;
    process.StartInfo.UseShellExecute = false;
    process.Start();

    using (StreamWriter sw = process.StandardInput)
    {
        sw.WriteLine($"sel dis {GetDiskNumber(mainOS)}");
        sw.WriteLine($"sel par {GetPartitionNumber(mainOS, "EFIESP")}");
        sw.WriteLine("set id=c12a7328-f81f-11d2-ba4b-00a0c93ec93b override");
    }

    process.WaitForExit();

    string postInstallCmdPath = Path.Combine(Directory.GetCurrentDirectory(), "Files", "PostInstall", "PostInstall.cmd");
    File.Copy(postInstallCmdPath, Path.Combine(win10Drive, "PostInstall.cmd"));
}

static void UnmountVHDX(string vhdxPath)
{
    Process process = new Process();
    process.StartInfo.FileName = "diskpart.exe";
    process.StartInfo.RedirectStandardInput = true;
    process.StartInfo.UseShellExecute = false;
    process.Start();

    using (StreamWriter sw = process.StandardInput)
    {
        sw.WriteLine($"sel vdisk file={vhdxPath}");
        sw.WriteLine("detach vdisk");
    }

    process.WaitForExit();
}

static void RunBCDEdit(string arguments)
{
    Process process = new Process();
    process.StartInfo.FileName = "bcdedit.exe";
    process.StartInfo.Arguments = arguments;
    process.Start();
    process.WaitForExit();
}

    static int GetDiskNumber(string mainOS)
{
    Process process = new Process();
    process.StartInfo.FileName = "powershell.exe";
    process.StartInfo.RedirectStandardInput = true;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.UseShellExecute = false;
    process.Start();

    using (StreamWriter sw = process.StandardInput)
    {
        sw.WriteLine($"(Get-Partition -DriveLetter {Path.GetPathRoot(mainOS)}).DiskNumber");
    }

    string output = process.StandardOutput.ReadToEnd();
    process.WaitForExit();

    return int.Parse(output.Trim());
}

static int GetPartitionNumber(string mainOS, string partitionName)
{
    Process process = new Process();
    process.StartInfo.FileName = "powershell.exe";
    process.StartInfo.RedirectStandardInput = true;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.UseShellExecute = false;
    process.Start();

    using (StreamWriter sw = process.StandardInput)
    {
        sw.WriteLine($"(Get-Partition -DriveLetter {Path.GetPathRoot(mainOS)} | Where-Object {{$_.GptType -eq '{GetPartitionGuid(partitionName)}'}}).PartitionNumber");
    }

    string output = process.StandardOutput.ReadToEnd();
    process.WaitForExit();

    return int.Parse(output.Trim());
}

static Guid GetPartitionGuid(string partitionName)
{
    switch (partitionName)
    {
        case "EFIESP":
            return new Guid("c12a7328-f81f-11d2-ba4b-00a0c93ec93b");
        case "DPP":
            return new Guid("e3c9e316-0b5c-4db8-817d-f92df00215ae");
        default:
            throw new ArgumentException($"Invalid partition name: {partitionName}");
    }
}

static void CopyDirectory(string sourceDir, string targetDir)
{
    if (!Directory.Exists(targetDir))
    {
        Directory.CreateDirectory(targetDir);
    }

    foreach (string file in Directory.GetFiles(sourceDir))
    {
        string targetFile = Path.Combine(targetDir, Path.GetFileName(file));
        File.Copy(file, targetFile, true);
    }

    foreach (string dir in Directory.GetDirectories(sourceDir))
    {
        string targetSubDir = Path.Combine(targetDir, Path.GetFileName(dir));
        CopyDirectory(dir, targetSubDir);
    }
}

} 

}
