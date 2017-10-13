# R version should be substituted here. see tags for details.
ARG RTVS_VERSION=latest
FROM microsoft/rtvs:${RTVS_VERSION}
ARG USERNAME
ARG PASSWORD

# Linux user credentials
RUN useradd --create-home ${USERNAME}
RUN echo "${USERNAME}:${PASSWORD}" | chpasswd

# The default image used here comes with common packages installed. If you need more packages you can add them here.
# For a full lsit of packages see the description for the image. Uncomment the line below to install.
# RUN Rscript --vanilla -e "install.packages(c('wordcloud','rchess'), repos = 'http://cran.us.r-project.org');"

# Installing R packages from github
# RUN Rscript --vanilla -e "library(devtools);install_github('igraph/rigraph')" 

# Installing R packages that require X11
# RUN xvfb-run --server-args="-screen 0 1024x768x24" Rscript --vanilla -e "install.packages('cairoDevice', repos='http://cran.us.r-project.org')"