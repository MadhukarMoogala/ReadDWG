# Hosting RealDWG on Web

This is a demo ASP .NET  web application using .NET 4.8 Framework to process AutoCAD drawings using [RealDWG SDK]([RealDWG Platform Technologies | Autodesk Developer Network](https://www.autodesk.com/developer-network/platform-technologies/realdwg)) in the code behind.

The Solution has three projects, lets understand each -

- ReadDWG
  
  - This is the main drawing processing library that uses RealDWG SDK.
  
  - This is has one interface, `IProcessingDrawing`, that takes drawing path and fetchs list of Blocks and Layers names.

- Dummy
  
  - This is a CLI app to test the `ReadDWG` library.

- WebApp
  
  - This is a web app with one endpoint `api/values` that fetchs list of Blocks and Layers names on web HTML

## Prerequisties

- Get the `Autodesk RealDWG SDK` from [Techsoft3d]([Autodesk RealDWG | Tech Soft 3D](https://www.techsoft3d.com/products/realdwg/)) vendor, they are the only RealDWG resellers of Autodesk.

- `Set REALDWGSDK= <Path of RealDWG SDK>`
  
  - example  `SET REALDWGSDK=D:\RD23\RealDWG 2023\`

- Download [.NET Framework 4.8 Developer Pack]([Download .NET Framework 4.8 | Free official downloads](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48))

- Download Visual Studio 2019 any version, community edition also works.

- Get a [test drawing](https://download.autodesk.com/us/samplefiles/acad/blocks_and_tables_-_metric.dwg) 

## Instructions To Build

```bash
git clone https://github.com/MadhukarMoogala/ReadDWG.git
cd ReadDWG
msbuild /t:build /p:Configuration=Debug;Platform=x64
```

1. Add `AcDbMgd.dll` reference from .\RealDWG 2023\ to ReadDWG project.
   
   1. Build ReadDWG project.
   
   2. Note, the path of ReadDWG.dll.

2. Add `ReadDWG.dll` reference to `Dummy` Project.
   
   1. Add `AcDbMgd.dll` reference to `Dummy` Project.
   
   2. Set the `Copy Local` to False in the `AcDbMgd.dll` reference properties.
   
   3. Build Dummy Project.

3. Add `ReadDWG.dll` reference to `WebApp` Project. 
   
   1. Add `AcDbMgd.dll` reference to `WebApp` Project.
   2. Set the `Copy Local` to False in the `AcDbMgd.dll` reference properties.
   3. Build WepApp Project.

Building Dummy Project is optional, as this is test cli project to check if the our library is working.

## Demo

<img src="https://github.com/MadhukarMoogala/ReadDWG/blob/master/Webapp-RealDWGSDK.gif" title="WebApp" alt="Demo" width="640">

### License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). Please see the [LICENSE]([ReadDWG/LICENSE at master · MadhukarMoogala/ReadDWG · GitHub](https://github.com/MadhukarMoogala/ReadDWG/blob/master/LICENSE)) file for full details.

### Written by

Madhukar Moogala, [Forge Partner Development](http://forge.autodesk.com)  @galakar