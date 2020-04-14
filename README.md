# Social.FsharpUtils.GenerateReferences
Tool for fsharp references generation

Usage:

Generate references from bin directory

```
cd src

generate.bat c:\Project\Social.Exporter\bin\Debug\net472 
```
 
To include System.* dll use option `--includesystem`

```
cd src

generate.bat c:\Project\Social.Exporter\bin\Debug\net472 --includesystem
``` 

References will be generated in /src/References.fsx 