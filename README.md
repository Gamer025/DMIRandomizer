# DMIRandomizer
A WPF C# tool for randomizing DMI files of the game engine byond

This is a little WPF tool for randomizing DMI files.
THis happens by randomly copying random sprites over other sprites

This can be done either in one file itself or you can insert sprites from file A into file B.


## How to use

* Download the newest version of DMIRandomizer from https://github.com/Gamer025/DMIRandomizer/releases or compile it yourself
* Download exiftool from https://www.sno.phy.queensu.ca/~phil/exiftool/ (at least version 11.63) and place it in the same directory as the DMIRandomizer exe
* Have fun!

## Modes
DMIRandomizer has multiple modes

### Single DMI file
This mode will take one DMI file and shuffle the sprites in the file itself around (copy area A over area B)

### Mix DMI in different DMI
This mode will take two DMI files and will copy random sprites from the SourceFile into random locations of the TargetFile.

## Multiple DMI files in folder
This options takes the path of a folder and will randomly choose two DMI files in the folder or any of it subfolders.
File A will then be mixed into file B like the "Mix DMI in different DMI" option does.
Its also possible to only mix files them self, see the option "Percentage of self mixing" for more information

## Options
DMIRandomizer comes with the following options:

![Picture of options](https://i.imgur.com/yULcnXw.png)

### Shuffle Multiplier:
Determines how often sprites are copied. This is relative to the amount of sprites in the file / both files when mixing.
The higher this value the higher the amount of shuffled sprites / sprites from a different DMI being mixed in.
### Strech/Compress sprites if needed (Mixing):
When you (or the folder option) mixes DMIs with sprites of different resolution this option determines if the source sprites will be compressed / strechted to the resolution of the target file (may not look very good)
### Percentage of self mixing
This option only applies when using the folder mode. It determines how often one files will be mixed into another one or mixed in itself.
If this is set to 0 the folder mode will always mix a file A into a file B. If set to 100 the folder mode will always mix a file with itself and no cross mixing will occur.
