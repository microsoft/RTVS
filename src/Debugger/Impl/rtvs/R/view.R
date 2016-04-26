# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

view <- function(x, title) {
  if(is.function(x) || is.data.frame(x) || is.matrix(x) || is.list(x)) {
    if (missing(title)) {
      title <- ""
    }
    invisible(rtvs:::send_message('View', deparse(substitute(x)), title))
  } else {
    print(x)
  }
}