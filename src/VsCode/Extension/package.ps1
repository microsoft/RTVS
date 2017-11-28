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

#&npm install -g vsce

&cd $vsc

&md -Force Broker
&xcopy /e /y /r /q "$bin\Broker" "Broker"

&md -Force Host
&xcopy /e /y /r /q "$bin\Host" "Host"
&xcopy /e /y /r /q "$vscSrc\..\..\..\lib\Host" "Host"
