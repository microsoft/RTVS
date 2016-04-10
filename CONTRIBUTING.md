# Contributing to RTVS

## Code of Conduct
This project's code of conduct can be found in the [CODE_OF_CONDUCT.md file](CODE_OF_CONDUCT.md)
(v1.4.0 of the http://contributor-covenant.org/ CoC).

## Contributor License Agreement
If your contribution is large enough, you will be asked to sign the Microsoft CLA (the CLA bot will tell you if it's necessary).

## Development

### Prerequisites

1. Visual Studio 2015 Update 1 or higher.
   - You must have C++, Web Tools, and VS Extensibility components (VS SDK) installed.
1. R 3.2.2 or later; either one of:
   - [CRAN R](https://cran.r-project.org/bin/windows/);
   - [Microsoft R Open](https://mran.revolutionanalytics.com/open/).
1. [Wix Tools 3.10](https://wix.codeplex.com/releases/view/617257) (only needed if you want to build the installer).

### Getting the source code

This repository uses git submodules for some of its dependencies, so you will need to clone it with `--recursive` command line
switch to obtain everything that is needed for a successful build:

```shell
git clone --recursive https://github.com/Microsoft/RTVS.git
```

The remaining dependencies are referenced as NuGet packages, and will be automatically downloaded by VS during the build.

### Building and running the product

1. Open `R.sln` solution file in Visual Studio.
1. Set `Microsoft.VisualStudio.R.Package` as a startup project.
1. Unload `SetupBundle` project - it has some internal dependencies, and cannot be built by third parties.
1. If you are not planning to build the installer MSI (see next section), you can also unload `Setup`, `SetupRHost` and `SetupCustomActions` projects.
1. Build the solution. Note that this will _not_ build `Setup` by default.
1. Start Debugging (F5).
1. VS experimental instance should start, and you should see "R Tools" entry in the main menu.

### Building the installer
1. Build `Setup` project specifically (right-click on it in Solution Explorer and select "Build").
1. Look for the MSI that it generates under `bin`. Running it will install the product.
