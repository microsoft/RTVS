Param(
    [Parameter(Position=1)]
    [string]$binPath,
    [Parameter(Position=2)]
    [string]$vscPath,
    [Parameter(Position=3)]
    [string]$vscSrcPath
)

$bin = Resolve-Path -Path $binPath
$vsc = Resolve-Path -Path $vscPath
$vscSrc = Resolve-Path -Path $vscSrcPath

&cd $vsc

&md -Force Broker
&xcopy /e /y /r /q "$bin\Broker" "Broker"

&md -Force Host
&xcopy /e /y /r /q "$bin\Host" "Host"
&xcopy /e /y /r /q "$vscSrc\..\..\..\lib\Host" "Host"

$vsce1 = "C:\Users\" + $env:UserName + "." + $env:UserDomain + "\AppData\Roaming\npm"
$vsce2 = "C:\Users\" + $env:UserName + "\AppData\Roaming\npm"
$env:Path=$env:Path + ";C:\Program Files\nodejs;" + $vsce1 + ";" + $vsce2

&npm install -g vsce
&npm install

&vsce package
