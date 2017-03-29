# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

view <- function(x, title) {
    if (is.function(x) || is.data.frame(x) || is.table(x) ||
        is.matrix(x) || is.list(x) || is.ts(x) || length(x) > 1) {

        dep <- deparse(substitute(x), backtick = TRUE)
        if (missing(title)) {
            title <- dep[1]
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

# Override for edit(...). Opens file in VS editor and blocks until the file is closed.
edit_file <- function(name = NULL, file = NULL, title = NULL) {
    source <- NULL;
    if(is.function(name)) {
        source <- send_request_and_get_response("?EditFile", paste0(deparse(name), collapse = '\n'), NULL);
    } else if(!is.null(file) && is.character(file) && file != "") {
        source <- send_request_and_get_response("?EditFile", NULL, file);
    }
    if(!is.null(source)) {
        source <- gsub("\r", "", source)
        result <- try(eval.parent(parse(text = source)));
        return(result);
    }
    editor <- getOption("externalEditor");
    if(is.null(editor)) {
        editor <- "notepad";
    }
    if(is.null(title) || title == "") {
        title <- "default.r";
    }
    if(is.null(file)) {
        file <- "";
    }
    .External2(utils:::C_edit, name, file, title, editor);
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
