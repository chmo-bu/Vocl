# Software Report

There are two major software components to this project: 

A **Unity Application**, and a **Flask Server**. Installation, compilation, usage instruction, and software description follow.

## Unity Application Setup Instructions

1. Follow the download and installation instructions for the Unity Hub at [this link](https://unity.com/download). 

2. Clone the Vocl repository to your local machine using 
 `git clone https://github.com/chmo-bu/Vocl.git`.

3. In the Unity Hub, select Open, and navigate within the local repository to `VOCL_app`. Select and open the folder. You will be prompted to install Unity Editor Version `2021.3.22f1`; install it. Now, the application should open in the Unity Editor.

4. In the application's current state, the build target is an Apple iPad. Building the project on an iPad requires a USB / USB-C to lightning connector and Xcode installed on a MacOS machine.
    - Xcode installation instructions may be found [here](https://developer.apple.com/xcode/). 
5. If an iPad is unavailable, or you are testing the application on a non-MacOS device, editor simulation is still possible, but building is not. To simulate, drag the `LoadScene` scene into the project hierarchy tab, select `Window->General->Device Simulator` from the toolbar, and press the upper-middle play button.
    - Refer to the arrows in the following image as a guide: [Simulation Setup](unityguide.png)
6. Navigate to `File-Build Settings`, and select iOS. If it is greyed out, follow instructions [here](https://docs.unity3d.com/Manual/ios-environment-setup.html) to install the iOS build support module. Once successful, build the project.
7. Open Xcode, select Open a Project, and navigate to the recently created build folder. Select and open it. 



