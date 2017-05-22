========================================================================
    MAKEFILE PROJECT : Microsoft.R.Host.RunAsUser Project Overview
========================================================================

Build instructions:
Compile[Debug]: g++ -std=c++14 -fexceptions -fpermissive -O0 -ggdb -I../src -I../lib/picojson -c ../src/*.c*
Compile[Release]: g++ -std=c++14 -fexceptions -fpermissive -ggdb -I../src -I../lib/picojson -c ../src/*.c*
Link: g++ -g -o Microsoft.R.Host.RunAsUser ./*.o -lpthread -L/usr/lib/x86_64-linux-gnu -lpam
sudo chmod u+s Microsoft.R.Host.RunAsUser
sudo chown root:root Microsoft.R.Host.RunAsUser

/////////////////////////////////////////////////////////////////////////////
