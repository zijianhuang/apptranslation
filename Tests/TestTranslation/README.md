For the sake of batch processing through CLI, "Google Cloud Translation API" is used.

## Translation using Google Translate 2


## Translation using Google Cloud Translate 3

Google Cloud Translate 3 provides variety of authentication for different usage scenarios. For the sake of integration testing, a client secret JSON file of "Cloud Translation API" is used. And the location of the file is declared in Windows user environment variable "GoogleTranslateV3ClientSecretJsonFile".

References:
* The credentials are set in https://console.cloud.google.com/apis/credentials?project=my-project-id
* https://developers.google.com/identity/protocols/oauth2/native-app?hl=en#uwp
* For integration testing of debug build and the console app, you should have different client secrets. https://developers.google.com/identity/protocols/oauth2/production-readiness/brand-verification?hl=en#projects-used-in-dev-test-stage

## Test Artifacts

The artifacts in folders like html and xlf20 will be copied to the output folder during the first run, handled by the Test Collection. If any file in this folders is changed, please remove the build in the bin folder first.

**Remarks:**
* The XML editor in Visual Studio will reformat the XLIFF file, and the new formatting will change the semantic of the XLIFF file. Better to use a less deligent editor like Notepad++ or VS Code.