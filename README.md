# IFC to Revit using Named Pipes

This is a sample application demonstrating how to use named pipes to establish a communication between a Revit plugin and a console application.
The console application is an IFC files loader that is built on top of xBIM toolkit. Although this is a great demo how to use Named Pipe 
to establish a two-way communication with a Revit context, there was a technical necessity to follow this design. The necessity arises from
a DLL collision that occurs when trying to use the xBIM toolkit inside Revit context directly; Microsoft.Extensions.Logging DLL is a common
dependency between Revit and xBIM but each ecosystem has a different incompatible version. So, the solution was to run the IFC parsing/loading 
operations out-of-process with Revit and establish the communication through named pipes.

### Vidoe tutorial:
[![Watch the video here!](https://user-images.githubusercontent.com/50090593/138493431-3d7768c4-d2fe-4f95-9ee2-447aea5c13b2.png)](https://www.youtube.com/watch?v=sGKHa4ep-xE)
