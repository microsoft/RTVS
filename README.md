# RTVS
R Tools for Visual Studio.

Building instructions

1. You must be using Visual Studio 2015 Update 1 or higher.
1. You must have C++, Web Tools and VS Extensibility components (aka VS SDK) installed.
1. You may choose to also install GitHub extensions for Visual Studio (check the option in the VS 2015 setup).
1. Install R 3.2.2 or later (either from CRAN or Microsoft R Open: https://mran.revolutionanalytics.com/open/).
1. Clone repository: 
    git clone https://github.com/Microsoft/RTVS.git or use your favorite Git GUI tool.
1. Update RHost submodule: 
    git submodule update --init --recursive
1. Open R.sln file.
1. Set Microsoft.VisualStudio.R.Package as a startup project.
1. Unload SetupBundle project since you may not be able to build it.
1. If you are not planning to build MSI setup, you can unload Setup, SetupRHost and SetupCustomActions.
1. Build the solution.
1. F5.
1. VS experimental instance should start and you should see R Tools menu.

Building Setup

1. If you are NOT planning to build burn bundle, skip to step 5.
1. Install Wix Toolset 3.7 http://wixtoolset.org/releases/.
1. Copy C:\Program Files (x86)\WiX Toolset v3.7 somewhere or simply rename the folder. For example, copy to C:\WiX.3.7.
1. Uninstall WiX 3.7.
1. Install Wix Tools 3.10 (http://wixtoolset.org/releases/).
1. In order to build RHost setup correctly you need first build Setup, then delete obj folder and build RHostSetup. 
   This is because unfortunately Wix toolset caches certain information in the obj folder and is unable to correctly
   build more than one MSI in the solution.

Building burn bundle

1. Burn bundle project has to be built from command line using Wix 3.7.
1. Open VS 2015 developer command line (Start -> All Apps -> Visual Studio 2015 -> Developer Command Prompt).
1. Go to src\SetupBundle.
1. Run (substitute for the actual 3.7 location)
   msbuild SetupBundle.wixproj /p:Configuration=Debug /p:WIX="C:\WiX.3.7"

