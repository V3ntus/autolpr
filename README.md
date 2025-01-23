# autolpr

My attempt at getting ultimateALPR to work live.

- This console application will ingest a YUV420P video stream through stdin.
- The application processes it using [ultimateALPR-SDK](https://github.com/DoubangoTelecom/ultimateALPR-SDK) and outputs detections to be used in plate OCR or other data collection methods.

## Setup

As this is a WIP, the docs here are very rough.

1. Clone the [ultimateALPR-SDK](https://github.com/DoubangoTelecom/ultimateALPR-SDK) repo.
2. Edit the `autolpr/autolpr.csproj` file so that the reference to the ultimateALPR-SDK.dll is valid, located in the repo you cloned above.
3. Edit `autolpr/Program.cs` `assets_folder` value so that it points to the assets folder in the ultimateALPR-SDK repo you cloned above.

## Issues

### "Cannot add ultimateALPR DDL dependencies to Visual Studio" or "App couldn't find the ultimateALPR module"

Visual Studio 2020 was not able to add the ultimateALPR-SDK .dll's as project dependencies. I had to include them in the csproj folder 
manually and then set the launch settings working directory to the ultimateALPR repo's binaries.

In release, you can mitigate this by [compiling a standalone, static executable](https://stackoverflow.com/questions/71209803/how-to-build-a-single-exe-file-with-static-net-runtime-from-a-c-sharp-wpf-pr).


### "Cannot find /models/ folder"

Make sure you adjusted the config to point to the ultimateALPR repo's assets folder.
