# WhiteCryptTool
This tool allows you to encrypt and decrypt the filelist files from XIII-2 and XIII-LR as well as the clb files from all the three games. 

The program should be launched from command prompt with a function switch, a filetype switch and with the input file. a list of valid function and filetype switches are given below.

**Function Switches:**
<br>``-d`` - For decryption
<br>``-e`` - For encryption

**Filetype Switches:**
<br>``-filelist`` - For handling XIII-2 and XIII-LR's filelist files
<br>``-clb`` - For handling clb files from all the trilogy

<br>**Commandline usage examples:**
<br>For Filelist:
<br>`` WhiteCryptTool.exe -d -filelist "filelist2v.win32.bin" ``
<br>`` WhiteCryptTool.exe -e -filelist "filelist2v.win32.bin" ``

For clb:
<br>`` WhiteCryptTool.exe -d -clb "common.clb" ``
<br>`` WhiteCryptTool.exe -e -clb "common.clb" ``
