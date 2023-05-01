# Software Report

There are two major software components to this project: 

A **Unity Application**, and a **Server**. Installation, compilation, usage instruction, and software description follow.

## Unity Application Setup Instructions

1. Follow the download and installation instructions for the Unity Hub at [this link](https://unity.com/download). 

2. Clone the Vocl repository to your local machine using 
 `git clone https://github.com/chmo-bu/Vocl.git`.

3. In the Unity Hub, select Open, and navigate within the local repository to `VOCL_app`. Select and open the folder. You will be prompted to install Unity Editor Version `2021.3.22f1`; install it. Now, the application should open in the Unity Editor.

4. In the application's current state, the build target is an Apple iPad. Building the project on an iPad requires a USB / USB-C to lightning connector and Xcode installed on a MacOS machine.
    - Xcode installation instructions may be found [here](https://developer.apple.com/xcode/). 
5. If an iPad is unavailable, or you are testing the application on a non-MacOS device, editor simulation is still possible, but building is not. To simulate, drag the `LoadScene` scene into the project hierarchy tab, select `Window->General->Device Simulator` from the toolbar, and press the upper-middle play button.
    - Refer to the arrows in the following image as a guide: [Simulation Setup](/MDattachments/unityguide.png)
6. Navigate to `File-Build Settings`, and select iOS. If it is greyed out, follow instructions [here](https://docs.unity3d.com/Manual/ios-environment-setup.html) to install the iOS build support module. Once successful, build the project.
7. Open Xcode, select Open a Project, and navigate to the recently created build folder. Select and open it. Once loaded into the Xcode environment, follow [these](https://steemit.com/xcode/@ktsteemit/xcode-free-provisioning) instructions to build the app on the target iPad if you don't have an Apple Developer account.

## Server Setup Instructions

1. Once completed, please fill this in, Will.

## Dev-Build Tool Information

### Unity information
1. Unity 
    - Unity Hub version 3.4.1
    - Editor Version version 2021.3.22f1
         - C# version 9.0
         - [Memory Profiler](https://docs.unity3d.com/Packages/com.unity.memoryprofiler@0.7/manual/index.html) version 0.7.1  
2. Assets
    - [Dreamscape Nature : Meadows : Stylized Open World Environment](https://assetstore.unity.com/packages/3d/environments/fantasy/dreamscape-nature-meadows-stylized-open-world-environment-186894) version 2.0
    - [Game Props Factory : Rabbit Bunny Hare Animated](https://assetstore.unity.com/packages/3d/characters/animals/rabbit-bunny-hare-animated-180544#reviews) version 1.1
    - [Candy Package](https://assetstore.unity.com/packages/3d/props/food/candy-package-143935) version 1.1
    - [Animated Loading Icons](https://assetstore.unity.com/packages/2d/gui/icons/animated-loading-icons-47844) version 1.0
    - [Fruit Pack](https://assetstore.unity.com/packages/3d/props/food/fruit-pack-80254) version 1.1
    - [Fantasy Skybox](https://assetstore.unity.com/packages/2d/textures-materials/sky/fantasy-skybox-18216) version 1.6.0
3. Xcode Version 14.2 

### Server Information
1. fill in package versions, etc.

## Software Overview



    





