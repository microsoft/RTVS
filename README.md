# R Tools for Visual Studio

THANK YOU for checking out this early version of R Tools for Visual Studio (RTVS). We’ve decided to make it available early so that we’ll have more time to address your feedback. As such, there are bugs and missing features, so please beware! This release is meant for evaluation purposes only and not for production use.

If you **already** have VS2015 with Update 1 installed and R installed, you can download RTVS from the link below - but we highly recommend following the [Installation guide](http://microsoft.github.io/RTVS-docs/installation.html):

### Installer: [Download R Tools for Visual Studio](https://aka.ms/rtvs-current)

### Docs: [RTVS Documentation](http://microsoft.github.io/RTVS-docs/)

<br>
<hr>
<br>


## Building instructions

1. You must be using Visual Studio 2015 Update 1 or higher.
1. You must have C++, Web Tools and VS Extensibility components (aka VS SDK) installed.
1. You may also choose to install GitHub extensions for Visual Studio (check the option in the VS 2015 setup).
1. Install R 3.2.2 or later (either from [CRAN](https://cran.r-project.org/bin/windows/) or [Microsoft R Open](https://mran.revolutionanalytics.com/open/)).
1. Clone repository:  
`git clone https://github.com/Microsoft/RTVS.git`  
(or use your favorite Git GUI tool.)
1. Update [R-Host](https://github.com/Microsoft/R-Host) submodule:  
`git submodule update --init --recursive`
1. Open `R.sln` solution file in Visual Studio.
1. Set `Microsoft.VisualStudio.R.Package` as a startup project.
1. Unload `SetupBundle` project since you will not be able to build it.
1. If you are not planning to build MSI setup, you can unload `Setup`, `SetupRHost` and `SetupCustomActions`.
1. Build the solution.
1. Start Debugging (F5).
1. VS experimental instance should start, and you should see "R Tools" entry in the main menu.

## Building Setup

1. Install [Wix Tools 3.10](https://wix.codeplex.com/releases/view/617257).
1. Build `Setup` project.
1. Delete the `obj` folder.
1. Build `RHostSetup` project.  
   (This is necessary because the WiX toolset caches certain information in the `obj` folder, and is unable to correctly
   build more than one MSI project in the solution at once.
