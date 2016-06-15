# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

grid_header_format <- function(x)
	if (is.na(x)) NULL else format(x)

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
  x <- x[rows, cols, drop = FALSE]

  # Process and format values column by column, then flatten the resulting list of character vectors.
  max_length <- 100 - 3
  data <- c(lapply(1:ncol(x), function(i) {
    lapply(format(x[,i], trim = TRUE, justify = "none"), function(s) {
      if (nchar(s) <= max_length) s else paste0(substr(s, 1, max_length), '...', collapse = '')
    })
  }), recursive = TRUE)

  # Any names in the original data will flow through, but we don't want them.
  names(data) <- NULL;

  rn <- row.names(x);
  cn <- colnames(x);

  # Format row names
  x.rownames <- NULL;
  if (length(rn) > 0) {
    x.rownames <- sapply(rn, grid_header_format, USE.NAMES = FALSE);
  }

  # Format column names
  x.colnames <- NULL;
  if (!is.null(cn) && (length(cn) > 0)) {
    x.colnames <- sapply(cn, grid_header_format, USE.NAMES = FALSE);
  }

  # assign return value
  vp <- list();
  vp$row.start <- rows[1];
  vp$row.count <- length(rows);
  vp$row.names <- as.list(x.rownames);
  vp$col.start <- cols[1];
  vp$col.count <- length(cols);
  vp$col.names <- as.list(x.colnames);
  vp$data <- as.list(data);
  vp;
}
