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

grid.sort.df <- function(x, rows, cols, df.sort.expression) {
  if(missing(df.sort.expression)) {
    x.df <- as.data.frame(x)[rows, cols];
  } else {
    x.df <- as.data.frame(x)
    x.df <- x.df[eval(parse(text = df.sort.expression)),][rows, cols];
  }
  x.df
}

grid.sort.vector <- function(x, vector.sort.type) {
  if(!missing(vector.sort.type) && vector.sort.type > 0) {
    if(vector.sort.type == 1) {
        x <- sort(x)
    } else {
        x <- sort(x, decreasing = TRUE)
    }
  }
  x
}

grid.data <- function(x, rows, cols, df.sort.expression, vector.sort.type) {
  d <- dim(x);
  if (missing(rows)) {
    rows <- 1:d[[1]];
  }
  if (missing(cols)) {
    cols <- 1:d[[2]];
  }

  # get values for column/row names and data
  if (length(rows) == 1 || length(cols) == 1) {
    # one-dimension objects
    if(is(x, 'vector') || is.ts(x)) {
      x <- grid.sort.vector(x, vector.sort.type)
      if(length(cols) == 1) {
        data <- grid.format(x[rows]);
      } else {
        data <- grid.format(x[cols]);
      }
    } else {
      data <- grid.format(x[rows, cols]);
    }
    rn <- row.names(x)[rows];
    cn <- colnames(x)[cols];
  } else {
    x <- as.data.frame(x)
    x.df <- grid.sort.df(x, rows, cols, df.sort.expression);
    data <- sapply(x.df, grid.format, USE.NAMES = FALSE);
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
