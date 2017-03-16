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

install.packages <- function(...) {
    invisible(rtvs:::send_notification('!BeforePackagesInstalled'))
    utils::install.packages(...)
    invisible(rtvs:::send_notification('!AfterPackagesInstalled'))
}

remove.packages <- function(...) {
    utils::remove.packages(...)
    invisible(rtvs:::send_notification('!PackagesRemoved'))
}
