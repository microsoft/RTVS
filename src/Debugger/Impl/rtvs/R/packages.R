# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

matrix.as.lists <- function(matrix.data) {
    res <- base::list()
    dn <- base::dimnames(matrix.data)
    for (r in dn[[1]]) {
        res[[r]] <- base::as.list(matrix.data[r,])
    }
    res
}

packages.installed <- function() {
    matrix.as.lists(utils::installed.packages(fields = c('Title', 'Author')))
}

packages.available <- function() {
    matrix.as.lists(utils::available.packages())
}

packages.loaded <- function() {
    prefix <- "package:"
    prefix.length <- base::nchar(prefix)
    loaded <- base::search()
    pkgs <- base::substring(loaded[base::substring(loaded, 1, prefix.length) == prefix], prefix.length + 1)
    base::as.list(pkgs)
}

packages.libpaths <- function() {
    base::as.list(base::.libPaths())
}
