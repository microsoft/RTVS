.rtvs.NA_if_error <- function(expr) {
  tryCatch(expr, error = function(e) { NA })
}

# Like toString, but guarantees that result is a single-element character vector.
.rtvs.toString <- function(obj) {
  s <- paste0(toString(obj), collapse='');
  if (!is.character(s) || length(s) != 1 || is.na(s)) '' else s;
}

# Guarantees that result is a single-element numeric vector or NA.
.rtvs.number <- function(x) {
  if (!is.numeric(x) || length(x) != 1) NA else x;
}

# Like dput, but returns the value as string rather than printing it.
.rtvs.dput <- function(obj) {
  con <- textConnection(NULL, open = "w");
  on.exit(close(con), add = TRUE);
  
  dput(obj, con);
  paste0(textConnectionValue(con), collapse='\n')
}

# A wrapper for .rtvs.dput that will first make name a symbol if it can be a legitimate one.
.rtvs.dput_symbol <- function(name) {
  if (is.character(name) && !is.na(name) && length(name) > 0 && nchar(name) > 0) {
    name <- as.symbol(name);
  }
  .rtvs.dput(name)
}

# Like str, but special-cases some common types to provide a more descriptive and concise output.
.rtvs.str <- function(obj) {
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
    }
  
  if (!is.character(str) || is.na(str) || length(str) == 0)  {
    return(NA);
  }

  str  
}

# Produces JSON from an R object according to the following mappings:
#
# NULL -> null
# TRUE -> true
# FALSE -> false
# Vector of a single non-NA number -> numeric literal
# Vector of a single non-NA string -> string literal
# Empty or multiple-element vector -> array (recursively)
# List with all elements unnamed -> array (recursively)
# List with all elements named, or environment -> object (recursively)
#
# If any element of a vector, list or environment is NA, that element is skipped.
.rtvs.toJSON <- function(data, con) {
  if (missing(con)) {
    con <- textConnection(NULL, open = "w");
    on.exit(close(con), add = TRUE);
    Recall(data, con);
    cat('\n', file = con, sep = '');
    return(paste0(textConnectionValue(con), collapse=''));
  }
  
  if (is.null(data)) {
    cat('null', file = con, sep = '');
  } else if (is.atomic(data)) {
    if (length(data) == 0) {
      cat('null', file = con, sep = '');
    } else if (length(data) == 1 && !is.na(data)) {
      if (is.logical(data)) {
        cat((if (data) 'true' else 'false'), file = con, sep = '');
      } else if (is.integer(data)) {
        dput(as.double(data), con);
      } else {
        dput(data, con);
      }
    } else {
      .rtvs.toJSON(as.list(data), con);
    }
  } else if (is.list(data) || is.environment(data)) {
    commas <- 0;
    names <- names(data);
    if (!is.environment(data) && (is.null(names) || all(is.na(names)) || all(names == ''))) {
      cat('[', file = con, sep = '');
      for (x in data) {
        if (is.atomic(x) && is.na(x)) {
          next;
        }
        
        if (commas > 0) {
          cat(', ', file = con, sep = '');
        }
        
        commas <- commas + 1;
        .rtvs.toJSON(x, con);
      }
      cat(']', file = con, sep = '');
    } else {
      if (any(is.na(names)) || any(names == '')) {
        stop("list must either have all elements named or all elements unnamed to be convertible to JSON");
      }
      
      cat('{', file = con, sep = '');
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
        .rtvs.toJSON(x, con);
      }
      cat('}', file = con, sep = '');
    }
  }
}


# Evaluates an expression in a given environment, and produces an environment describing the result.
# If obj is provided, then it is used as an evaluation result to describe. Otherwise, expr is parsed
# and evaluated in env to produce the result.
# If evaluation fails, the error is captured, and environment describing it is produced. This is the
# case even if obj is provided - if it is an expression, it will be delay-evaluated in a safe context.
# If provided, fields should be a character vector listing the fields in the output environment that
# are desired; only those fields will be included in the output (however, fields that are used to
# distinguish between various result kinds - error/value/promise/active_binding - are always included).
.rtvs.eval <- function(expr, env, kind, fields, obj) {
  res <- new.env();
  
  field <-
    if (missing(fields)) {
      function(x) TRUE;
    } else {
      function(x) x %in% fields
    }

  if (field('expression')) {
    res$expression <- expr;
  }
  
  if (field('kind')) {
    if (!missing(kind)) { 
      res$kind <- kind;
    }
  }

  err <- NULL;
  tryCatch({
    if (is.character(expr)) {
      expr <- parse(text = expr);
    }
    if (missing(obj)) {
      obj <- eval(expr, env);
    } else {
      force(obj);
    }
  }, error = function(e) {
    err <<- e;
  });

  if (!is.null(err)) {
    res$error <- .rtvs.toString(.rtvs.NA_if_error(conditionMessage(err)));
    return(res);
  }

  if (field('repr')) {
    repr <- new.env();
    
    if (field('repr.dput')) {
      repr$dput <- .rtvs.NA_if_error(.rtvs.dput(obj));
    }
    
    if (field('repr.toString')) {
      repr$toString <- .rtvs.NA_if_error(.rtvs.toString(obj));
    }
    
    if (field('repr.str')) {
      repr$str <- .rtvs.NA_if_error(.rtvs.str(obj)[1]);
    }
  
    res$repr <- repr;
  }  

  if (field('type')) {
    res$type <- .rtvs.toString(.rtvs.NA_if_error(typeof(obj)));
  }

  if (field('classes')) {
    classes <- .rtvs.NA_if_error(class(obj));
    res$classes <- lapply(classes, function(cls) {
      if (!is.character(cls) || length(cls) != 1 || is.na(cls)) NA else cls
    });
  }

  if (field('length')) {
    res$length <- .rtvs.NA_if_error(.rtvs.number(length(obj)));
  }
  
  if (field('slot_count')) {
    res$slot_count <- .rtvs.NA_if_error(.rtvs.number(length(slotNames(class(obj)))));
  }
  
  if (field('attr_count')) {
    res$attr_count <- .rtvs.NA_if_error(.rtvs.number(length(attributes(obj))));
  }
  
  if (field('name_count')) {
    res$name_count <- .rtvs.NA_if_error(.rtvs.number(length(names(obj))));
  }
  
  if (field('dim')) {
    dim <- .rtvs.NA_if_error(dim(obj));
    if (is.integer(dim) && !is.na(dim)) {
      res$dim <- dim;
    }
  }
    
  has_parent_env <- FALSE;
  if (is.environment(obj)) {
    has_parent_env <- !identical(obj, emptyenv());
    if (field('env_name')) {
      res$env_name <- .rtvs.NA_if_error(environmentName(obj));
    }
  }

  if (field('flags')) {
    res$flags <- c(
      if (is.atomic(obj)) "atomic" else NA,
      if (is.recursive(obj)) "recursive" else NA,
      if (has_parent_env) "has_parent_env" else NA
    );
  }
  
  res
}

.rtvs.children <- function(obj, env, fields, count) {
  if (!missing(env)) {
    expr <- obj;
    obj <- eval(parse(text = obj), env);
  } else {
    expr <- 'obj';
  }

  children <- vector("list", if (missing(count)) 1000 else count);
  last_child <- 0;

  if (is.environment(obj)) {
    names <- ls(obj, all.names = TRUE);
    
    if (!missing(count)) {
      names <- head(names, count);
      count <- count - length(names);
    }
    
    for (name in names) {
      name <- .rtvs.toString(name);
      if (name == '' || eval(bquote(missing(.(as.symbol(name)))), obj)) {
        next;
      }

      code <- tryCatch({
        .Call(".rtvs.Call.unevaluated_promise", name, obj)
      }, error = function(e) {
        NULL
      });

      if (!is.null(code)) {
        value <- list(promise = .rtvs.dput(code));
      } else if (bindingIsActive(name, obj)) {
        value <- list(active_binding = TRUE);
      } else {
        item_expr <- paste0(expr, '$', .rtvs.dput_symbol(name), collapse = '');
        value <- .rtvs.eval(item_expr, environment(), '$', fields, get(name, envir = obj));
      }
      
      child <- list(value);
      names(child) <- name;
      last_child <- last_child + 1;
      children[[last_child]] <- child;
    }
  }

  is_S4 <- isS4(obj);
  names <- .rtvs.NA_if_error(slotNames(class(obj)));

  if (!missing(count)) {
    names <- head(names, count);
    count <- count - length(names);
  }

  for (name in names) {
    name <- .rtvs.toString(name);
    if (name == '') {
      next;
    }
    
    accessor <- paste0('@', .rtvs.dput_symbol(name), collapse = '');
    if (is_S4) {
      slot_expr <- paste0('(', expr, ')', accessor, collapse = '')
    } else {
      slot_expr <- paste0('methods::slot((', expr, '), ', .rtvs.dput(name), ')', collapse = '')
    }
    
    value <- .rtvs.eval(slot_expr, environment(), '@', fields, slot(obj, name));
    
    child <- list(value);
    names(child) <- accessor;
    last_child <- last_child + 1;
    children[[last_child]] <- child;
  }

  if (is.atomic(obj) || is.list(obj) || is.language(obj)) {
    n <- length(obj);

    names <- names(obj);
    if (!is.character(names)) {
      names <- NULL;
    }

    if (n != 1 || !is.atomic(obj) || !(is.null(names[[1]]) || is.na(names[[1]]) || names[[1]] == '')) {
      if (!missing(count)) {
        n <- max(n, count);
        count <- count - n;
      }
  
      for (i in 1:n) {
        accessor <- paste0('[[', as.double(i), ']]', collapse = '');
        kind <- '[[';
  
        name <- .rtvs.toString(names[[i]]);
        if (name != '' && match(name, names) == i) {
          kind <- '$';
          if (is.list(obj)) {
            accessor <- paste0('$', .rtvs.dput_symbol(name), collapse = '');
          } else {
            accessor <- paste0('[[', .rtvs.dput(name), ']]', collapse = '');
          }
        }
        
        value <- .rtvs.eval(paste0(expr, accessor, collapse = ''), environment(), kind, fields, obj[[i]]);
        
        child <- list(value);
        names(child) <- accessor;
        last_child <- last_child + 1;
        children[[last_child]] <- child;
      }
    }
  }

  if (last_child == 0) list() else children[1:last_child]
}


.rtvs.traceback <- function() {
  calls <- sys.calls();
  nframe <- sys.nframe();

  frames <- vector('list', nframe);

  for (i in 1:nframe) {
    frame <- new.env();
    call <- sys.call(i);
    
    frame$call <- if (is.null(call)) NULL else .rtvs.dput(call);
    frame$filename <- .rtvs.toString(getSrcFilename(call, full.names = TRUE));
    if (frame$filename == '') {
      frame$filename <- NA;
    }
    
    frame$line_number <- .rtvs.number(getSrcLocation(call, which = 'line'));
    
    if (identical(globalenv(), sys.frame(i - 1))) {
      frame$is_global <- TRUE;
    }
    
    frames[[i]] <- frame;
  }

  .rtvs.toJSON(frames)
}


.rtvs.breakpoint <- function(filename, line_number) {
  browser()
}
