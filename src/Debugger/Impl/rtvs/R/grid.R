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

grid.data <- function(expr, env, rows, cols) {
  x <- expr;

  d <- dim(x);
  if (is.null(d) || (length(d) != 2)) {
    stop('grid.data requires two dimensional object');
  }

  # get values for column/row names and data
  if (is.matrix(x)) {
    if ((length(rows) == 1) || (length(cols) == 1)) { 
      data <- grid.format(x[rows, cols]);
    } else {
      x.df <- as.data.frame(x[rows, cols]);
      data <- sapply(x.df, grid.format, USE.NAMES=FALSE);  
    }

    rn <- row.names(x)[rows];
    cn <- colnames(x)[cols];
  } else {
    x.df <- as.data.frame(x)[rows, cols];
    data <- sapply(x.df, grid.format, USE.NAMES=FALSE);
    rn <- row.names(x.df);
    cn <- colnames(x.df);
  }

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

