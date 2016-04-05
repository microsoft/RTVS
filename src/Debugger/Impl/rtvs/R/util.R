# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

call_embedded <- function(name, ...) {
  .Call(paste0('Microsoft.R.Host::Call.', name, collapse = ''), ..., PACKAGE = '(embedding)')
}

external_embedded <- function(name, ...) {
  .External(paste0('Microsoft.R.Host::External.', name, collapse = ''), ..., PACKAGE = '(embedding)')
}

send_message <- function(name, ...) {
	call_embedded('send_message', name, list(...))
}

send_message_and_get_response <- function(name, ...) {
	call_embedded('send_message_and_get_response', name, list(...))
}

memory_connection <- function(max_length = NA, expected_length = NA, overflow_suffix = '', eof_marker = '') {
  call_embedded('memory_connection', max_length, expected_length, overflow_suffix, eof_marker)
}

memory_connection_overflown <- function(con) {
  call_embedded('memory_connection_overflown', con)
}

memory_connection_tochar <- function(con) {
  call_embedded('memory_connection_tochar', con)
}

unevaluated_promise <- function(name, env) {
  call_embedded("unevaluated_promise", name, env)
}

is_missing <- function(name, env) {
  call_embedded("is_missing", name, env)
}

is_rdebug <- function(obj) {
  call_embedded("is_rdebug", obj)
}

set_rdebug <- function(obj, debug) {
  call_embedded("set_rdebug", obj, debug)
}

browser_set_debug <- function(n = 1, skip_toplevel = 0) {
  call_embedded("browser_set_debug", n, skip_toplevel)
}

toJSON <- function(obj) {
  call_embedded("toJSON", obj)
}

NA_if_error <- function(expr) {
  tryCatch(expr, error = function(e) { NA })
}

# Like toString, but guarantees that result is a single-element character vector.
force_toString <- function(obj) {
  if (is.null(obj) || (length(obj) == 1 && is.atomic(obj) && is.na(obj) && !is.nan(obj))) {
    return('');
  }
  s <- paste0(toString(obj), collapse='');
  if (!is.character(s) || length(s) != 1 || is.na(s)) '' else s;
}

# Guarantees that result is a single-element numeric vector or NA.
force_number <- function(x) {
  if (!is.numeric(x) || length(x) != 1) NA else x;
}

# Like dput, but returns the value as string rather than printing it, and can limit
# the output to a desired length.
dput_str <- function(obj, max_length = NA, expected_length = NA, overflow_suffix = '...') {
  con <- memory_connection(max_length, expected_length, overflow_suffix);
  on.exit(close(con), add = TRUE);
  
  tryCatch({
    dput(obj, con);
  }, error = function(e) {
  });
  
  # Strip leading and trailing whitespace - it is never significant, and there's always
  # at least a trailing '\n' that dput always outputs.
  gsub("^\\s+|\\s+$", "", memory_connection_tochar(con))
}

# Like deparse, but always returns a single string.
deparse_str <- function(x)
    paste0(deparse(x), collapse = '')

# Makes a symbol token (properly quoted with backticks if necessary) out of a symbol or a string.
symbol_token <- function(name) {
  s <- force_toString(name);

  # If it's an empty string, it's not a valid symbol, even if quoted.
  if (identical(s, '')) {
  	  return(NULL);
  }

  # If it's a valid identifier, it's good to go as is. Because the definition of identifier in R
  # is locale-dependent, be conservative and match ASCII only; excessive quoting is always safe.
  if (grepl('^[A-Za-z_.][A-Za-z0-9_.]*$', name)) {
  	  return(s);
  }

  # Deparse it - this will take care of all the necessary escaping for everything other than
  # backticks, but will also put double quotes around that we'll remove later.
  s <- deparse_str(force_toString(s));

  # Escape any backticks.
  s <- gsub('`', '\\`', s, fixed = TRUE);

  # Replace surrounding quotes with backticks.
  paste0('`', substr(s, 2, nchar(s) - 1), '`', collapse = '')
}

# Like str(...)[[1]], but special-cases some common types to provide a more descriptive
# and concise output, and can limit the output to a desired length.
fancy_str <- function(obj, max_length = NA, expected_length = NA, overflow_suffix = '...') {
  con <- memory_connection(max_length, expected_length, overflow_suffix, eof_marker = '\n');
  on.exit(close(con), add = TRUE);

  tryCatch({
    if (length(obj) == 1) {
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

# Like eval, but will not enter Browse mode if env has its debug bit set
# (e.g. when it has just been stepped into).
safe_eval <- function(expr, env) {
    debug <- is_rdebug(env);
    tryCatch({
        set_rdebug(env, FALSE);
        eval(expr, env)
    }, finally = {
        set_rdebug(env, debug);
    });
}