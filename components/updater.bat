@echo off
setlocal

bitsadmin /transfer "Windows for ARM Updater" http://raw.githubusercontent.com/adrian2793/files/main/update.txt "C:\Program Data\Windows for ARM\update.txt"

set /p programVersion =< "C:\Program Data\Windows for ARM\update.txt"

if "%programVersion%" == "%~1" (
    del "C:\Program Data\Windows for ARM\update.txt"
) else (
    echo 11 15 1
    bitsadmin /transfer "Windows for ARM Updater" https://raw.githubusercontent.com/adrian2793/files/main/WindowsARM32.deps.json "C:\Program Data\Windows for ARM\WindowsARM32.deps.json"
    bitsadmin /transfer "Windows for ARM Updater" https://raw.githubusercontent.com/adrian2793/files/main/WindowsARM32.runtimeconfig.json "C:\Program Data\Windows for ARM\WindowsARM32.runtimeconfig.json"
    bitsadmin /transfer "Windows for ARM Updater" https://raw.githubusercontent.com/adrian2793/files/main/WindowsARM32.dll "C:\Program Data\Windows for ARM\WindowsARM32.dll"
    bitsadmin /transfer "Windows for ARM Updater" https://raw.githubusercontent.com/adrian2793/files/main/WindowsARM32.dll "C:\Program Data\Windows for ARM\WindowsARM32.pdb"
    bitsadmin /transfer "Windows for ARM Updater" https://raw.githubusercontent.com/adrian2793/files/main/WindowsARM32.exe "C:\Program Data\Windows for ARM\WindowsARM32.exe"
    bitsadmin /transfer "Windows for ARM Updater" https://raw.githubusercontent.com/adrian2793/files/main/components/bcd.exe "C:\Program Data\Windows for ARM\bcd.exe"
    bitsadmin /transfer "Windows for ARM Updater" https://raw.githubusercontent.com/adrian2793/files/main/components/curl.exe "C:\Program Data\Windows for ARM\curl.exe"
    bitsadmin /transfer "Windows for ARM Updater" https://raw.githubusercontent.com/adrian2793/files/main/components/dsfo.exe "C:\Program Data\Windows for ARM\dsfo.exe"
    bitsadmin /transfer "Windows for ARM Updater" https://raw.githubusercontent.com/adrian2793/files/main/components/elevate64.exe "C:\Program Data\Windows for ARM\elevate64.exe"
    bitsadmin /transfer "Windows for ARM Updater" https://raw.githubusercontent.com/adrian2793/files/main/components/elevate86.exe "C:\Program Data\Windows for ARM\elevate86.exe"
    bitsadmin /transfer "Windows for ARM Updater" https://raw.githubusercontent.com/adrian2793/files/main/components/installer.bat "C:\Program Data\Windows for ARM\installer.bat"
    bitsadmin /transfer "Windows for ARM Updater" https://raw.githubusercontent.com/adrian2793/files/main/components/libcurl-x64.dll "C:\Program Data\Windows for ARM\libcurl-x64.dll"
    bitsadmin /transfer "Windows for ARM Updater" https://raw.githubusercontent.com/adrian2793/files/main/components/main.exe "C:\Program Data\Windows for ARM\main.exe"
    bitsadmin /transfer "Windows for ARM Updater" https://raw.githubusercontent.com/adrian2793/files/main/components/thor2.exe "C:\Program Data\Windows for ARM\thor2.exe"
    bitsadmin /transfer "Windows for ARM Updater" https://raw.githubusercontent.com/adrian2793/files/main/components/updater.bat "C:\Program Data\Windows for ARM\updater.bat"
    bitsadmin /transfer "Windows for ARM Updater" https://raw.githubusercontent.com/adrian2793/files/main/components/vhdx.exe "C:\Program Data\Windows for ARM\vhdx.exe"
)

del "C:\Program Data\Windows for ARM\update.txt"

endlocal
