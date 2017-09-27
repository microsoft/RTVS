FROM rtools/rtvsdbase-ub1604:latest

ARG DEBIAN_FRONTEND=noninteractive

RUN apt-get install -y wget

RUN mkdir /tmp/mro && cd /tmp/mro && \
  wget -O microsoft-r-open.tar.gz https://mran.microsoft.com/install/mro/3.3.3/microsoft-r-open-3.3.3.tar.gz && \
  tar -xvzf microsoft-r-open.tar.gz

RUN /tmp/mro/microsoft-r-open/install.sh --accept-mkl-eula --silent