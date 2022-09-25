# Auto Clicker
* execute pre-recorded mouse left click event
* **This app only runs on Window**

- [Develop Environment](#develop-environment)
- [Setup Guide](#setup-guide)
- [UI Introduction](#ui-introduction)
- [Other Instruction](#other-instruction)
- [Changelog](#Changelog) 
## Develop Environment
* Window 10
* Visual Studio 2022
* WPF
* .net 6.0

## Setup Guide
### Record
1. Choose your record save directory
2. Name the file **(don't leave space and use special character)**
3. if you want to start a program before recording, choose load program directory and check the box
4. Click the record Button
5. Perform your mouse click
6. Click again the record button

### Execute
1. Choose a load directory and check the box
2. Choose the program directory and check the box if needed
3. Click the start button
4. Wait Until the execution finish

## UI Introduction
![Instruction](https://github.com/Shellcial/AutoClicker/blob/main/README_Images/Instruction.png)
1. Here shows the program status and issues encountered during excecution
2. Check it and it will auto start when open Auto Clicker
3. choose the save location and define file name, otherwise recording won't start
4. choose and check the load file you want, otherwise recording won't be executed
5. choose a program that you want to start when recording and also executing
6. start and stop recording
7. start button

## Other Instruction
- When Starting Execution, a progress bar will display and show the percentage of excecution
- A setting file will be created at AutoClicker directory, you can adjust the save and load directory parameter directly in this file.
![AutoClickSetting](https://github.com/Shellcial/AutoClicker/blob/main/README_Images/AutoClickSetting.PNG)

- For Load Program, if you choose a shortcut link, it may result in directing to the target directory without parameter. In this case, you can directly type the lnk path in the setting file.

In the below case, Auto Clicker will only get the path as *C:\Program Files\BlueStacks_nxt\HD-Player.exe*
![lnk_1](https://github.com/Shellcial/AutoClicker/blob/main/README_Images/lnk_1.PNG)

To solve this, manually set the lnk path for the *programPath* value in setting file
![lnk_2](https://github.com/Shellcial/AutoClicker/blob/main/README_Images/lnk_2.PNG)

- There is a customOffsetTime parameter in the save record file, you can adjust the time(in second) to delay or execute the mouse click early.
- When custom duplicating mouse click, the "order" doesn't mean anything so leave it unchange is fine
- Keep the app as default resolution (720x480), otherwise UI Components will be shifted and cropped

## Changelog
### 1.0.0 
* Initial release
