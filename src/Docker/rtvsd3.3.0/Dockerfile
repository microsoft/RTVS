FROM rtools/rtvsdbase-ub1604:latest

ARG DEBIAN_FRONTEND=noninteractive

RUN apt-get update --fix-missing && apt-get install -y libxml2-dev libssl-dev libcurl4-openssl-dev ed gtk2.0 build-essential libgtk2.0-dev libiodbc2-dev xvfb python-dev

RUN sh -c 'echo "deb https://cran.revolutionanalytics.com/bin/linux/ubuntu xenial/" > /etc/apt/sources.list.d/cran-r.list' \
 && apt-key adv --keyserver keyserver.ubuntu.com --recv-keys E084DAB9 \
 && apt-get update \
 && apt-get install -y r-base-core=3.3.0-2xenial0 \
 && apt-get install -y r-base-dev=3.3.0-2xenial0 \
 && apt-get install -y r-cran-rodbc
