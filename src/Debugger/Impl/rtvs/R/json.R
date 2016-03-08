# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

# Produces JSON from an R object according to the following mappings:
#
# NULL, NA, empty vector -> null
# TRUE -> true
# FALSE -> false
# Vector of a single non-NA integer or double -> numeric literal
# Vector of a single non-NA string -> string literal
# List with all elements unnamed -> array (recursively)
# List with all elements named, or environment -> object (recursively)
#
# If any element of a list or environment is NA, that element is skipped.
# Any input not covered by the rules above is considered invalid.
toJSON <- function(data, con) {
  if (missing(con)) {
    con <- memory_connection(NA, 0x10000);
    on.exit(close(con), add = TRUE);

    tryCatch({
        Recall(data, con);
    }, error = function(e) {
        warning('Error serializing to JSON:\n', data, '\n');
        stop(e)
    });

    return(memory_connection_tochar(con));
  }

  to_literal_or_array <- function() {
    if (length(data) == 0) {
      cat('null', file = con, sep = '');
    } else if (length(data) == 1) {
      # Atomic vector of length 1 is NA, boolean, number, or string literal.
      if (is.na(data)) {
        cat('null', file = con, sep = '');
      } else if (is.logical(data)) {
        cat((if (data) 'true' else 'false'), file = con, sep = '');
      } else if (is.double(data)) {
        dput(data, con);
      } else if (is.integer(data)) {
        dput(as.double(data), con);
      } else if (is.character(data)) {
	    escaped <- vapply(strsplit(data, '')[[1]], function(ch) {
			cp <- utf8ToInt(ch);
			if (cp <= 31 || cp >= 127 || ch == '\\' || ch == '"') {
				sprintf("\\u%04x", cp)
			} else {
				ch
			}
		}, FUN.VALUE = '', USE.NAMES = FALSE);
        cat('"', escaped, '"', file = con, sep = '');
      } else {
        stop("vector must be of logical, integer, double or character type to be convertible to JSON:\n\n", data);
	  }
    } else {
      stop("vector must have 0 or 1 element to be convertible to JSON:\n\n", data);
    }
  }

  to_object <- function() {
    if (any(is.na(names)) || any(names == '')) {
      stop("list must either have all elements named or all elements unnamed to be convertible to JSON:\n\n", data);
    }
    
    cat('{', file = con, sep = '');
    commas <- 0;
    
    for (name in names) {
      x <- data[[name]];
      if (length(x) == 1 && is.atomic(x) && is.na(x)) {
        next;
      }
      
      if (commas > 0) {
        cat(', ', file = con, sep = '');
      }
      commas <- commas + 1;
      
      dput(name, con);
      cat(': ', file = con, sep = '');
      toJSON(x, con);
    }
    
    cat('}', file = con, sep = '');
  }
  
  to_array <- function() {
    cat('[', file = con, sep = '');
    commas <- 0;
    
    for (x in data) {
      if (is.atomic(x) && is.na(x)) {
        next;
      }
      
      if (commas > 0) {
        cat(', ', file = con, sep = '');
      }
      
      commas <- commas + 1;
      toJSON(x, con);
    }
    
    cat(']', file = con, sep = '');
  }

  if (is.null(data)) {
    cat('null', file = con, sep = '');
  } else if (is.atomic(data)) {
    to_literal_or_array();
  } else if (is.list(data) || is.environment(data)) {
    names <- names(data);
    # If it's an environment, it's an object.
    # If it's a list, then it's an object if all elements have names, and an array if
    # none of the elements have names (anything in between is considered invalid input).
    if (!is.environment(data) && (is.null(names) || all(is.na(names)) || all(names == ''))) {
      to_array();
    } else {
      to_object();
    }
  }
}
