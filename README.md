# BellmanProject
Creating interactive visualisations of Markov Decision Processes, the Bellman Equations, and the algorithms that satisfy them. This project is to satisfy the requirements
of COMP8755 Individual Computing Project for the Master of Computing program at the Australian National University.
Current working branch is the [Alpha Build v0.2](https://github.com/crh82/BellmanProject/tree/AlphaBuildv02).
---
## Contents
- Instructions for downloading and running the software
  - Download instructions for those with an anu.edu.au email address
  - Download instructions for all others
- Note for MacOS—resolutions for Apple's quarantining of non-AppStore software
- Instructions for navigating the software's main solver
- Instructions for navigating the software's grid world builder feature
---
## Instructions for downloading and running the software



The software was developed using the Unity Game Engine and runs as a standalone application. It does not need to be built.

---
#### If you have an anu.edu.au email follow the instructions below. Otherwise, skip to next subsection 
1. Navigate to the [OneDrive directory](https://anu365-my.sharepoint.com/:f:/g/personal/u6810978_anu_edu_au/EtVr8-Dew_hDkou9th8lGskBYH0H0HppADKREnGeMQGZeg?e=SmK61w)
2. Download the directory corresponding to your chosen operating system.
   - **MacOS**
     - Download the *file* `BellmanProjectMacOS.tar.gz`
     - See "Note for MacOS" below before runnging as there are issues with Apple's quarantining of non-AppStore applications.
   - **Linux**
     - Download the *directory* `BellmanProjectLinux`
   - **Windows:**
     - Download the *directory* `BellmanProjectWindows`
3. Run the executable 
   - **MacOS**
     - `BellmanProject_MacOS.app`
   - **Linux**
     - `BellmanProjectLinux.x86_64`
   - **Windows:**
     - `BellmanV0.exe`
---
#### Non-ANU folks can follow these download instructions. 
1. Navigate to your operating system and click download in the top right of the bottom window: 
   - [**MacOS**](https://github.com/crh82/BellmanProject/blob/AlphaBuildv02/BellmanProjectMacOS.tar.gz)
     - SHASUM 256: `09a99e31c4f4cfe38680cf184ac48f91da8987081e256608828968eca6ba786e`
   - [**Linux**](https://github.com/crh82/BellmanProject/blob/AlphaBuildv02/BellmanProjectLinux.tar.gz)
     - SHASUM 256: `936f651acc33b78c0287443a0cb76954dd7c53a0c05cd0bd1d4410c7ef46f2ef`
   - [**Windows**](https://github.com/crh82/BellmanProject/blob/AlphaBuildv02/BellmanProjectWindows.tar.gz)
     - SHASUM 256: `47a8e1b477611c78ef772d834fbb57973956429466ccffde4661b7ae193fddc8`
2. Decompress the tar file.
3. Run the executable 
   - **MacOS**
     - **See "Note for MacOS" below as there are issues with Apple's quarantining of non-AppStore applications.**
     - `BellmanProject_MacOS.app`
   - **Linux**
     - `BellmanProjectLinux.x86_64`
   - **Windows:**
     - `BellmanV0.exe`
---
## Note for MacOS
Once the BellmanProjectMacOS.tar.gz is decompressed, you will need to open the application using the right click open option to permit the software to run—this is due to Apple's quarantining of non-AppStore software.

If the following error displays on running:

<img width="262" alt="Screen Shot 2022-10-24 at 10 14 39 pm" src="https://user-images.githubusercontent.com/103348212/197514079-8a70f959-54a7-4ac3-9022-7dcf8adf0946.png">

The file is not damaged, it is an effect of Apple's quarantining of downloaded software. This is an ongoing issue that the [Unity development team are still trying to address](https://issuetracker.unity3d.com/issues/macos-builds-now-contain-a-quarantine-attribute?page=1#comments). To fix it, open the terminal and run the command:

`xattr -r -d com.apple.quarantine <Path to BellmanProjectMacOS Directory>/BellmanProject_MacOS.app`

If downloading the BellmanProjectMacOS.tar.gz has not worked. Either contact me at u6810978@anu.edu.au or you can download the directory `MacOS_Backup_Upload`. It will download as a `.zip` file. After unzipping the file, run the `BellmanProject_MacOS.app` application. You will likely encounter this error:

<img width="264" alt="Screen Shot 2022-10-24 at 10 52 24 pm" src="https://user-images.githubusercontent.com/103348212/197520273-9ce0e387-2fbf-4be4-9824-e50451abc395.png">

Again, this is because of Apple's stringent quarantining of any software not downloaded from its AppStore, when the file is unzipped and you attempt to run the software you may get an unspecificed error. This is the quarantining. 

This is an ongoing issue that the [Unity development team are still trying to address](https://issuetracker.unity3d.com/issues/macos-builds-now-contain-a-quarantine-attribute?page=1#comments). 

They suggest running the commmand `xattr -r -d com.apple.quarantine path/to/game.app` from the terminal, however this did not work for me when I was testing it. To bypass the issue, in the terminal, I needed to run the command `chmod -R 777 <Path to BellmanProjectMacOS Directory>/BellmanProject_MacOS.app`. The second time I tested it, it then threw the Damaged file error, so I ran the `xattr -r -d ` command on the file and it worked.

---

## Instructions for navigating the software's main solver
*Note that this image is availabe within the software in the both the Main Menu Help Screen by clicking the `Help` button and within the MDP solver by pressing `H`*

![KeyboardAnnotations](https://user-images.githubusercontent.com/103348212/197325505-adecfe79-1b6e-4fe7-a5f3-5d9cc6b5b9ad.png)

### Print-friendly PDF version for download 
[KeyboardAnnotationsWhiteBG.pdf](https://github.com/crh82/BellmanProject/files/9851299/KeyboardAnnotationsWhiteBG.pdf)

---
## Instructions for navigating the software's grid world builder feature
*Note that this image is availabe when using the grid builder by pressing `H`*

![GridWorldBuilderInstructions](https://user-images.githubusercontent.com/103348212/197515742-51bbaad3-56d8-40e4-bc67-18f9b41f06a7.png)

### PDF version for download
[GridWorldBuilderInstructions.pdf](https://github.com/crh82/BellmanProject/files/9851255/GridWorldBuilderInstructions.pdf)

