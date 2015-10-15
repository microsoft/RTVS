.rtvs.datainspect.print_into <<- function(con, obj, name, add.children, first.hundred) {
  cat('{', file = con, sep = '');

  obj.type = typeof(obj)
  obj.class = class(obj)
  obj.length = length(obj)

  if (obj.length == 1)
    repr <- capture.output(str(obj, max.level = 0, give.head = FALSE))
  else
    repr <- capture.output(str(obj, max.level = 0, give.head = TRUE))

  cat('"name": ', file = con, sep = '');
  dput(name, file = con);
  cat(',', file = con, sep = '');
  cat('"class": "', file = con, sep = '');
  cat(obj.class, file = con);
  cat('"', file = con, sep = '');
  cat(',"value": ', file = con, sep = '');
  dput(repr[1], file = con);
  cat(',"type": ', file = con, sep = '');
  dput(obj.type, file = con);
  cat(',"length": ', file = con, sep = '');
  
  cat(obj.length, file = con, sep='');

  if (((obj.length > 1)||(typeof(obj)=="list")) && add.children){
    cat(',', file = con, sep = '');
    .rtvs.datainspect.append_children(con, obj, first.hundred=first.hundred)
  }
  
  cat('}\n', file = con, sep = '');
}

.rtvs.datainspect.append_children <<- function(con, obj, first.hundred=FALSE) {
  cat('"children": ', file = con, sep = '');
  
  l <- 0
  varnams <- vector("character", 0)
  if (is.environment(obj)) {
    varnames <- names(as.list(obj, all.names=FALSE))
    l<-length(varnames)
  }  else {
    varnames <- names(obj)
    l<-length(obj)
  }

  cat('{"total": ', file = con, sep = '');
  cat(l, file = con);

  begin<-1;

  if (first.hundred) end<-min(l, begin+99) else end <- l
  
  cat(',"begin": ', file = con, sep = '');
  cat(begin, file = con, sep = '');
  cat(',"end": ', file = con, sep = '');
  cat(end, file = con, sep = '');

  cat(',"variables": ', file = con, sep = '');
  cat('[', file = con, sep = '');
    
  if (is.null(varnames)) {
    if (l > 0) {
      is_first <- TRUE;
        for(i in begin:end) {
        if (is_first) {
          is_first <- FALSE;
        } else {
          cat(', ', file = con, sep = '');
        }
        .rtvs.datainspect.print_into(con, obj[[i]], gettextf("[[%s]]", i), add.children=TRUE, first.hundred=TRUE);
      }
    }
  } else {
    varnames <- varnames[begin:end]
    is_first <- TRUE;
    index <- 1
    for(varname in varnames) {
      if (is_first) {
        is_first <- FALSE;
      } else {
        cat(', ', file = con, sep = '');
      }
      if (varname == "") {
        .rtvs.datainspect.print_into(con, obj[[index]], gettextf("[[%s]]", index), add.children=TRUE, first.hundred=TRUE);
      } else {
        .rtvs.datainspect.print_into(con, obj[[varname]], varname, add.children=TRUE, first.hundred=TRUE);
      }
      index <- index + 1
    }
  }
  cat(']}\n', file = con, sep = '');
}

.rtvs.datainspect.eval_into <<- function(con, expr, env) {
    obj <- eval(parse(text = expr), env);
    
    .rtvs.datainspect.print_into(con, obj, expr, add.children=TRUE, first.hundred=FALSE);
}

.rtvs.datainspect.eval <<- function(expr, env) {
    con <- textConnection(NULL, open = "w");
    json <- "{}";
    tryCatch({
        .rtvs.datainspect.eval_into(con, expr, env);
        json <- textConnectionValue(con);
    }, finally = {
        close(con);
    });
    return(paste(json, collapse=''));
}

.rtvs.datainspect.env_vars <<- function(env) {
    con <- textConnection(NULL, open = "w");
    json <- "{}";
    tryCatch({
        cat('[', file = con, sep = '');
        is_first <- TRUE;
        for (varname in ls(env)) {
            if (is_first) {
                is_first <- FALSE;
            }
            else {
                cat(', ', file = con, sep = '');
            }
            cat('{', file = con, sep = '');
            .rtvs.datainspect.eval_into(con, varname, env);
            cat('}', file = con, sep = '');
        }
        cat(']\n', file = con, sep = '');
        json <- textConnectionValue(con);
    }, finally = {
        close(con);
    });
    
    return(paste(json, collapse=''))
}
