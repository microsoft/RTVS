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
  sapply(format(x, trim = TRUE, justify = "none"), grid.trim);
}

grid.data <- function(x, rows, cols) {
  d <- dim(x);
  if (missing(rows)) {
    rows <- 1:d[[1]];
  }
  if (missing(cols)) {
    cols <- 1:d[[2]];
  }

  # get values for column/row names and data
  onedim <- ((length(rows) == 1) || (length(cols) == 1));
  if (onedim || is.matrix(x)) {
    if (onedim) {
      if(is.list(x)) {
        data <- grid.format(x[rows]);
      } else {
        data <- grid.format(x[rows, cols]);
      }
    } else {
      data <- sapply(as.data.frame(x[rows, cols]), grid.format, USE.NAMES=FALSE);
    }
    rn <- row.names(x)[rows];
    cn <- colnames(x)[cols];
  } else {
    x.df <- as.data.frame(x)[rows, cols]
    data <- sapply(x.df, grid.format, USE.NAMES=FALSE);
    rn <- row.names(x.df);
    cn <- colnames(x.df);
  }

  # format row names
  x.rownames <- NULL;
  if (length(rn) > 0) {
    x.rownames <- sapply(rn, format, USE.NAMES = FALSE);
  }

  #format column names
  x.colnames <- NULL;
  if (!is.null(cn) && (length(cn)>0)) {
    x.colnames <- sapply(cn, format, USE.NAMES = FALSE);
  }

  # assign return value
  vp<-list();
  vp$row.start <- rows[1];
  vp$row.count <- length(rows);
  vp$row.names <- as.list(x.rownames);
  vp$col.start <- cols[1];
  vp$col.count <- length(cols);
  vp$col.names <- as.list(x.colnames);
  data.list <- list();
  for (i in data) {
     data.list[length(data.list) + 1] <- as.list(i);
  }
  vp$data<-data.list;
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

getEnvRepr <- function(env, higherThanGlobal) {
  envRepr <- NULL;

  if (higherThanGlobal) {
    nframe <- sys.nframe();
    if (nframe > 0) {
      for (i in 1:nframe) {
        frame <- sys.frame(i)
        if (identical(env, frame)) {
          envRepr <- list(name = deparse(sys.call(i)[[1]]), frameindex = i, higherThanGlobal = TRUE);
          break;
        }
      }
    }
  }

  if (is.null(envRepr)) {
    if (identical(env, baseenv())) {
      envRepr <- list(name = 'package:base', higherThanGlobal = FALSE);
    } else if (identical(env, globalenv())) {
      envRepr <- list(name = '.GlobalEnv', higherThanGlobal = FALSE);
    } else {
      envRepr <- list(name = environmentName(env), higherThanGlobal = FALSE);
    }
  }
  envRepr;
}

# return list of environment statck walking from the given environment up.
getEnvironments <- function(env) {
  if (missing(env)) {
    curEnv <- sys.frame(-1);
  } else {
    curEnv <- env;
  }
  envs <- list();

  higherThanGlobal <- TRUE;

  while (!identical(curEnv, emptyenv())) {
    repr <- getEnvRepr(curEnv, higherThanGlobal);
    if (nchar(repr$name) > 0
      && repr$name != "Autoloads"
      && repr$name != "rtvs::graphics::ide") {
      envs[[length(envs) + 1]] <- repr;
    }
    higherThanGlobal <- repr$higherThanGlobal;
    curEnv <- parent.env(curEnv);
  }
  envs;
}
