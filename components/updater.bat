@echo off
setlocal

bitsadmin /transfer "Windows for ARM Updater" http://raw.githubusercontent.com/adrian2793/files/main/update.txt "C:\Program Data\Windows for ARM\update.txt"

set /p programVersion =< "C:\Program Data\Windows for ARM\update.txt"

if "%programVersion%" == "%~1" (
    del "C:\Program Data\Windows for ARM\update.txt"
) else (
    echo 11 15
    bitsadmin /transfer "Windows for ARM Updater" https://raw.githubusercontent.com/adrian2793/files/main/WindowsARM32.deps.json "C:\Program Data\Windows for ARM\WindowsARM32.deps.json"
    bitsadmin /transfer "Windows for ARM Updater" https://raw.githubusercontent.com/adrian2793/files/main/WindowsARM32.dll "C:\Program Data\Windows for ARM\WindowsARM32.dll"
    bitsadmin /transfer "Windows for ARM Updater" https://raw.githubusercontent.com/adrian2793/files/main/WindowsARM32.exe "C:\Program Data\Windows for ARM\WindowsARM32.exe"
    bitsadmin /transfer "Windows for ARM Updater" https://raw.githubusercontent.com/adrian2793/files/main/WindowsARM32.runtimeconfig.json "C:\Program Data\Windows for ARM\WindowsARM32.runtimeconfig.json"
)

del "C:\Program Data\Windows for ARM\update.txt"

endlocal
