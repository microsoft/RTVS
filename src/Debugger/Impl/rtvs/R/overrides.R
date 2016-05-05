# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

view <- function(x, title) {
  if(is.function(x) || is.data.frame(x) || is.table(x) || 
     is.matrix(x) || is.list(x) || is.ts(x) || length(x) > 1) {
    if (missing(title)) {
      title <- ""
    }
    invisible(rtvs:::send_message('View', deparse(substitute(x)), title))
  } else {
    print(x)
  }
}

open_url <- function(url) {
  rtvs:::send_message('open_url', url)
}

setwd <- function(dir) {
  old <- .Internal(setwd(dir))
  rtvs:::send_message('setwd', dir)
  invisible(old)
}

redirect_functions <- function(...) {
  attach(as.environment(
           list(View = rtvs:::view, library = rtvs:::library)
         ), name = ".rtvs", warn.conflicts = FALSE)
}

library <- function(...) {
  if (nargs() == 0) {
    invisible(rtvs:::send_message('library'))
  } else {
    base::library(...)
  }
}

show_file <- function (files, header, title, delete.file) {
  cFiles <- length(files)
  for (i in cFiles) {
    if ((i > length(header)) || !nzchar(header[[i]])) {
      tabName <- title
    } else {
      tabName <- header[[i]]
    }
    invisible(rtvs:::send_message('show_file', files[[i]], tabName, delete.file))
  }
}