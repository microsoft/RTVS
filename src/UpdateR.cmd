@echo off 
echo MDD Update Script
echo ------------------

setlocal

set BINARIES=%1

set SysProgramFiles=%ProgramFiles%
if NOT "%ProgramFiles(x86)%"=="" (
  set "SysProgramFiles=%ProgramFiles(x86)%"
)

set COMMON7IDE=%SysProgramFiles%\Microsoft Visual Studio 14.0\Common7\IDE
set MDDBUILD=%SysProgramFiles%\MSBuild\Microsoft\VisualStudio\v14.0\ApacheCordovaTools\vs-mda-targets
set PRIVATEASSEMBLIES=%SysProgramFiles%\Microsoft Visual Studio 14.0\Common7\IDE\PrivateAssemblies

rem ------- If not provided on the command line, use the default binaries location
if "%BINARIES%"=="" set BINARIES=%0\..\..\bin\debug

set PACKAGES=%0\..\Packages

set INSTALLROOT="%COMMON7IDE%\Extensions\Microsoft\R Tools for Visual Studio"

if /I "%ROBOCOPY%"=="" SET ROBOCOPY=robocopy /NJH /NJS /NDL /XX /W:1

echo Installing DLLs and extension stuff from %BINARIES%
echo ...to %INSTALLROOT%

rem -------- Copy the DLLs and extension stuff from the bin\debug folder to the Extension folder
rem --------
%robocopy% "%BINARIES%" "%INSTALLROOT%" extension.vsixmanifest Microsoft.VisualStudio.R.* Microsoft.R.* Microsoft.Languages.*

rem -------- Update the Visual Studio MEF cache(s)
rem --------
if exist "%COMMON7IDE%\devenv.exe" "%COMMON7IDE%\devenv.exe" /updateConfiguration
if exist "%COMMON7IDE%\VSWinExpress.exe" "%COMMON7IDE%\VSWinExpress.exe" /updateConfiguration

if exist "%COMMON7IDE%\devenv.exe" "%COMMON7IDE%\devenv.exe" /clearcache
if exist "%COMMON7IDE%\VSWinExpress.exe" "%COMMON7IDE%\VSWinExpress.exe" /clearcache

:ClearMEFCache
@echo Clearing VS MEF cache to avoid corruption from updated DLLs
IF EXIST "%localappdata%\microsoft\visualstudio" FOR /f %%i in ('dir /s /b "%localappdata%\microsoft\visualstudio\componentmodelcache"') do (rd /s /q %%i)
IF EXIST "%localappdata%\microsoft\vswinexpress" FOR /f %%i in ('dir /s /b "%localappdata%\microsoft\vsWinExpress\componentmodelcache"') do (rd /s /q %%i)
exit /b 0

:end
