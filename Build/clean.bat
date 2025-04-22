@echo off
cd /d "%~dp0"

:: Clean the solution
dotnet clean Build.sln

:: Remove .vs folder
if exist ".vs" rd /s /q ".vs"

:: Remove all bin and obj folders recursively
for /d /r %%d in (bin,obj) do (
    if exist "%%d" rd /s /q "%%d"
)

:: Delete DWG files from WebApi\Temp
del /q "WebApi\Temp\*.dwg"
del /q "WebApi\Temp\*.txt"

echo Clean-up completed!
pause
exit /b 0