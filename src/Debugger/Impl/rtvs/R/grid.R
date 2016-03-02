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
  
  if ((length(rows) == 1) || (length(cols) == 1)) {
    x1 <- grid.format(x[rows, cols]);
  } else {
    x0 <- as.data.frame(x[rows, cols]);
    x1 <- sapply(x0, grid.format, USE.NAMES=FALSE);
  }

  vp<-list();

  dn <- dimnames(x);
  if (!is.null(dn) && (length(dn)==2)) {
    dnvalue <- 0;
    vp$dimnames <- dnvalue;
    if (!is.null(dn[[1]])) {
      vp$row.names <- sapply(row.names(x)[rows], format, USE.NAMES = FALSE);
      dnvalue <- dnvalue + 1;
    } else {
      vp$row.names <- 'dummy';
    }
    
    if (!is.null(dn[[2]])) {
      vp$col.names <- sapply(colnames(x)[cols], format, USE.NAMES = FALSE);
      dnvalue <- dnvalue + 2;
    } else {
      vp$col.names <- 'dummy';
    }
    vp$dimnames <- format(dnvalue);
  } else {
    vp$dimnames <- '0';
    vp$row.names <- 'dummy';  # dummy required for parser
    vp$col.names <- 'dummy';
  }

  vp$data<-x1;

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

