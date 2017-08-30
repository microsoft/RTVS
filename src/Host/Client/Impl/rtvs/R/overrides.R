# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

view_env <- new.env() # Make sure this matches ViewEnvPrefix in VariableGridHost

# 0 = cached, 1 = dynamic
set_view_mode <- function(mode) {
    view_env$view_mode <- mode 
}

view <- function(x, title) {
    if (is.function(x) || is.data.frame(x) || is.table(x) ||
        is.matrix(x) || is.list(x) || is.ts(x) || length(x) > 1) {

        dep <- deparse(substitute(x), backtick = TRUE)
        if (missing(title)) {
            title <- dep[1]
        }
        
        # Cache expression result in default mode.
        # In dynamic mode pass expression to the main module for evaluation.
        if((is.null(view_env$view_mode) || view_env$view_mode == 0) && !is.function(x)) {
            if(is.null(view_env$view_variable_num)) {
                view_env$view_variable_num <- 1
            }
            var_name <- paste0("x", view_env$view_variable_num)
            view_env$view_variable_num <- view_env$view_variable_num + 1
            assign(var_name, as.data.frame(x), view_env)
            dep <- paste0("rtvs:::view_env$", var_name)
        }

        invisible(rtvs:::send_notification('!View', dep, title))
    } else {
        print(x)
    }
}

open_url <- function(url) {
    rtvs:::send_notification('!WebBrowser', url)
}

setwd <- function(dir) {
    old <- .Internal(setwd(dir))
    rtvs:::send_notification('!SetWD', dir)
    invisible(old)
}

redirect_functions <- function(...) {
    attach(as.environment(
           list(View = rtvs:::view,
                library = rtvs:::library,
                install.packages = rtvs:::install.packages,
                remove.packages = rtvs:::remove.packages)
         ), name = ".rtvs", warn.conflicts = FALSE)
}

library <- function(...) {
    if (nargs() == 0) {
        invisible(rtvs:::send_notification('!Library'))
    } else {
        base::library(...)
    }
}

show_file <- function(files, header, title, delete.file) {
    cFiles <- length(files)
    for (i in cFiles) {
        if ((i > length(header)) || !nzchar(header[[i]])) {
            tabName <- title
        } else {
            tabName <- header[[i]]
        }
        invisible(rtvs:::send_notification('!ShowFile', files[[i]], tabName, delete.file))
    }
}

defaultEditor <- getOption('editor')

# Override for edit(...). Opens file in VS editor and blocks until the file is closed.
edit_file <- function(name = NULL, file = NULL, title = NULL) {
    source <- NULL;
    if(is.function(name)) {
        source <- send_request_and_get_response("?EditFile", paste0(deparse(name), collapse = '\n'), NULL);
    } else if(!is.null(file) && is.character(file) && !identical(file, "")) {
        source <- send_request_and_get_response("?EditFile", NULL, file);
    }
    if(!is.null(source)) {
        source <- gsub("\r", "", source)
        result <- try(eval.parent(parse(text = source)));
        return(result);
    }
    edit(name, file, title, editor = defaultEditor);
}

install.packages <- function(...) {
    invisible(rtvs:::send_request_and_get_response('?BeforePackagesInstalled'))
    utils::install.packages(...)
    invisible(rtvs:::send_request_and_get_response('?AfterPackagesInstalled'))
}

remove.packages <- function(...) {
    utils::remove.packages(...)
    invisible(rtvs:::send_notification('!PackagesRemoved'))
}

suppress_ui <- function() {
    # The message may only become visible in remote session.
    # TODO: provide localized replacement.
    not_supported <- function(...) { stop('Not supported') }
    
    if (tolower(.Platform$OS.type) == "windows"){
        # Suppress Windows UI 
        # http://astrostatistics.psu.edu/datasets/R/html/utils/html/winMenus.html
        replace_function('bringToTop', 'grDevices', not_supported);
        replace_function('winMenuAdd', 'utils', not_supported);
        replace_function('winMenuAddItem', 'utils', not_supported);
        replace_function('winMenuDel', 'utils', not_supported);
        replace_function('winMenuDelItem', 'utils', not_supported);
        replace_function('winMenuNames', 'utils', not_supported);
        replace_function('winMenuItems', 'utils', not_supported);
    }
}


replace_function <- function(function_name, package_name, replacement) {
    package_spec <- paste("package:", package_name, sep='');

    original <- get(function_name, package_spec, mode="function");
    if (!is.null(original)) {
      env <- as.environment(package_spec);
      unlockBinding(function_name, env);
      assign(function_name, replacement, package_spec);
      lockBinding(function_name, env);
   }

   if (package_name %in% loadedNamespaces()) {
        ns <- asNamespace(package_name)
        unlockBinding(function_name, ns)
        assign(function_name, replacement, envir = ns)
        lockBinding(function_name, ns)
    }
}
