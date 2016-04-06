# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

# Fills the provided environment 'res' with information about object 'obj', such as:
# - various textual representations
# - typeof and classes
# - length, number of slots, number of attributes, and number of names
# - dimensions
# - environment name
# - various assorted flags
#
# If provided, fields should be a character vector listing the fields in the output environment that
# are desired; only those fields will be included in the output.
describe_object <- function(obj, res, fields, repr_max_length = NA) {
  if (missing(res)) {
    res <- new.env();
  }   

  field <-
    if (missing(fields)) {
      function(x) TRUE;
    } else {
      function(x) x %in% fields
    }
  
  if (field('repr')) {
    repr <- new.env();
    
    if (field('repr.deparse')) {
      repr$deparse <- paste0(collapse = '', NA_if_error(
        if (is.na(repr_max_length)) {
            deparse(obj)
        } else {
            # Force max length into range permitted by deparse
            cutoff <- min(max(repr_max_length, 20), 500);
            deparse(obj, width.cutoff = cutoff, nlines = 1)
        }))
    }
    
    if (field('repr.toString')) {
      repr$toString <- NA_if_error(paste0(toString(obj), collapse = ''));
    }
    
    if (field('repr.str')) {
      repr$str <- NA_if_error(fancy_str(obj, repr_max_length, 0x100));
    }
  
    res$repr <- repr;
  }  

  if (field('type')) {
    res$type <- force_toString(NA_if_error(typeof(obj)));
  }

  if (field('classes')) {
    classes <- NA_if_error(class(obj));
    res$classes <- lapply(classes, function(cls) {
      if (!is.character(cls) || length(cls) != 1 || is.na(cls)) NA else cls
    });
  }

  if (field('length')) {
    res$length <- NA_if_error(force_number(length(obj)));
  }
  
  if (field('slot_count')) {
    res$slot_count <- NA_if_error(force_number(length(slotNames(class(obj)))));
  }
  
  if (field('attr_count')) {
    res$attr_count <- NA_if_error(force_number(length(attributes(obj))));
  }
  
  if (field('name_count')) {
    res$name_count <- NA_if_error(force_number(length(names(obj))));
  }
  
  if (field('dim')) {
    dim <- NA_if_error(dim(obj));
    if (is.integer(dim) && !anyNA(dim)) {
      res$dim <- as.list(dim);
    }
  }
    
  has_parent_env <- FALSE;
  if (is.environment(obj)) {
    has_parent_env <- !identical(obj, emptyenv());
    if (field('env_name')) {
      res$env_name <- NA_if_error(environmentName(obj));
    }
  }

  if (field('flags')) {
    res$flags <- list(
      if (is.atomic(obj)) "atomic" else NA,
      if (is.recursive(obj)) "recursive" else NA,
      if (has_parent_env) "has_parent_env" else NA
    );
  }
  
  res
}


# Evaluates an expression in a given environment, and produces an environment describing the result.
# If obj is provided, then it is used as an evaluation result to describe. Otherwise, expr is parsed
# and evaluated in env to produce the result.
# If evaluation fails, the error is captured, and environment describing it is produced. This is the
# case even if obj is provided - if it is an expression, it will be delay-evaluated in a safe context.
# If provided, fields should be a character vector listing the fields in the output environment that
# are desired; only those fields will be included in the output (however, fields that are used to
# distinguish between various result kinds - error/value/promise/active_binding - are always included).
eval_and_describe <- function(expr, env, kind, fields, obj, repr_max_length = NA) {
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
      obj <- safe_eval(expr, env);
    } else {
      force(obj);
    }
  }, error = function(e) {
    err <<- e;
  });

  if (is.null(err)) {
    describe_object(obj, res, fields, repr_max_length);
  } else {
    res$error <- force_toString(NA_if_error(conditionMessage(err)));
  }

  res
}

describe_children <- function(obj, env, fields, count = NULL, repr_max_length = NA) {
  if (!missing(env)) {
    expr <- obj;
    obj <- safe_eval(parse(text = obj), env);
  } else {
    expr <- 'obj';
  }

  # Preallocate to avoid growing the list on every new child.
  children <- vector("list", if (is.null(count)) 1000 else count);
  last_child <- 0;

  process_env <- function(fields) {
    names <- ls(obj, all.names = TRUE);
    
    if (!is.null(count)) {
      names <- head(names, count);
      count <<- count - length(names);
    }
    
    for (name in names) {
      name <- force_toString(name);
      # If a binding has an empty name, or it wasn't a string, it cannot be accessed, so ignore it.
      if (name == '') {
        next;
      }

      name_sym <- symbol_token(name);

      # Check if it's a promise, and retrieve the promise expression if it is.
      code <- tryCatch({
        unevaluated_promise(name, obj)
      }, error = function(e) {
        NULL
      });

      item_expr <-
        if (expr == 'base::environment()') {
          name_sym
        } else {
          paste0(expr, '$', name_sym, collapse = '')
        };

      if (!is.null(code)) {
        # It's a promise - we don't want to force it as it could affect the debugged code.
        value <- list(promise = deparse_str(code), expression = item_expr);
      } else if (bindingIsActive(name, obj)) {
        # It's an active binding - we don't want to read it to avoid inadvertently changing program state.
        value <- list(active_binding = TRUE, expression = item_expr);
      } else {
        # It's just a regular binding, so get the actual value, but check for missing() first.

        is_missing <- tryCatch({
          value <- obj[[name]];
          missing(value)
        }, error = function(e) {
            FALSE
        });
        if (is_missing) {
            next;
        }

        value <- eval_and_describe(item_expr, environment(), '$', fields, get(name, envir = obj), repr_max_length);
      }
      
      child <- list(value);
      names(child) <- name_sym;
      last_child <<- last_child + 1;
      children[[last_child]] <<- child;
    }
  }
  
  if (is.environment(obj)) {
    process_env(fields);
  }

  process_slots <- function(fields) {
    is_S4 <- isS4(obj);
    names <- NA_if_error(slotNames(class(obj)));
  
    if (!is.null(count)) {
      names <- head(names, count);
      count <<- count - length(names);
    }
  
    for (name in names) {
      name <- force_toString(name);
      # If a binding has an empty name, or it wasn't a string, it cannot be accessed, so ignore it.
      if (name == '') {
        next;
      }

      name_sym <- symbol_token(name);
      
      # For S4 objects, slots can be accessed with '@'. For other objects, we have to
      # use slot(). Still, always use '@' as accessor name to show to the user.
      accessor <- paste0('@', name_sym, collapse = '');
      if (is_S4) {
        slot_expr <- paste0('(', expr, ')', accessor, collapse = '')
      } else {
        slot_expr <- paste0('methods::slot((', expr, '), ', deparse_str(name), ')', collapse = '')
      }
      
      value <- eval_and_describe(slot_expr, environment(), '@', fields, slot(obj, name), repr_max_length);
      
      child <- list(value);
      names(child) <- accessor;
      last_child <<- last_child + 1;
      children[[last_child]] <<- child;
    }
  }
  
  # Not just S4 objects have slots, so run this on everything - slotNames() will always
  # do the right thing.
  process_slots(fields);

  process_items <- function(fields) {  
    n <- length(obj);

    names <- names(obj);
    if (!is.character(names)) {
      names <- NULL;
    }

    # For list and language, we always want to show their children, even if there's only one.
    # For vectors, we don't want to show the child if that's the only item in the vector, to
    # avoid presenting an infinitely recursive model (a vector of size 1 has the only child,
    # which is another vector of size 1 etc). However, we do want to show the only child if
    # it is named, so that the name is exposed.
    if (n != 1 || !is.atomic(obj) || (length(names) >= 1 && !(is.null(names[[1]]) || is.na(names[[1]]) || names[[1]] == ''))) {
      if (!is.null(count)) {
        n <- min(n, count);
        count <<- count - n;
      }
  
      for (i in 1:n) {
        # Start with the assumption that this is an unnamed item, accessed by position.
        accessor <- paste0('[[', as.double(i), ']]', collapse = '');
        kind <- '[[';
  
        # If it has a name, it is a named item - but only if that name is unique, or
        # if this item corresponds to the first mention of that name - i.e. if we have
        # c(1,2,3), and names() is c('x','y','x'), then c[[1]] is named 'x', but c[[3]]
        # is effectively unnamed, because there's no way to address it by name.
        name <- tryCatch({
            names[[i]]
        }, error = function(e) {
            NULL
        });
        name <- force_toString(name);
        if (name != '' && match(name, names, -1) == i) {
          kind <- '$';
          # Named items can be accessed with '$' in lists, but other types require brackets.
          if (is.list(obj)) {
            accessor <- paste0('$', symbol_token(name), collapse = '');
          } else {
            accessor <- paste0('[[', deparse_str(name), ']]', collapse = '');
          }
        }
        
        value <- eval_and_describe(paste0(expr, accessor, collapse = ''), environment(), kind, fields, obj[[i]], repr_max_length);
        
        child <- list(value);
        names(child) <- accessor;
        last_child <<- last_child + 1;
        children[[last_child]] <<- child;
      }
    }
  }
  
  # If it is an atomic vector, a list or a language object, it might have children,
  # some of which are possibly named.
  if (is.atomic(obj) || is.list(obj) || is.language(obj)) {
    process_items(fields);
  }
    
  # Trim the preallocated vector to the actual number of children placed in it.
  if (last_child == 0) list() else children[1:last_child]
}
