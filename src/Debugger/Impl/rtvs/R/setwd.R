# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

setwd <- function(dir) {
  old <- .Internal(setwd(dir))
  rtvs:::send_message('setwd', dir)
  invisible(old)
}