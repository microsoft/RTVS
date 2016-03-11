# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

grid.trim <- function(str, max_length = 100) {
  if (nchar(str) > (100 - 3)) {
    paste(substr(str, 1, 97), '...', sep='');
  } else {
    str;
  }
}

grid.format <- function(x) {
  sapply(format(x, trim = TRUE), grid.trim);
}

grid.data <- function(x, rows, cols) {
  d <- dim(x);
  if (is.null(d) || (length(d) != 2)) {
    stop('grid.data requires two dimensional object');
  }

  # get values for column/row names and data
  if (is.matrix(x)) {
    x.df <- as.data.frame(x[rows, cols]);
    rn <- row.names(x)[rows];
    cn <- colnames(x)[cols];
  } else {
    x.df <- as.data.frame(x)[rows, cols];
    rn <- row.names(x.df);
    cn <- colnames(x.df);
  }

  #format data
  data <- sapply(x.df, grid.format, USE.NAMES=FALSE);


  # format row names
  dimnames <- 0;
  if (length(rn) > 0) {
    x.rownames <- sapply(rn, format, USE.NAMES = FALSE);
    dimnames <- dimnames + 1;
  } else {
    x.rownames <- 'dummy';
  }

  #format column names
  if (!is.null(cn) && (length(cn)>0)) {
    x.colnames <- sapply(cn, format, USE.NAMES = FALSE);
    dimnames <- dimnames + 2;
  } else {
    x.colnames <- 'dummy';
  }

  # assign return value
  vp<-list();
  vp$dimnames <- format(dimnames);
  vp$row.names <- x.rownames;
  vp$col.names <- x.colnames;
  vp$data<-data;
  vp;
}

grid.dput <- function(obj) {
    conn <- memory_connection(NA, 0x10000);
    json <- "{}";
    tryCatch({
        dput(obj, conn);
        json <- memory_connection_tochar(conn);
    }, finally = {
        close(conn);
    });
    json;
}

getEnvRepr <- function(env, startEnvLevel) {
  envRepr <- NULL;

  if (startEnvLevel > 4L) {    # if within call or unknown, try to get call frame
    nframe <- sys.nframe();
    if (nframe > 0) {
      for (i in 1:nframe) {
        frame <- sys.frame(i)
        if (identical(env, frame)) {
          envRepr <- list(name = deparse(sys.call(i)[[1]]), level = 5L, frameIndex = i);
          break;
        }
      }
    }
  }

  if (is.null(envRepr)) {
    if (identical(env, baseenv())) {
      envRepr <- list(name = 'package:base', level = 2L);
    } else if (identical(env, globalenv())) {
      envRepr <- list(name = '.GlobalEnv', level = 4L);
    } else if (identical(env, emptyenv())) {
      envRepr <- list(name = 'EmptyEnv', level = 1L);
    } else {
      envRepr <- list(name = environmentName(env), level = 3L);
    }
  }
  envRepr;
}

# return list of environment statck walking from the given environment up.
# R environment level: empty(1) > base(2) > packages(3) > global(4) > calls(5) > unknown(10)
getEnvironments <- function(env) {
  if (missing(env)) {
    curEnv <- sys.frame(-1);
  } else {
    curEnv <- env;
  }
  envs <- list();

  prevEnvLevel <- 10L;

  while (!identical(curEnv, emptyenv())) {
    repr <- getEnvRepr(curEnv, prevEnvLevel);
    if (repr$name != "Autoloads"
      && repr$name != "rtvs::graphics::ide") {
      envs[[length(envs) + 1]] <- repr;
    }
    prevEnvLevel <- repr$level;
    if (prevEnvLevel == 5L) {
      curEnv <- sys.frame(repr$frameIndex - 1);
    } else {
      curEnv <- parent.env(curEnv);
    }
  }
  envs;
}

# return variable's frame index
# look up starts from startFrameIndex and up
getFrameIndex <- function(varName, startFrameIndex) {
  if (missing(startFrameIndex)) {
    startFrameIndex <- sys.nframe() - 1;
  }
  frameIndex <- startFrameIndex;
  
  while (frameIndex >= 0) {
    if (exists(varName, frame = frameIndex, inherit = FALSE)) {
      return(frameIndex);
    }
    frameIndex <- frameIndex - 1;
  }
  
  -1L;
}

