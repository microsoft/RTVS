.rtvs.repr <- function(obj) {
  con <- textConnection(NULL, open = "w");
  on.exit(close(con), add = TRUE);
  
  dput(obj, con);
  paste0(textConnectionValue(con), collapse='\n')
}

.rtvs.repr_symbol <- function(name) {
  if (is.character(name) && !is.na(name) && nchar(name) > 0) {
    name <- as.symbol(name);
  }
  .rtvs.repr(name)
}

.rtvs.eval <- function(expr, env, obj, use.str = FALSE, con) {
  if (missing(con)) {
    con <- textConnection(NULL, open = "w");
    on.exit(close(con), add = TRUE);
    cat('{', file = con, sep = '');
    Recall(expr, env, obj, use.str, con);
    cat('}\n', file = con, sep = '');
    return(paste0(textConnectionValue(con), collapse=''));
  }

  cat('"expression":', file = con, sep = '');
  dput(expr, con);

  err <- NULL;
  tryCatch({
    if (is.character(expr)) {
      expr <- parse(text = expr);
    }
    if (missing(obj)) {
      obj <- eval(expr, env);
    }
  }, error = function(e) {
    err <<- e;
  });

  if (!is.null(err)) {
    cat(',"error": ', file = con, sep = '');
    dput(conditionMessage(err), con);
    return();
  }

  if (use.str) {
    cat(',"value":null,"raw_value":null', file = con, sep = '');
  } else {
    repr <- .rtvs.repr(obj);
    cat(',"value":', file = con, sep = '');
    dput(repr, con);
  
    raw_value <- tryCatch({
      paste0(toString(obj), collapse='');
    }, error = function(e) {
      repr;
    })
  
    cat(',"raw_value":', file = con, sep = '');
    dput(raw_value, con);
  }
  
  cat(',"type":', file = con, sep = '');
  dput(typeof(obj), con);
  
  cat(',"class":[', file = con, sep = '');
  commas <- 0;
  for (cls in class(obj)) {
    if (!is.character(cls) || is.na(cls)) {
      next;
    }
    
    if (commas != 0) {
      cat(',', file = con, sep = '');
    }
    commas <- commas + 1;
    
    dput(cls, con);
  }
  cat(']', file = con, sep = '');
  
  cat(',"is_atomic":', (if (is.atomic(obj)) "true" else "false"), file = con, sep = '');
      
  cat(',"is_recursive":', (if (is.recursive(obj)) "true" else "false"), file = con, sep = '');
          
  cat(',"length":', file = con, sep = '');
  dput(as.double(length(obj)), con);
  
  cat(',"slot_count":', file = con, sep = '');
  dput(as.double(length(slotNames(class(obj)))), con);
  
  cat(',"attr_count":', file = con, sep = '');
  dput(as.double(length(attributes(obj))), con);
  
  cat(',"names_count":', file = con, sep = '');
  dput(as.double(length(names(obj))), con);
  
  dim <- dim(obj);
  if (is.integer(dim)) {
    cat(',"dim":[', file = con, sep = '');
    commas2 <- 0;
    for (d in dim) {
      if (commas2 != 0) {
        cat(',', file = con, sep = '');
      }
      commas2 <- commas2 + 1;
      dput(as.double(d), con);
    }
    cat(']', file = con, sep = '');
  }
  
  if (is.environment(obj)) {
    cat(',"env_name":', file = con, sep = '');
    dput(environmentName(obj), con);
    
    cat(',"has_parent_env":', (if (identical(obj, emptyenv())) "false" else "true"), file = con, sep = '');
  }

  if (use.str) {
    cat(',"str":', file = con, sep = '');
    str.repr <- "";
    
    if (length(obj) == 1) {
      if (any(class(obj) == "factor")) {
        str.repr <- if (is.na(obj)) "NA" else capture.output(str(levels(obj)[[obj]], max.level = 0, give.head = FALSE))
      } else {
        str.repr <- capture.output(str(obj, max.level = 0, give.head = FALSE))
      }
    } else {
      str.repr <- capture.output(str(obj, max.level = 0, give.head = TRUE))
    }
    
    if (length(str.repr) != 0) {
      dput(str.repr[1], con);
    } else {
      cat('""', file = con, sep = '');
    }
  }
}

.rtvs.children <- function(obj, env, use.str = FALSE, truncate.length = NULL) {
  if (!missing(env)) {
    expr <- obj;
    obj <- eval(parse(text = obj), env);
  } else {
    expr <- 'obj';
  }
  
  con <- textConnection(NULL, open = "w");
  on.exit(close(con), add = TRUE);
  
  cat('[', file = con, sep = '');
  commas <- 0;
  truncate <- !is.null(truncate.length)

  if (is.environment(obj)) {
    for (name in ls(obj, all.names = TRUE)) {
      if (!is.character(name) || is.na(name) || name == "") {
        next; 
      }

      if (truncate && commas >= truncate.length) {
        break;
      }

      if (commas != 0) {
        cat(',', file = con, sep = '');
      }
      commas <- commas + 1;

      cat('{', file = con, sep = '');
      dput(paste0('$', name, collapse=''), con);
      cat(': {', file = con, sep = '');
      
      code <- tryCatch({
        .Call(".rtvs.Call.unevaluated_promise", name, obj)
      }, error = function(e) {
        NULL
      });
      
      if (!is.null(code)) {
        cat('"promise":', file = con, sep = '');
        dput(.rtvs.repr(code), con);
      } else if (bindingIsActive(name, obj)) {
        cat('"active_binding":true', file = con, sep = '');
      } else {
        item_expr <- paste0(expr, '$', .rtvs.repr_symbol(name), collapse = '');
        .rtvs.eval(item_expr, environment(), get(name, envir = obj), use.str, con);
      }
      
      cat('}}', file = con, sep = '');
    }
  }
  
  is_S4 <- isS4(obj);
  for (name in slotNames(class(obj))) {
    if (!is.character(name) || is.na(name)) {
      next;
    }

    if (truncate && commas >= truncate.length) {
      break;
    }

    if (commas != 0) {
      cat(',', file = con, sep = '');
    }
    commas <- commas + 1;

    cat('{', file = con, sep = '');
    
    accessor <- paste0('@', .rtvs.repr_symbol(name), collapse = '');
    if (is_S4) {
      slot_expr <- paste0('(', expr, ')', accessor, collapse = '')
    } else {
      slot_expr <- paste0('methods::slot((', expr, '), ', .rtvs.repr(name), ')', collapse = '')
    }
    
    dput(accessor, con);
    cat(':{', file = con, sep = '');
   .rtvs.eval(slot_expr, environment(), slot(obj, name), use.str, con);
    cat('}}', file = con, sep = '');
  }

  if (is.atomic(obj) || is.list(obj) || is.language(obj)) {
    count <- length(obj);
    names <- names(obj);
    if (!is.character(names)) {
      names <- NULL;
    }

    for (i in 1:count) {
      if (truncate && commas >= truncate.length) {
        break;
      }

      if (commas != 0) {
        cat(',', file = con, sep = '');
      }
      commas <- commas + 1;

      cat('{', file = con, sep = '');
      accessor <- paste0('[[', as.double(i), ']]', collapse = '');

      name <- names[[i]];
      if (is.character(name) && !is.na(name) && name != '' && match(name, names) == i) {
        if (is.list(obj)) {
          accessor <- paste0('$', .rtvs.repr_symbol(name), collapse = '');
        } else {
          accessor <- paste0('[[', .rtvs.repr(name), ']]', collapse = '');
        }
      }
      
      dput(accessor, con);
      cat(':{', file = con, sep = '');
      .rtvs.eval(paste0(expr, accessor, collapse = ''), environment(), obj[[i]], use.str, con);
      cat('}}', file = con, sep = '');
    }
  }

  cat(']\n', file = con, sep = '');

  paste0(textConnectionValue(con), collapse='')
}


.rtvs.traceback <- function() {
  con <- textConnection(NULL, open = "w");
  on.exit(close(con), add = TRUE);

  calls <- sys.calls();
  nframe <- sys.nframe();

  cat('[', file = con, sep = '')
  for (i in 1:nframe) {
    call <- sys.call(i);
    
    cat(if(i == 1) '' else ',', '{"call":', file = con, sep = '')
    if (is.null(call)) {
      cat('null', file = con, sep = '');
    } else {
      dput(.rtvs.repr(call), con);
    }
    
    cat(', "filename":', file = con, sep = '');
    filename <- getSrcFilename(call, full.names = TRUE);
    if (!is.null(filename) && length(filename) == 1) {
      dput(filename, con);
    } else {
      cat('null', file = con, sep = '');
    }
    
    cat(', "line_number":', file = con, sep = '');
    linenum <- getSrcLocation(call, which = 'line');
    if (!is.null(linenum)) {
      dput(as.double(linenum), con);
    } else {
      cat('null', file = con, sep = '');
    }

    cat(', "is_global":', if (identical(globalenv(), sys.frame(i - 1))) "true" else "false", file = con, sep = '');

    cat('}', file = con, sep = '');
  }
  cat(']\n', file = con, sep = '');

  return(paste0(textConnectionValue(con), collapse=''));
}


.rtvs.breakpoint <- function(filename, line_number) {
  browser()
}
