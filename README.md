# Implementing a web application using  RealDWG® 2023

This is a demo ASP .NET  web application using .NET 4.8 Framework to process AutoCAD drawings using [RealDWG SDK]([RealDWG Platform Technologies | Autodesk Developer Network](https://www.autodesk.com/developer-network/platform-technologies/realdwg)) in the code behind.

The Solution has three projects, lets understand each -

- ReadDWG
  
  - This is the main drawing processing library that uses RealDWG SDK.
  
  - This is has one interface, `IProcessingDrawing`, that takes drawing path and fetchs list of Blocks and Layers names.

- Dummy
  
  - This is a CLI app to test the `ReadDWG` library.

- WebApp
  
  - This is a web app with one endpoint `api/values` that fetchs list of Blocks and Layers names on web HTML

## Sytem Requirements

##### RealDWG® 2023

- Operating System: Microsoft® Windows® 11 or Windows 10 version 1809 or above. See Autodesk’s [Product Support Lifecycle](https://knowledge.autodesk.com/customer-service/account-management/users-software/previous-versions/previous-version-support-policy) for support information
- [RealDWG Platform Technologies | Autodesk Developer Network](https://www.autodesk.com/developer-network/platform-technologies/realdwg)
  
  

## Prerequisties

- Get the `Autodesk RealDWG SDK` from [Techsoft3d]([Autodesk RealDWG | Tech Soft 3D](https://www.techsoft3d.com/products/realdwg/)) vendor, they are the only RealDWG resellers of Autodesk.

- `Set REALDWGSDK= <Path of RealDWG SDK>`
  
  - example  `SET REALDWGSDK=D:\RD23\RealDWG 2023\`

- Download [.NET Framework 4.8 Developer Pack]([Download .NET Framework 4.8 | Free official downloads](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48))

- Download Visual Studio 2019 or 2022 any version, community edition also works.

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

```bash
curl https://localhost:44390/api/values
["0","DB - Windows","Defpoints","Dimensions","Text","Viewports","Walls","Stairs","Deck","Cabinetry","Schedules","Appliances","Doors","Power","Lighting","BDRTXT","BRDTITLE","*Model_Space","*Paper_Space","Toilet","Faucet - top","Faucet - front","Sink","Refrigerator","Range_Oven","Bathtub","Door - Bifold","Receptacle","Lighting fixture","Switch","ARCHBDR-D","Drawing Block Title","*B13","*B14","Window","Door - French","*B17","*B18","*B19","*B20","*B21","*B22","*B23","*B24","*T25","*T26","*B27","Door","*B29","*B30","*B31","*B32","*B33","*B34","*B35","*B36","*B37","*U38","*U39","*U40","*U41","*U42","*U43","*U44","*U45","*U46","*U47","*U48","*U49","*U50","*U51","*U52","*U53"]
```

### License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). Please see the [LICENSE]([ReadDWG/LICENSE at master · MadhukarMoogala/ReadDWG · GitHub](https://github.com/MadhukarMoogala/ReadDWG/blob/master/LICENSE)) file for full details.

### Written by

Madhukar Moogala, [Forge Partner Development](http://forge.autodesk.com)  @galakar