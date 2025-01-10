using System;
using System.Diagnostics;
using System.IO;

class WindowsInstaller
{
    private const string DismPath = @"C:\Windows\System32\dism.exe";

    public static void InstallWindows(string wimPath, string targetPath, bool compact = false)
    {
        if (!File.Exists(wimPath))
        {
            throw new FileNotFoundException("WIM file not found.", wimPath);
        }

        if (!Directory.Exists(targetPath))
        {
            Directory.CreateDirectory(targetPath);
        }

        string arguments = $"/Apply-Image /ImageFile:\"{wimPath}\" /Index:1 /ApplyDir:{targetPath}\\";

        if (compact)
        {
            arguments += " /Compact";
        }

        RunDismCommand(arguments);
    }

    public static void InstallDrivers(string driverPath, string targetPath)
    {
        if (!Directory.Exists(driverPath))
        {
            throw new DirectoryNotFoundException("Driver directory not found.");
        }

        string arguments = $"/Image:{targetPath}\\ /Add-Driver /Driver:\"{driverPath}\" /Recurse";
        RunDismCommand(arguments);
    }

    public static void EnablePageFile(string targetPath, string pageFilePath, int initialSizeMB, int maximumSizeMB)
    {
        string registryPath = $@"{targetPath}\Windows\System32\config\SYSTEM";

        using (var systemKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default).OpenSubKey("SYSTEM", true))
        using (var controlSet001Key = systemKey.OpenSubKey("ControlSet001", true))
        using (var memoryManagementKey = controlSet001Key.OpenSubKey("Control\\Session Manager\\Memory Management", true))
        {
            memoryManagementKey.SetValue("PagingFiles", new string[] { $"{pageFilePath} {initialSizeMB} {maximumSizeMB}" }, RegistryValueKind.MultiString);
        }
    }

    private static void RunDismCommand(string arguments)
    {
        Process process = new Process();
        process.StartInfo.FileName = DismPath;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception($"DISM command failed with exit code {process.ExitCode}. Output: {output}");
        }
    }
}
