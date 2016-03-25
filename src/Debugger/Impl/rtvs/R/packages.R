# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

matrix.as.lists <- function(matrix.data) {
    res <- list()
    dn <- dimnames(matrix.data)
    for (r in dn[[1]]) {
        res[[r]] <- as.list(matrix.data[r,])
    }
    res
}

packages.installed <- function() {
    matrix.as.lists(installed.packages(fields = c('Title', 'Author')))
}

packages.available <- function() {
    matrix.as.lists(available.packages())
}
