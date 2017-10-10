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

&md -Force Server
&cd server
&xcopy /e /y /r /q "$bin\netcoreapp1.1\publish"

&md -Force Broker
&xcopy /e /y /r /q "$bin\Broker" "Broker"

&md -Force Host
&xcopy /e /y /r /q "$bin\Host" "Host"
&xcopy /e /y /r /q "$bin\..\..\lib\Host" "Host"
