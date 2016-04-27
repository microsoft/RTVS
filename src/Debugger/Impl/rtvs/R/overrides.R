# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

View <- function(x, title) {
  if(is.function(x) || is.data.frame(x) || is.matrix(x) || is.list(x)) {
    if (missing(title)) {
      title <- ""
    }
    invisible(rtvs:::send_message('View', deparse(substitute(x)), title))
  } else {
    utils::View(x, title)
  }
}

browser <- function(url) {
  rtvs:::send_message('Browser', url)
}

setwd <- function(dir) {
  old <- .Internal(setwd(dir))
  rtvs:::send_message('setwd', dir)
  invisible(old)
}

library <- function(package, ...) {
  args <- list(...)
  if(missing(package) && length(args) == 0) {
    invisible(rtvs:::send_message('library'))
  } else {
    base::library(package, ...)
  }
}
