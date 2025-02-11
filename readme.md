## windowsARM toolkit

> [!CAUTION]
> Not actively maintained

(Installer.cs -> Class to use for installing windowsARM)
(Driver.cs -> Runs all the additional exe files needed)

**[WoA.cs](/kernel/WoA.cs) -> Install Windows simple just like drivers**

(Do not use the other class preferably)

> [!TIP]
> Leave a star and check out the wiki

Progress `100 / 100`

### windowsARM as C# class

windowsARM contains the following files fully rewritten as C# class by myself 🖤

### Features

- [x] Flash `.img` `.ffu` and `.wim` files

  - [x] Single boot
    
  - [x] Dual boot
    
  - [x] Kernel debug
    
  - [ ] Install Windows 10 Desktop on a SD card
    
- [ ] Unlock the bootloader

- [ ] Restore bootloader

- [ ] Enable Root Access

- [ ] Disable Root Access

- [ ] Create a backup of the Windows phone

- [ ] Custom platformIO like configuration files

- [ ] Registry editor for PC to edit Windows phone registry keys

- [ ] Includes Chromium as app for Windows 10 Mobile

  - [ ] arm32

  - [ ] arm64
    
- [ ] Includes a working Unigram mobile like Telegram client for Windows 10 Mobile

  - [ ] arm32
    
- [ ] Remote desktop app for Windows 10 Mobile

  - [ ] arm32
    
- [ ] DynamicDNS tool for Windows 10 Mobile

  - [ ] arm32
    
- [x] Server side files

### Libraries

Here is a overview of all the libraries I made to make this project work

- `FileSystem` Read and write configuration files using a given syntax
- `Kernel` Executes the main functions like for example unlocking the bootloader
