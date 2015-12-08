locals <- as.environment(list(breakpoints_enabled = FALSE));

# List of all active breakpoints.
# Names are filenames, values are vectors of line numbers.
breakpoints <- new.env(parent = emptyenv())

# Used to report a breakpoint. Before breaking, will check that the breakpoint is still set.
breakpoint <- function(filename, line_number) {
  line_numbers <- breakpoints[[filename]];
  if (line_number %in% line_numbers) {
    browser();
  }
}

# Adds a new breakpoint to the list of active breakpoints. If reapply=TRUE, will
# automatically reapply the new list.
add_breakpoint <- function(filename, line_number, reapply = TRUE) {
  breakpoints[[filename]] <- c(breakpoints[[filename]], line_number);
  if (reapply) {
  	  reapply_breakpoints();
  }
}

# Removes the breakpoint from the list.
remove_breakpoint <- function(filename, line_number) {
  bps <- setdiff(breakpoints[[filename]], line_number);
  if (length(bps) == 0) {
    rm(list = filename, envir = breakpoints);
  } else {
    breakpoints[[filename]] <- bps;
  }
  
  # TODO: implement reverse reapply: walk all environments looking for injected breakpoints,
  # and remove those that are no longer in the list using attr('rtvs::original_expr').
}

# Walks environments, starting from .GlobalEnv up, and injects active breakpoints
# into all functions in those environments.
reapply_breakpoints <- function() {
  if (!locals$breakpoints_enabled) {
    return();
  }
  
  (function(env, visited = NULL) {
    if (!identical(env, emptyenv()) && !any(sapply(visited, function(x) identical(x, env)))) {
      visited <- c(visited, env);

      for (name in names(env)) {
        x <- tryCatch({
          env[[name]]
        }, error = function(e) {
          NULL
        });

        if (!missing(x)) {
          if (is.function(x)) {
            bp_body <- inject_breakpoints(body(x));
            if (!is.null(bp_body)) {
              tryCatch({
                body(env[[name]]) <- bp_body;
              }, error = function(e) {
              });
            }
            # TODO: also walk parent.env of the function, in case it was defined
            # elsewhere and then copied here?
          } else if (is.environment(x)) {
            # TODO: also walk nested environments?
            #  Recall(x, visited);
          }
        }
      }
      
      Recall(parent.env(env), visited);
    }
  })(.GlobalEnv);
  
  # TODO: also walk environment chains for all loaded namespaces.
  
  # TODO: this is expensive already. If a deeper walk is performed, this might
  # have to be rewritten in native code to get acceptable perf.
}

# Given an expression or a language object, and a line number, return the steps that
# must be taken from the root of that object to get to the instruction that is located
# at that line number, or NULL if there is no such instruction. The returned value
# is an integer vector that can be directly used as an index for [[ ]].
steps_for_line_num <- function(expr, line, have_srcrefs = FALSE) {
  is_brace <- function(expr)
    typeof(expr) == 'symbol' && identical(as.character(expr), '{')
  
  if (typeof(expr) == 'language' || typeof(expr) == 'expression') {
    srcrefs <- attr(expr, 'srcref')
    for (i in seq_along(expr)) {
      srcref <- srcrefs[[i]]
      
      # Check for non-matching range. 
      if (!is.null(srcref) && (srcref[1] > line || line > srcref[3])) {
        next
      }
      
      # We're in range.  See if there's a finer division, and add it as a substep if so.
      finer <- steps_for_line_num(expr[[i]], line, have_srcrefs || !is.null(srcrefs))
      if (!is.null(finer)) {
        return(c(i, finer))
      }
      
      # If there was no subdivision, then this is the exact instruction, but only if
      # there was an srcref. However, if this is an opening curly brace for a block,
      # and parent had an srcref, then match the parent (which will be the whole block).
      if (!is.null(srcref) && (!have_srcrefs || !is_brace(expr[[i]]))) {
        return(i)
      }
    }
  }
  
  NULL
}

# Given an expression or language object, return that object with all active
# breakpoints injected into it, or NULL if no breakpoints were injected.
inject_breakpoints <- function(expr) {
  if (length(breakpoints) == 0) {
    return(NULL);
  }
  
  filename <- getSrcFilename(expr);
  if (is.null(filename) || !is.character(filename) || length(filename) != 1 || is.na(filename)) {
    return(NULL);
  }
  
  line_numbers <- breakpoints[[filename]];
  if (is.null(line_numbers)) {
    return(NULL);
  }

  changed <- FALSE;
  for (line_num in line_numbers) {
  	step <- steps_for_line_num(expr, line_num);
  	if (length(step) > 0) {
  	bp_expr <- expr[[step]];
  	original_expr <- attr(bp_expr, 'rtvs::original_expr');
  	if (is.null(original_expr)) {
  		original_expr <- bp_expr;
  	}
          
  	expr[[step]] <- substitute({.doTrace(rtvs:::breakpoint(FILENAME, LINE_NUMBER)); EXPR},
  									list(FILENAME = filename, LINE_NUMBER = line_num, EXPR = original_expr));
  	attr(expr[[step]], 'rtvs::original_expr') <- original_expr;
  	attr(expr[[step]], 'srcref') <- attr(original_expr, 'srcref');
  	changed <- TRUE;
  	}
  }
  
  if (!changed) {
    return(NULL);
  }
  
  expr
}

# Stash away the original .Internal before detouring it. In case it has already been detoured,
# check the attribute that is set to save the original first.
#original_.Internal <- attr(base::.Internal, 'rtvs::original');
#if (is.null(original_.Internal)) {
#  original_.Internal <- base::.Internal;
#}

detoured_.Internal <- function(call) {
  # Call the real thing first.
  res <- eval(substitute(.Primitive('.Internal')(call)), envir = parent.frame());

  # If it was .Internal(parse(...)), inject breakpoints into the resulting expression object.
  if (substitute(call)[[1]] == 'parse') {
    bp_res <- inject_breakpoints(res);
    if (!is.null(bp_res)) {
      res <- bp_res;
    }
  }
  
  res
}

detour_parsing <- function(env, enable) {
  on.exit({
    lockBinding('.Internal', env);  
    lockBinding('parse', env);  
    lockBinding('source', env);  
    lockBinding('sys.source', env);  
  });  
  
  unlockBinding('.Internal', env);  
  unlockBinding('parse', env);  
  unlockBinding('source', env);  
  unlockBinding('sys.source', env);  
  
  if (enable) {
    .Internal <- env$.Internal;
    attr(detoured_.Internal, 'rtvs::original') <- .Internal;
    env$.Internal <- detoured_.Internal;
  
    # The following code is a magic hack to force these three functions from
    # the same namespace as .Internal to notice the changed definition of it.
    
    ne <- new.env(parent = env);
    
    parse <- env$parse;
    attr(parse, 'rtvs::original') <- parse;
    environment(parse) <- ne;
    env$parse <- parse;
    
    source <- env$source;
    attr(source, 'rtvs::original') <- source;
    environment(source) <- ne;
    env$source <- source;
    
    sys.source <- env$sys.source;
    attr(sys.source, 'rtvs::original') <- sys.source;
    environment(sys.source) <- ne;
    env$sys.source <- sys.source;
    
    # TODO: do the same to other stuff in base that calls .Internal(parse).
  } else {
    env$.Internal <- attr(env$.Internal, 'rtvs::original');
    env$parse <- attr(env$parse, 'rtvs::original');
    env$source <- attr(env$source, 'rtvs::original');
    env$sys.source <- attr(env$sys.source, 'rtvs::original');
  }
}

# Enables or disables instrumentation that makes breakpoints work.
enable_breakpoints <- function(enable) {
  if (locals$breakpoints_enabled != enable) {
    locals$breakpoints_enabled <- enable;
    
    # Detouring needs to be done both in the package environment for base,
    # and in the namespace environment for it.
    detour_parsing(baseenv(), enable);
    detour_parsing(.BaseNamespaceEnv, enable);
    
    # TODO: also detour the same in other package environments, in search
    # paths for all loaded namespaces.
    
    if (enable) {
      reapply_breakpoints();
    }
  }
}
