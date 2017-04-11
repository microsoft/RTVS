# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

locals <- as.environment(list(breakpoints_enabled = FALSE));

# List of all active breakpoints.
# Names are filenames, values are vectors of line numbers.
breakpoints <- new.env(parent = emptyenv())

# Used to check whether a breakpoint is still valid at this location.
is_breakpoint <- function(filename, line_number) {
    if (locals$breakpoints_enabled) {
        line_numbers <- breakpoints[[filename]];
        return(line_number %in% line_numbers);
    } else {
        return(FALSE);
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

            # If there was no subdivision, then this is the exact instruction, but only if there was an srcref,
            # and this is not a breakpoint. However, if this is an opening curly brace for a block, and parent
            # had an srcref, then match the parent (which will be the whole block).
            if (!is.null(srcref) && !isTRUE(attr(expr[[i]], 'rtvs::is_breakpoint')) && (!have_srcrefs || !is_brace(expr[[i]]))) {
                return(i)
            }
        }
    }

    NULL
}

# Given an expression or language object, return that object with all active
# breakpoints injected into it, or NULL if no breakpoints were injected.
inject_breakpoints <- function(expr) {
    if (length(breakpoints) == 0 || length(expr) == 0) {
        return(NULL);
    }

    filename <- utils::getSrcFilename(expr);
    if (is.null(filename) || !is.character(filename) || length(filename) != 1 || is.na(filename) || identical(filename, '')) {
        return(NULL);
    }

    line_numbers <- breakpoints[[filename]];
    if (is.null(line_numbers)) {
        return(NULL);
    }

    original_expr <- expr;
    changed <- FALSE;

    for (line_num in sort(line_numbers)) {
        step <- steps_for_line_num(expr, line_num);
        if (length(step) > 0) {
            new_expr <- expr;
            target_expr <- expr[[step]];

            # If there's already an injected breakpoint there, nothing to do for this line.
            if (isTRUE(attr(target_expr, 'rtvs::at_breakpoint')) || !is.null(attr(target_expr, 'rtvs::original_expr'))) {
                next;
            }

            # Attributes cannot be set on NULL, so wrap it in (), and set attributes on the resulting call object.
            if (identical(target_expr, NULL)) {
                target_expr <- quote((NULL));
            }

            new_expr[[step]] <- substitute({
                .doTrace(if (rtvs:::is_breakpoint(FILENAME, LINE_NUMBER)) browser());
                EXPR
            }, list(
        FILENAME = filename,
        LINE_NUMBER = line_num,
        EXPR = target_expr
      ));

            attr(new_expr[[step]], 'rtvs::original_expr') <- target_expr;
            attr(new_expr[[step]][[2]], 'rtvs::is_breakpoint') <- TRUE;
            attr(new_expr[[step]][[3]], 'rtvs::at_breakpoint') <- TRUE;

            expr <- new_expr;
            changed <- TRUE;
        }
    }

    if (!changed) {
        return(NULL);
    }

    # Recursively copy srcrefs from the original expression to the new one with injected breakpoints,
    # synthesizing them for injected breakpoint expressions from original expressions that they replace
    expr <- (function(before, after) {
        if (is.symbol(before) || is.symbol(after)) {
            return(after);
        }

        before_srcref <- attr(before, 'srcref');
        attr(after, 'srcref') <- before_srcref;

        for (i in 1:length(after)) {
            if (is.null(attr(before[[i]], 'rtvs::original_expr')) && !is.null(attr(after[[i]], 'rtvs::original_expr'))) {
                # If it has the original_expr attribute that wasn't there before, it's an breakpoint expression that 
                # was freshly injected, replacing the original expression at the point where the breakpoint was set.
                # It looks like this:
                #
                # {.doTrace(...); <original expression>}
                #
                # Copy srcrefs from the original, replicating them such that they apply to the entirety of
                # the replacement expression, so that the same line is considered current for all of it.
                attr(after[[i]], 'srcref') <- rep(list(before_srcref[[i]]), length(after[[i]]));

                # Auto-step over '{' and '.doTrace', so that stepping skips to the original expression.
                attr(attr(after, 'srcref')[[i]], 'Microsoft.R.Host::auto_step_over') <- TRUE;
                attr(attr(after[[i]], 'srcref')[[2]], 'Microsoft.R.Host::auto_step_over') <- TRUE;

                # Recurse into the original expression, in case it has more breakpoints inside.
                after[[i]][[3]] <- Recall(before[[i]], after[[i]][[3]]);
            } else if (is.language(after[[i]])) {
                # Otherwise, if this is not a literal, keep recursing down in case injected breakpoints
                # are in the subexpressions of this expression.
                after[[i]] <- Recall(before[[i]], after[[i]]);
            }
        }

        after
    })(original_expr, expr);

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
    conn <- file(filename, "r", encoding = encoding);
    tryCatch({
        text <- readLines(conn, warn = FALSE)
        if (!length(text)) {
            text <- ""
        }
    }, finally = close(conn));

    srcfile <- srcfilecopy(filename, text, file.mtime(filename), isFile = TRUE)

    exprs <- parse(text = text, srcfile = srcfile);
    if (length(exprs) == 0) {
        return(quote({ }));
    }

    # Create a `{` call wrapping all expressions in the file.
    result <- quote({ });
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
    safe_eval(debug_parse(file, encoding), parent.frame(1))
}
