# Contributing to RTVS

## Code of Conduct
This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact
[opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Contributor License Agreement
If your contribution is large enough, you will be asked to sign the Microsoft CLA (the CLA bot will tell you if it's necessary).

## Development

### Prerequisites

1. Visual Studio 2017 [Preview](https://www.visualstudio.com/vs/preview/) or at least the latest available release. 
You must install C++, .NET Desktop, .NET Core (currently 1.1) and VS Extensibility components (VS SDK).
**Release/1_0** *is the last branch buildable with Visual Studio 2015. Update 3 is required.*

2. MSYS2. See instructions at the [R-Host submodule](https://github.com/Microsoft/R-Host/blob/master/BUILDING-WIN32.md).

3. R 3.4.0 or later; either one of:
   - [CRAN R](https://cran.r-project.org/bin/windows/);
   - [Microsoft R Open](https://mran.revolutionanalytics.com/open/).

4. [Wix Tools 3.11](http://wixtoolset.org/releases/) (only needed if you want to build the remote services installer for Windows).

### Getting the source code

This repository uses git submodules for some of its dependencies, so you will need to clone it with `--recursive` command line
switch to obtain everything that is needed for a successful build:

```shell
git clone --recursive https://github.com/Microsoft/RTVS.git
```

The remaining dependencies are referenced as NuGet packages, and will be automatically downloaded by VS during the build.


### Building and running the product
1. Open `R.sln` solution file in Visual Studio 2017.
1. Set `Microsoft.VisualStudio.R.Package` as a startup project.
1. If you are not planning to build the installer MSI (see next section), you can also unload `SetupRemote` and `SetupRHost` projects.
1. Build the solution. Note that this will _not_ build `Setup` by default.
1. Start Debugging (F5).
1. VS experimental instance should start, and you should see "R Tools" entry in the main menu.

### Building the remote services installer
1. Build `Setup` project specifically (right-click on it in Solution Explorer and select "Build").
1. Look for the MSI that it generates under `bin`. Running it will install the product.
