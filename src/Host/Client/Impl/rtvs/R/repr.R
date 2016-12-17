# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

# Returns a repr function, suitable for use with `describe_object`, that uses `deparse`
# to obtain the representation. Result is always a single-element character vector.
#
# If `max_length` is not NULL, the representation is trimmed if it exceeds the provided length. 
make_repr_deparse <- function(max_length = NULL) {
    function(obj) {
        paste0(collapse = '',
            if (is.null(max_length)) {
                deparse(obj)
            } else {
                # Force max length into range permitted by deparse
                cutoff <- min(max(max_length, 20), 500);
                deparse(obj, width.cutoff = cutoff, nlines = 1)
            })
    }
}

# Returns a repr function, suitable for use with `describe_object`, that uses `str` to
# obtain the representation, with special casing for single-element vectors. Result is
# always a single-element character vector; if `str` returns more than one string, only
# the first one is used.
#
# If `max_length` is not NULL, the representation is trimmed if it exceeds the provided
# length. 
#
# If `expected_length` is not NULL, it should provide an upper bound estimate on the
# length of the produced representation. The implementation uses the estimate for
# optimization purposes, to avoid reallocating the buffer as result is generated.
# 
# `overflow_suffix` is a string that is appended to the result if it didn't fit into
# `max_length`. Sufficient number of characters is discarded from the result to ensure
# that the overall length is no more than `max_length`.
make_repr_str <- function(max_length = NULL, expected_length = NULL, overflow_suffix = '...') {
    function(obj) {
        con <- memory_connection(max_length, expected_length, overflow_suffix, eof_marker = '\n');
        on.exit(close(con), add = TRUE);

        tryCatch({
            if (length(obj) == 1 && typeof(obj) != 'externalptr') {
                if (any(class(obj) == 'factor')) {
                    if (is.na(obj) && !is.nan(obj)) {
                        cat('NA', file = con);
                    } else {
                        capture.output(str(levels(obj)[[obj]], max.level = 0, give.head = FALSE), file = con);
                    }
                } else {
                    capture.output(str(obj, max.level = 0, give.head = FALSE), file = con);
                }
            } else {
                capture.output(str(obj, max.level = 0, give.head = TRUE), file = con);
            }
        }, error = function(e) {
        });

        str <- memory_connection_tochar(con);
        if (length(str) == 0) NA else str;
    }
}

repr_toString <- function(obj)
paste0(toString(obj), collapse = '')
