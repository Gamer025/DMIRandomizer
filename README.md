# DMIRandomizer
A WPF C# tool for randomizing DMI files of the game engine byond

This is a little WPF tool for randomizing DMI files.
THis happens by randomly copying random sprites over other sprites

This can be done either in one file itself or you can insert sprites from file A into file B.

NEW: The folder options now also works
This option allows you to select a folder and the tool will find all DMI files.
it will then pick two DMI files a source and a target file. The source files sprites will be randomly injected into the target and the target will be removed from the list.

To use this tool you need https://github.com/exiftool/exiftool and set "exiftool" accordingly.

Currently exiftool imports the zTXt metadata at the end of the file resulting in the output .dmi file to be unreadable by Byond.
So after mixing a DMI you need to move zTXt above IDAT with the program TweakPNG http://entropymine.com/jason/tweakpng/
