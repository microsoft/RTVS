FROM rtools/rtvsdbase-ub1604:latest

ARG DEBIAN_FRONTEND=noninteractive

RUN apt-get update --fix-missing && apt-get install -y libxml2-dev libssl-dev libcurl4-openssl-dev ed gtk2.0 build-essential libgtk2.0-dev libiodbc2-dev xvfb python-dev

RUN sh -c 'echo "deb https://cran.revolutionanalytics.com/bin/linux/ubuntu xenial/" > /etc/apt/sources.list.d/cran-r.list' \
 && apt-key adv --keyserver keyserver.ubuntu.com --recv-keys E084DAB9 \
 && apt-get update \
 && apt-get install -y r-base-core=3.2.5-1xenial \
 && apt-get install -y r-base-dev=3.2.5-1xenial \
 && apt-get install -y r-cran-rodbc

RUN Rscript --vanilla -e "install.packages('devtools', repos = 'http://cran.us.r-project.org');"
RUN xvfb-run --server-args="-screen 0 1024x768x24" Rscript --vanilla -e "install.packages('cairoDevice', repos='http://cran.us.r-project.org')"
# RUN Rscript --vanilla -e "library(devtools);install_github('igraph/rigraph')" 

# Some packages need shiny like 'htmlwidgets'
RUN Rscript --vanilla -e "install.packages('shiny', repos = 'http://cran.us.r-project.org');"

# Install commonly used packages
RUN Rscript --vanilla -e "install.packages(c('argparse','assertthat','AzureML','backports','base64enc','BH','bindr','bindrcpp','bitops','boot','brew','brglm','car','caret','caTools','checkpoint','class','cluster','codetools','coin','colorspace','combinat','commonmark','config','corpcor','crayon','Cubist','curl','data.table','DBI','dbplyr','desc','DescTools','devtools','df2json','DiagrammeR','dichromat','digest','doParallel','dplyr','DT','e1071','earth','ellipse','evaluate','fastICA','findpython','foreach','foreign','formatR','gam','gbm','gdata','getopt','ggplot2','git2r','glmnet','glue','gridExtra','gtable','gtools','h2o','highr','hms','htmltools','htmlwidgets','httpuv','httr','influenceR','ipred','IRdisplay','irlba','ISwR','iterators','jsonlite','kernlab','KernSmooth','klaR','knitr','labeling','lattice','lava','lazyeval','lme4','lubridate','magrittr','manipulate','markdown','MASS','Matrix','MatrixModels','mda','memoise','mgcv','mime','miniCRAN','miniUI','minqa','mlbench','ModelMetrics','modeltools','multcomp','munsell','mvtnorm','nlme','nloptr','nnet','numDeriv','openssl','pamr','party','pbdZMQ','pbkrtest','pkgconfig','plogr','plotmo','plotrix','pls','plyr','png','praise','pROC','prodlim','profileModel','proto','proxy','purrr','quantreg','R6','randomForest','RANN','rappdirs','rattle','RColorBrewer','Rcpp','RcppEigen','RCurl','readr','repr','reshape2','rgexf','RGtk2','rjson','RJSONIO','rlang','rmarkdown','Rook','roxygen2','rpart','rprojroot','rPython','rstudioapi','RUnit','sandwich','scales','sourcetools','sparklyr','SparseM','spatial','spls','statmod','stringi','stringr','strucchange','subselect','superpc','survival','TeachingDemos','testthat','TH.data','tibble','tidyr','tidyverse','uuid','viridis','viridisLite','visNetwork','whisker','withr','xgboost','XML','xml2','xtable','yaml','zoo'), repos = 'http://cran.us.r-project.org')"