NA_if_error <- function(expr) {
  tryCatch(expr, error = function(e) { NA })
}

# Like toString, but guarantees that result is a single-element character vector.
force_toString <- function(obj) {
  if (is.null(obj) || (length(obj) == 1 && is.na(obj))) {
    return('');
  }
  s <- paste0(toString(obj), collapse='');
  if (!is.character(s) || length(s) != 1 || is.na(s)) '' else s;
}

# Guarantees that result is a single-element numeric vector or NA.
force_number <- function(x) {
  if (!is.numeric(x) || length(x) != 1) NA else x;
}

# Like dput, but returns the value as string rather than printing it.
dput_str <- function(obj) {
  con <- textConnection(NULL, open = "w");
  on.exit(close(con), add = TRUE);
  
  dput(obj, con);
  paste0(textConnectionValue(con), collapse='\n')
}

# A wrapper for dput_str that will first make name a symbol if it can be a legitimate one.
dput_symbol <- function(name) {
  if (is.character(name) && length(name) == 1 && !is.na(name) && nchar(name) > 0) {
    name <- as.symbol(name);
  }
  dput_str(name)
}

# Like str, but special-cases some common types to provide a more descriptive and concise output.
fancy_str <- function(obj) {
  str <-
    if (length(obj) == 1) {
      if (any(class(obj) == 'factor')) {
        if (is.na(obj)) {
          'NA'
        } else {
          capture.output(str(levels(obj)[[obj]], max.level = 0, give.head = FALSE))
        }
      } else {
        capture.output(str(obj, max.level = 0, give.head = FALSE))
      }
    } else {
      capture.output(str(obj, max.level = 0, give.head = TRUE))
    };
  
  if (!is.character(str) || length(str) == 0) NA else str;
}
