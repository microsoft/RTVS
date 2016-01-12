locals <- as.environment(list(breakpoints_enabled = FALSE));

# List of all active breakpoints.
# Names are filenames, values are vectors of line numbers.
breakpoints <- new.env(parent = emptyenv())

# Used to report a breakpoint. Before breaking, will check that the breakpoint is still set.
breakpoint <- function(filename, line_number) {
  if (locals$breakpoints_enabled) {
    line_numbers <- breakpoints[[filename]];
    if (line_number %in% line_numbers) {
      browser();
    }
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
  if (is.null(filename) || !is.character(filename) || length(filename) != 1 || is.na(filename) || identical(filename, '')) {
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

# Enables or disables instrumentation that makes breakpoints work.
enable_breakpoints <- function(enable) {
  if (locals$breakpoints_enabled != enable) {
    locals$breakpoints_enabled <- enable;
    if (enable) {
      call_embedded('set_instrumentation_callback', inject_breakpoints);
      reapply_breakpoints();
    } else {
      call_embedded('set_instrumentation_callback', NULL);
    }
  }
}

# Like parse, but returns a single `{` call object wrapping the content of the file, rather than
# an expression object containing separate calls. Consequently, when the returned object is eval'd,
# it is possible to use debug stepping commands to execute expressions sequentially.
debug_parse <- function(filename, encoding = getOption('encoding')) {
   exprs <- parse(filename, encoding = encoding);

   # Create a `{` call wrapping all expressions in the file.
   result <- quote({});
   for (i in 1:length(exprs)) {
     result[[i + 1]] <- exprs[[i]];
   }

   # Copy top-level source info.
   attr(result, 'srcfile') <- attr(exprs, 'srcfile');

   # Since the result has indices shifted by 1 due to the addition of `{` at the beginning,
   # per-line source info needs to be adjusted accordingly before copying.
   old_srcref <- attr(exprs, 'srcref');
   new_srcref <- list(attr(exprs, 'srcref')[[1]]);
   for (i in 1:length(exprs)) {
      new_srcref[[i + 1]] <- old_srcref[[i]];
   }
   attr(result, 'srcref') <- new_srcref;

   result
}

debug_source <- function(file, encoding = getOption('encoding')) {
    eval.parent(debug_parse(file, encoding))
}
