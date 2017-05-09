========================================================================
    MAKEFILE PROJECT : Microsoft.R.Host.RunAsUser Project Overview
========================================================================

Build instructions:
Compile: g++ -std=c++14 -fexceptions -fpermissive -O0 -ggdb -I../src -c ../src/*.c*
Link: g++ -g -o Microsoft.R.Host.RunAsUser.out ./*.o -lpthread -L/usr/lib/x86_64-linux-gnu -lpam -lpam_misc
sudo chmod u+s Microsoft.R.Host.RunAsUser.out
sudo chown root:root Microsoft.R.Host.RunAsUser.out

usage:
Microsoft.R.Host.RunAsUser.out <-a|-r>
    -a : authenticate only
    -r : authenticate and run command

/////////////////////////////////////////////////////////////////////////////
