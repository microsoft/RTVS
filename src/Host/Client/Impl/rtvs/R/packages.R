# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

packages.installed.functions <- function() {
	unname(lapply(packages.installed.named(), function(p) {
		result <- list()
		result$Package <- p$Package
		result$Description <- p$Description
		result$Version <- p$Version
		result$ExportedFunctions <- package.exported.functions.names(p$Package)
		result$InternalFunctions <- setdiff(package.all.functions.names(p$Package), result$ExportedFunctions)
		return(result)
	}))
}

package.exported.functions.names <- function(packageName) {
    as.list(union(
		tryCatch({
			getNamespaceExports(packageName)
		}, error = function(e) {
			return(c())
		}), 
		tryCatch({
			ls(paste0('package:', packageName))
		}, error = function(e) {
			return(c())
		})
	))
}

package.all.functions.names <- function(packageName) {
    as.list(
		tryCatch({
			ls(getNamespace(packageName), all.names = TRUE)
		}, error = function(e) {
			return(c())
		})
	)
}

packages.installed.named <- function() {
    pkgs <- installed.packages(fields = c('Title', 'Author', 'Description'))
    pkgs <- pkgs[!duplicated(pkgs[, 'Package']),]
    return(apply(pkgs, 1, as.list))
}

packages.installed <- function() {
    unname(packages.installed.named())
}

packages.loaded <- function() {
    as.list(.packages())
}

packages.libpaths <- function() {
    as.list(.libPaths())
}

download.packages.rds <- function(repo.url) {
  # download the repository's packages.rds and return path to the temp file
    pkg.rds.url <- gsub("src/contrib", "web/packages/packages.rds", repo.url)
    pkg.rds.path <- tempfile('pkg', fileext = '.rds')
    suppressWarnings(download.file(pkg.rds.url, destfile = pkg.rds.path, quiet = TRUE, mode = "wb"))
    return(pkg.rds.path)
}

read.packages.rds <- function(pkg.rds.path) {
  # read packages.rds into a data frame, only keep details fields
    details.fields <- c('Package', 'Title', 'Description', 'Built', 'URL', 'Author')
    extra <- readRDS(pkg.rds.path)
    extra <- as.data.frame(extra, stringsAsFactors = FALSE, rownames = extra[, 'Package'])
    return(extra[details.fields])
}

add.details <- function(repo.available, repo.url) {
    # fetch details from the repository's packages.rds file
    # and merge those details with the packages data frame passed in.
    # if we can't download or read the repo's packages.rds file,
    # we return the original package data frame (this happens for repos
    # that are not CRAN mirrors).
    tryCatch({
        pkg.rds.path <- download.packages.rds(repo.url)
        repo.details <- read.packages.rds(pkg.rds.path)
        repo.details <- repo.details[!duplicated(repo.details$Package),]
        repo.available.with.details <- merge(repo.available, repo.details, by = 'Package', all.x = TRUE)

        # merged data frame lost its row names, so add them back
        rownames(repo.available.with.details) <- repo.available.with.details[, 'Package']

        unlink(pkg.rds.path)
        return(repo.available.with.details)
    },
    error = function(e) {
        # we couldn't get the details from packages.rds, so we
        # return the original data frame, which doesn't have the details
        return(repo.available)
    }
  )
}

packages.available <- function() {
    # retrieve all available packages from all repos
    base.fields <- c('Package', 'Version', 'Depends', 'Imports', 'Suggests',
                   'Enhances', 'License', 'NeedsCompilation', 'Repository')
    all.available <- available.packages()
    all.available <- as.data.frame(all.available, stringsAsFactors = FALSE)
    all.available <- all.available[base.fields]

    # the urls of the repos we'll need to get additional details from
    repo.urls <- unique(all.available[, 'Repository'])

    # we return an unnamed list
    # each element in that list is a named list
    # ie. Package='abc', Version='1.0', Title='', etc.
    res <- list()
    for (repo.url in repo.urls) {
        # the available packages from this repo
        repo.available <- all.available[all.available$Repository == repo.url,]

        # merge details from this repo's packages.rds
        repo.available.with.details <- add.details(repo.available, repo.url)

        # append to our result each row of the dataframe as a named list
        res <- append(res, apply(repo.available.with.details, 1, function(x) as.list(x)))
    }

    return(unname(res))
}
