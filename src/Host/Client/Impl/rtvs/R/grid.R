# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

grid_header_format <- function(x)
    if (is.na(x)) NULL else format(x)

# Given a 2D data object and indices of columns, returns a vector of row indices indicating the appropriate order
# of rows if the data is sorted by the columns specified. A positive column index indicates ascending order, and
# a positive index indicates descending order. If a column cannot be sorted (e.g. because it contains values of
# different types, or otherwise incomparable), it is ignored.
grid_order <- function(x, ...) {
    col_idxs <- c(...)

    # order() will do the heavy lifting, but we need to prepare the arguments for it.
    # Use xtfrm() to get a vector of integers that will sort in the same order as the original data -
    # this allows us to negate those integers to obtain descending order.
    # If the column is a list (and hence can contain heterogenous values that cannot be meaningfully
    # compared), xtfrm will raise an error; handle that by producing a vector of zeroes, which will
    # make that column a no-op for order().
    args <- list()
    for (col_idx in col_idxs) {
        col <- x[, abs(col_idx)]
        args[[length(args) + 1]] <- tryCatch(
            xtfrm(col) * sign(col_idx),
            error = function(e) replicate(length(col), 0))
    }

    do.call(order, args)
}

grid_data <- function(x, rows, cols, row_selector) {
    # If it's a 1D vector, turn it into a single-column 2D matrix, then process as such.
    x <- as.data.frame(x)
    d <- dim(x);

    if (missing(rows)) {
        rows <- 1:d[[1]];
    }
    if (missing(cols)) {
        cols <- 1:d[[2]];
    }

    # Row names must be retrieved before slicing the data, because slicing can change the type -
    # for example, a sliced timeseries is just a vector or matrix, and so time() no longer works.
    rn <- row.names(x);

    # Slice the row names to match the data.
    if (!missing(row_selector)) {
        rn <- rn[row_selector(x), drop = FALSE]
    }
    rn <- rn[rows, drop = FALSE]

    # Slice the data.
    if (!missing(row_selector)) {
        x <- x[row_selector(x),, drop = FALSE]
    }
    x <- x[rows, cols, drop = FALSE]

    # Process and format values column by column, then flatten the resulting list of character vectors.
    max_length <- 100 - 3
    data <- c(lapply(1:ncol(x), function(i) {
        col <- x[, i]

        # Some 2D collections (e.g. tibble) produce 2D output when sliced by column. If that happens,
        # use as.matrix to coerce it to a vector or list. Note: cannot use as.list, because that will
        # just produce a list of 1 element, which is the column itself.
        if (length(dim(col)) > 1) {
            col <- as.matrix(col)
            dim(col) <- NULL;
        }

        if (is.atomic(col)) {
            # For atomic vectors, we want to apply format() to the whole thing at once,
            # so that it can determine the number of decimal places accordingly - e.g.
            # for c(1.5, 2, 3.04), we want the output to be "1.50 2.00 3.04".
            col <- format(col, trim = TRUE, justify = "none")
        }

        lapply(col, function(s) {
            # If it's already a string, use as is (this also will be the case if format was already applied above).
            # If it's something else, try format() on this individual value.
            # If that fails (e.g. for externalptr), try str().
            # If that also fails, give up and display the value as <?>.
            if (!is.character(s) || length(s) != 1) {
                s <- tryCatch({
                    # Preserve the behavior of format() for list elements, documented as follows:
                    # If x is a list, the result is a character vector obtained by applying format.default(x, ...)
                    # to each element of the list (after unlisting elements which are themselves lists), and then
                    # collapsing the result for each element with paste(collapse = ", ").
                    paste(format.default(unlist(s), trim = TRUE, justify = "none"), collapse = ', ')
                }, error = function(e) {
                    tryCatch({
                        make_repr_str(max_length = 100)(s) 
                    }, error = function(e) {
                        "<?>"
                    })
                })
            }

            if (is.na(s)) { 'NA' }
            else if (nchar(s) <= max_length) { s }
            else { paste0(substr(s, 1, max_length), '...', collapse = '') }
        })
    }), recursive = TRUE)
    
    # Any names in the original data will flow through, but we don't want them.
    names(data) <- NULL;

    cn <- colnames(x);

    # Format row names
    x.rownames <- NULL;
    if (length(rn) > 0) {
        if (is.numeric(rn)) {
            # For numeric vectors, we want to apply format() to the whole thing at once,
            # so that it can determine the number of decimal places accordingly - e.g.
            # for c(1.5, 2, 3.04), we want the output to be "1.50 2.00 3.04".
            rn <- lapply(format(rn, trim = TRUE, justify = "none"), function(s) if(s=="NA") NULL else s)
        }
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
