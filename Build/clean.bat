@echo off

:: Clean the solution
dotnet clean Build.sln

:: Remove temporary and build files
rd /s /q _obj
rd /s /q _out
rd /s /q .vs
rd /s /q **\bin
rd /s /q **\obj

echo Clean-up completed!
