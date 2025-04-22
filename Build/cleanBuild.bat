@echo off
echo Cleaning previous build output...

REM Remove output and intermediate folders
rd /s /q _out
rd /s /q _obj

REM Optional: clean with dotnet too
dotnet clean Build.sln

echo Starting fresh build...
dotnet build Build.sln

echo Done.
pause
