# autoklpr
My attempt at getting ultimateALPR to work live.

- This console application will ingest a YUV420P video stream through stdin.
- The application processes it using [ultimateALPR-SDK](https://github.com/DoubangoTelecom/ultimateALPR-SDK) and outputs detections to be used in plate OCR or other data collection methods.

### Setup
As this is a WIP, the docs here are very rough.

1. Clone the [ultimateALPR-SDK](https://github.com/DoubangoTelecom/ultimateALPR-SDK)
2. Edit the `autolpr/autolpr.csproj` file so that the reference to the ultimateALPR-SDK.dll is valid, located in the repo you cloned above.
3. Edit `autolpr/Program.cs` `assets_folder` value so that it points to the assets folder in the ultimateALPR-SDK repo you cloned above.
