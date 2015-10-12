.rtvs.repr <<- function(obj) {
  con <- textConnection(NULL, open = "w");
  tryCatch({
    dput(obj, con);
    repr <- paste0(textConnectionValue(con), collapse='');
  }, finally = {
    close(con);
  });
  repr
}


.rtvs.eval_into <<- function(con, expr, env) {
  err <- NULL;
  tryCatch({
    obj <- eval(parse(text = expr), env);
  }, error = function(e) {
    err <<- e;
  });

  if (is.null(err)) {
    repr <- .rtvs.repr(obj);
    cat('"value": ', file = con, sep = '');
    dput(repr, con);
    
    raw_value <- tryCatch({
      paste0(toString(obj), collapse='');
    }, error = function(e) {
      repr;
    })
    
    cat(', "raw_value": ', file = con, sep = '');
    dput(raw_value, con);
    
    cat(', "type": ', file = con, sep = '');
    dput(typeof(obj), con);
  } else {
    cat('"error": ', file = con, sep = '');
    dput(conditionMessage(err), con);
  }
}

.rtvs.eval <<- function(expr, env) {
  con <- textConnection(NULL, open = "w");
  json <- "{}";
  tryCatch({
    cat('{', file = con, sep = '');
    .rtvs.eval_into(con, expr, env);
    cat('}\n', file = con, sep = '');
    json <- paste0(textConnectionValue(con), collapse='');
  }, finally = {
    close(con);
  });

  json
}


.rtvs.traceback <<- function() {
  calls <- sys.calls();
  nframe <- sys.nframe();
  
  con <- textConnection(NULL, open = "w");
  json <- "[]";
  tryCatch({
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
    json <- paste0(textConnectionValue(con), collapse='');
  }, finally = {
    close(con);
  });
  
  json
}


.rtvs.env_vars <<- function(env) {
  con <- textConnection(NULL, open = "w");
  json <- "{}";
  tryCatch({
    cat('{', file = con, sep = '');
    
    is_first <- TRUE;
    for (varname in ls(env)) {
      if (is_first) {
        is_first <- FALSE;
      } else {
        cat(', ', file = con, sep = '');
      }
      
      dput(varname, con);
      cat(': {', file = con, sep = '');
      
      code <- .Call(".rtvs.Call.unevaluated_promise", varname, env);
      if (!is.null(code)) {
        cat('"promise":', file = con, sep = '');
        dput(.rtvs.repr(code), con);
      } else if (bindingIsActive(varname, env)) {
        cat('"active_binding":true', file = con, sep = '');
      } else {
        .rtvs.eval_into(con, varname, env);
      }
      
      cat('}', file = con, sep = '');
    }
    
    cat('}\n', file = con, sep = '');
    json <- paste0(textConnectionValue(con), collapse='');
  }, finally = {
    close(con);
  });
  
  json;
}


.rtvs.base_source <<- NULL;


.rtvs.source <<- function(...) {
  dput(list(...))
  r <- .rtvs.base_source(...);
  r
}


.rtvs.detour_source <<- function(detour) {
  if (detour) {
    if (is.null(.rtvs.base_source)) {
      .rtvs.base_source <<- source;
      
      unlockBinding('source', baseenv());
      unlockBinding('source', .BaseNamespaceEnv);
  
      assign('source', .rtvs.source, envir = baseenv());
      assign('source', .rtvs.source, envir = .BaseNamespaceEnv);
      
      lockBinding('source', baseenv());
      lockBinding('source', .BaseNamespaceEnv);
    }
  } else {
    if (!is.null(.rtvs.base_source)) {
      unlockBinding('source', baseenv());
      unlockBinding('source', .BaseNamespaceEnv);
      
      assign('source', .rtvs.base_source, envir = baseenv());
      assign('source', .rtvs.base_source, envir = .BaseNamespaceEnv);
      
      lockBinding('source', baseenv());
      lockBinding('source', .BaseNamespaceEnv);
      
      .rtvs.base_source <<- NULL;
    }
  }
}

