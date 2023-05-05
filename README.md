# VOCL

Boston University Senior Design Project 2023

Mia Hernandez, Krishan Eskew, William Nilsen, and Charles Mo

VOCL is a software project created in Unity for the 2023-2024 ECE Senior Design cohort at the Boston University College of Engineering. VOCL was created for client Andrew Vyshedskiy of ImagiRation, and we owe a great deal of our progress to him and the legacy code given to us from ImagiRation in order to kickstart our project. VOCL was created by Senior Design Team 12: Krishan Eskew, Mia Hernandez, Charles Mo, and William Nilsen. The following README is intended to provide a brief introduction and overview to VOCL as well as guides for challenges faced by our team when creating the project in order to prevent headaches for future developers.

VOCL is an iOS application developed in the Unity engine aimed at young children with nonverbal autism. The market for entertainment geared toward these children with nonverbal autism is woefully underdeveloped, and VOCL aims to provide them with a fun experience that engages them with both tactile and aural reactive inputs

VOCL is structured a set of games which can be selected from the main menu. These games contain a series of smaller, objective-oriented tasks. Tasks often involve interaction with graphical assets on screen, and can follow a larger narrative over the course of the game (e.g. collecting pieces of candy in a basket). Tasks implement some form of user interaction, be it moving objects by sliding a finger, tapping objects in place, or requiring some form of nonverbal audio input. These tasks are designed to be simple, as we are developing for young children with autism, and over the course of development we followed a mantra of “simplicity is best” when it comes to UI/UX design.

The following paragraphs each describe an aspect of VOCL and challenges we faced while developing that aspect. We hope that you will be able to gain insight from this in order to avoid similar pitfalls while developing. 

One critical part of VOCL is the audio processing functionality. VOCL uses a combination of machine learning and traditional audio processing in order to respectively classify and quantify user audio input for the purpose of completing tasks. The machine learning section uses a retrained version of the YAMnet network model, derived from the Mobilenet architecture, that is trained to look for distinct, relevant audio inputs such as clapping. 



