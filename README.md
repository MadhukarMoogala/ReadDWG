# 🏗️ DWG Processing API

This solution provides an ASP.NET Core Web API for processing DWG files using Autodesk RealDWG in a decoupled job queue model.

## 🗂️ Project Structure

```
Build.sln
├── DWGExtractor/         # DWG parsing logic (runs RealDWG)
├── DWGLib/               # Shared library (optional usage)
├── WebApi/               # ASP.NET Core Web API
│   ├── Controllers/      # API controllers
│   ├── Services/         # IJobQueue, DrawingProcessor
│   ├── Models/           # DrawingJob, Response Models
├── WebApi.Tests/         # xUnit Test Project
```

## 🖼️ Architecture Diagram

## 🚀 How to Run

### Prerequisties

- Get the `Autodesk RealDWG SDK` from [Techsoft3d]([Autodesk RealDWG | Tech Soft 3D](https://www.techsoft3d.com/products/realdwg/)) vendor, they are the only RealDWG resellers of Autodesk.

- `Set REALDWGSDK= <Path of RealDWG SDK>`
  
  - example  `SET REALDWGSDK=D:\RD25\RealDWG 2025\`

- Download [.NET 8.0 ](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

- Download Visual Studio 2019 or 2022 any version, community edition also works.

### Run Tests (from `Build/` folder):

```bash
dotnet test WebApi.Tests/WebApi.Tests.csproj
```

### ▶️ Run API:

```bash
dotnet run --project WebApi/WebApi.csproj
```

## 📬 API Endpoints

- `POST /upload` – Uploads a DWG file for processing.
- `GET /status/{id}` – Get status of a processing job.
- `GET /result/{id}` – Get parsed layer/block results from DWG file.

## 🔧 How It Works

1. Uploaded file creates a `DrawingJob` in an `InMemoryJobQueue`.
2. Background processor triggers DWGExtractor as a subprocess.
3. Output is parsed into text and returned via `/result`.

## 📈 Load Testing with K6

The API is load tested using [K6](https://k6.io/), a modern load testing tool. Here's an example test script:

There are other scripts for various scenaros at `.\LoadTesting` folder.

```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export default function () {
  const file = open('./sample.dwg', 'b');
  const data = {
    file: http.file(file, 'sample.dwg')
  };

  const res = http.post('http://localhost:5000/api/drawings/upload', data);
  check(res, {
    'upload successful': (r) => r.status === 200,
  });

  sleep(1);
}
```

### Run the Test:

```bash
k6 run loadTest.js
```

This script uploads a DWG file and checks the response for correctness. You can expand it to chain calls to `/status` and `/result`.

### License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). Please see the [LICENSE]([ReadDWG/LICENSE at master · MadhukarMoogala/ReadDWG · GitHub](https://github.com/MadhukarMoogala/ReadDWG/blob/master/LICENSE)) file for full details.

### Written by

Madhukar Moogala, [Autodesk Platform Services](http://aps.autodesk.com)  @galakar