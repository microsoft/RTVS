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

grid_data <- function(x, rows, cols, row_selector) {
  # If it's a 1D vector, turn it into a single-column 2D matrix, then process as such.
  d <- dim(x);
  if (is.null(d) || length(d) == 1) {
    vp <- grid_data(matrix(x), rows, cols, row_selector)
    vp$is_1d <- TRUE;
    return(vp);
  }

  if (missing(rows)) {
    rows <- 1:d[[1]];
  }
  if (missing(cols)) {
    cols <- 1:d[[2]];
  }

  if (!missing(row_selector)) {
      x <- x[row_selector(x),, drop = FALSE]
  }
  x <- x[rows, cols]

  data <- sapply(x, grid_format, USE.NAMES = FALSE);
  rn <- row.names(x);
  cn <- colnames(x);

  # Format row names
  x.rownames <- NULL;
  if (length(rn) > 0) {
    x.rownames <- sapply(rn, format, USE.NAMES = FALSE);
  }

  # Format column names
  x.colnames <- NULL;
  if (!is.null(cn) && (length(cn)>0)) {
    x.colnames <- sapply(cn, format, USE.NAMES = FALSE);
  }

  # assign return value
  vp <- list();
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
  vp$data <- data.list;
  vp;
}
