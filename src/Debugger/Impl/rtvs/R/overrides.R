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
  rtvs:::send_message('Browser', url)
}

setwd <- function(dir) {
  old <- .Internal(setwd(dir))
  rtvs:::send_message('setwd', dir)
  invisible(old)
}

library <- function(package, ...) {
  if (nargs() == 0) {
    invisible(rtvs:::send_message('library'))
  } else {
    pkgname <- as.character(substitute(package))
    args <- list(...)
    args[['character.only']] = TRUE
    args <- c(pkgname, args)
    do.call(base::library, args)
  }
}

redirect_View <- function() {
  .rtvs <- new.env()
  .rtvs$View <- rtvs:::view
  attach(.rtvs, warn.conflicts = FALSE)
}