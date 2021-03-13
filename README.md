# Image-Processing-HandGestures
Hand gestures of Indian sign language representing the English alphabets are identified by the windows application using image processing and machine learning methods.


# Process and project summary
Steps to be followed to run the code are as follows.
* Download and install viusal studio 2019 or a more recent version.
* Download or clone the project.
* Download the datasets folder from the link.[Link](https://www.dropbox.com/sh/wvjeespb3v07t2v/AABmSOvFQQoE9VICvHxGZYQDa?dl=0)
* Copy the datasets folder and paste in the runnable application location which is ".\Image-Processing-HandGestures\FinalYearProjectTrial\bin\Debug".
* Open the file in location .\Image-Processing-HandGestures\ named "FinalYearProjectTrial.sln" via the Visual Studio installed application.
* Click on build-->build solution.
* Click on start 
* The "samplegestures" folder in the [Link](https://www.dropbox.com/sh/wvjeespb3v07t2v/AABmSOvFQQoE9VICvHxGZYQDa?dl=0) has some images of hand gestures which can be used to test the functionality of the code.

The project can be summarized as follows.
1. The gesture given by the user is captured which forms the input to the system.
2. This input gesture is subjected to various pre-processing tasks such as skin detection and other morphological filters to obtain a smoothened image.
3. The subset of features is extracted from this image to reduce dimensionality and compute the number of edges per pixel block.
4. The SVM classifier is implemented to classify images into alphabets.
5. The test data is compared with the stored train data and corresponding result is displayed as the output.


# Output and screenshots of the application
The below link contains a pdf of the application and the output.
[OutputLink](https://github.com/Harish-Muralidhar/Image-Processing-HandGestures/blob/main/Output.pdf)
