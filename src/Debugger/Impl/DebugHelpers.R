.rtvs.eval_into <<- function(con, expr, env) {
  obj <- eval(parse(text = expr), env);
  
  con_repr <- textConnection(NULL, open = "w");
  tryCatch({
    dput(obj, con_repr);
    repr <- paste0(textConnectionValue(con_repr), collapse='');
  }, finally = {
    close(con_repr);
  });
  
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
        dput(format(call), con);
      }
      
      cat(', "filename":', file = con, sep = '');
      filename <- getSrcFilename(call, full.names = TRUE);
      if (!is.null(filename) && length(filename) == 1) {
        dput(filename, con);
      } else {
        cat('null', file = con, sep = '');
      }
      
      cat(', "linenum":', file = con, sep = '');
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
      .rtvs.eval_into(con, varname, env);
      cat('}', file = con, sep = '');
    }
    
    cat('}\n', file = con, sep = '');
    json <- paste0(textConnectionValue(con), collapse='');
  }, finally = {
    close(con);
  });
  
  json;
}


.rtvs.parse <<- function(...) {
  .rtvs.base_parse(...);
}


if (!identical(parse, .rtvs.parse)) {
  .rtvs.base_parse <<- parse;
  unlockBinding("parse", as.environment("package:base"));
  #assignInNamespace('parse', .rtvs.parse, ns = 'base', envir = as.environment('package:base'));
  #assign('parse', .rtvs.parse, envir = as.environment('package:base'));
  lockBinding("parse", as.environment("package:base"));
}
