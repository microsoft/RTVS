.rtvs.repr <<- function(obj) {
  con <- textConnection(NULL, open = "w");
  on.exit(close(con), add = TRUE);
  
  dput(obj, con);
  paste0(textConnectionValue(con), collapse='\n')
}

.rtvs.eval <<- function(expr, env, con) {
  if (missing(con)) {
    con <- textConnection(NULL, open = "w");
    on.exit(close(con), add = TRUE);
    cat('{', file = con, sep = '');
    Recall(expr, env, con);
    cat('}\n', file = con, sep = '');
    return(paste0(textConnectionValue(con), collapse=''));
  }

  err <- NULL;
  tryCatch({
    if (is.character(expr)) {
      expr <- parse(text = expr);
    }
    obj <- eval(expr, env);
  }, error = function(e) {
    err <<- e;
  });
  if (!is.null(err)) {
    cat('"error": ', file = con, sep = '');
    dput(conditionMessage(err), con);
    return();
  }

  repr <- .rtvs.repr(obj);
  cat('"value":', file = con, sep = '');
  dput(repr, con);
  
  raw_value <- tryCatch({
    paste0(toString(obj), collapse='');
  }, error = function(e) {
    repr;
  })
  
  cat(',"raw_value":', file = con, sep = '');
  dput(raw_value, con);
  
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
}

.rtvs.children <<- function(obj, env) {
  if (!missing(env)) {
    obj <- eval(parse(text = obj), env);
  }
  
  con <- textConnection(NULL, open = "w");
  on.exit(close(con), add = TRUE);
  
  cat('{', file = con, sep = '');
  commas <- 0;
  
  if (is.environment(obj)) {
    for (name in ls(obj, all.names = TRUE)) {
      if (!is.character(name) || is.na(name) || name == "") {
        next; 
      }
      
      if (commas != 0) {
        cat(',', file = con, sep = '');
      }
      commas <- commas + 1;
      
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
        .rtvs.eval(substitute(`$`(obj, name), list(name = name)), environment(), con);
      }
      
      cat('}', file = con, sep = '');
    }
  }
  
  if (isS4(obj)) {
    for (name in slotNames(class(obj))) {
      if (!is.character(name) || is.na(name)) {
        next;
      }
      
      if (commas != 0) {
        cat(',', file = con, sep = '');
      }
      commas <- commas + 1;
      
      dput(paste0('@', name, collapse = ''), con);
      cat(':{', file = con, sep = '');
     .rtvs.eval(substitute(`@`(obj, name), list(name = name)), environment(), con);
      cat('}', file = con, sep = '');
    }
  }
  
  if (is(obj, "vector")) {
    count <- length(obj);
    names <- names(obj);
    if (!is.character(names)) {
      names <- NULL;
    }
    
    for (i in 1:count) {
      if (commas != 0) {
        cat(',', file = con, sep = '');
      }
      commas <- commas + 1;
      
      accessor <- paste0('[[', as.double(i), ']]', collapse = '');

      name <- names[[i]];
      if (is.character(name) && !is.na(name) && name != '' && match(name, names) == i) {
        if (is.list(obj)) {
          accessor <- paste0('$', .rtvs.repr(as.symbol(name)), collapse = '');
        } else {
          accessor <- paste0('[[', .rtvs.repr(name), ']]', collapse = '');
        }
      }
      
      dput(accessor, con);
      cat(':{', file = con, sep = '');
      .rtvs.eval(paste0("obj", accessor, collapse = ''), environment(), con);
      cat('}', file = con, sep = '');
    }
  }

  cat('}\n', file = con, sep = '');

  paste0(textConnectionValue(con), collapse='')
}


.rtvs.traceback <<- function() {
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


.rtvs.breakpoint <<- function(filename, line_number) {
  browser()
}
