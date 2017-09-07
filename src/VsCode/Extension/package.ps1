Param(
    [Parameter(Position=1)]
    [string]$binPath,
    [Parameter(Position=2)]
    [string]$vscPath
)

$bin = Resolve-Path -Path $binPath
$vsc = Resolve-Path -Path $vscPath

#&npm install -g vsce

&cd $vsc

&md -Force server
&cd server

&xcopy /e /y /r /q "$bin\netcoreapp1.1\publish"

&copy $bin/Microsoft.R.Host.exe
&copy $bin/Microsoft.R.Host.UserProfile.exe
#&copy $bin/Microsoft.R.Host.RunAsUser.exe
&copy $bin/Microsoft.R.Host.Broker.Windows.exe
&copy $bin/Microsoft.R.Host.Broker.Config.json
&copy $bin/Microsoft.R.Platform.Windows.dll
&copy $bin/Microsoft.R.Containers.Windows.dll

&copy $bin/libuv.dll
&copy $bin/libzip-5.dll
&copy $bin/libwinpthread-1.dll
&copy $bin/zlib1.dll

&cd ..
&vsce package
