FROM ubuntu:16.04

ARG DEBIAN_FRONTEND=noninteractive

RUN apt-get update \
 && apt-get install -y software-properties-common python-software-properties \
 && apt-get install -y apt-transport-https libexplain51 libzip4 libc6 lshw ssl-cert locales

RUN sh -c 'echo "deb [arch=amd64] https://apt-mo.trafficmanager.net/repos/dotnet-release/ xenial main" > /etc/apt/sources.list.d/dotnetdev.list' \
 && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 417A0893 \
 && apt-get update \
 && apt-get install -y dotnet-dev-1.0.4

RUN locale-gen "en_US.UTF-8" && dpkg-reconfigure locales