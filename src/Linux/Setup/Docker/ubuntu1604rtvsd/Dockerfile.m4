FROM rtvsinternal.azurecr.io/ub1604base:latest

ADD ./RTVS_PKG_NAME /tmp
RUN dpkg -i /tmp/RTVS_PKG_NAME && apt-get -f install
RUN cp /tmp/server.pfx /etc/rtvs
EXPOSE 5444