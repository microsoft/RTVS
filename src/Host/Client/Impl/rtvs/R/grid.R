# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

grid_trim <- function(str, max_length = 100) {
  if (nchar(str) > (100 - 3)) {
    paste(substr(str, 1, 97), '...', sep='');
  } else {
    str;
  }
}

grid_format <- function(x) {
  sapply(format(x, trim = TRUE, justify = "none"), grid_trim);
}

grid_sort_df <- function(x, rows, cols, row_selector) {
  if (missing(row_selector)) {
    x.df <- as.data.frame(x)[rows, cols];
  } else {
    x.df <- as.data.frame(x)
    x.df <- x.df[row_selector(x.df),][rows, cols];
  }
  x.df
}

grid_sort_vector <- function(x, vector_sort_type) {
  if (!missing(vector_sort_type) && vector_sort_type > 0) {
    if (vector_sort_type == 1) {
        x <- sort(x)
    } else {
        x <- sort(x, decreasing = TRUE)
    }
  }
  x
}

grid_data <- function(x, rows, cols, row_selector, vector_sort_type) {
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
    if (is(x, 'vector') || is.ts(x)) {
      x <- grid_sort_vector(x, vector_sort_type)
      if (length(cols) == 1) {
        data <- grid_format(x[rows]);
      } else {
        data <- grid_format(x[cols]);
      }
    } else {
      data <- grid_format(x[rows, cols]);
    }
    rn <- row.names(x)[rows];
    cn <- colnames(x)[cols];
  } else {
    x <- as.data.frame(x)
    x.df <- grid_sort_df(x, rows, cols, row_selector);
    data <- sapply(x.df, grid_format, USE.NAMES = FALSE);
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
