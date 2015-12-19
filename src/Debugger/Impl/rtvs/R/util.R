call_embedded <- function(name, ...) {
  .Call(paste0('rtvs::Call.', name, collapse = ''), ..., PACKAGE = '(embedding)')
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

NA_if_error <- function(expr) {
  tryCatch(expr, error = function(e) { NA })
}

# Like toString, but guarantees that result is a single-element character vector.
force_toString <- function(obj) {
  if (is.null(obj) || (length(obj) == 1 && is.atomic(obj) && is.na(obj))) {
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

# A wrapper for dput_str that will first make name a symbol if it can be a legitimate one.
dput_symbol <- function(name) {
  if (is.character(name) && length(name) == 1 && !is.na(name) && nchar(name) > 0) {
    name <- as.symbol(name);
  }
  dput_str(name)
}

# Like str(...)[[1]], but special-cases some common types to provide a more descriptive
# and concise output, and can limit the output to a desired length.
fancy_str <- function(obj, max_length = NA, expected_length = NA, overflow_suffix = '...') {
  con <- memory_connection(max_length, expected_length, overflow_suffix, eof_marker = '\n');
  on.exit(close(con), add = TRUE);

  tryCatch({
    if (length(obj) == 1) {
      if (any(class(obj) == 'factor')) {
        if (is.na(obj)) {
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