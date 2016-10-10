# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

original_mirror <- as.environment(list(url = NULL))

# If url is not NULL, set CRAN mirror to that URL.
# If it is NULL, restore CRAN mirror URL to the original value that it had before the first call to set_mirror.
set_mirror <- function(url) {
    repos <- getOption('repos')

    if (is.null(url)) {
        if (!is.null(original_mirror$url)) {
            repos[['CRAN']] <- original_mirror$url
        }

        # If the only mirror on the list is @CRAN@, it will trigger the mirror selector UI every time.
        # To avoid that, override it to point to the automatic mirror redirection service that will pick the right one by itself. 
        if (identical(repos[['CRAN']], '@CRAN@')) {
            repos[['CRAN']] <- 'https://cloud.r-project.org/'
        }
    } else {
        if (is.null(original_mirror$url)) {
            original_mirror$url <- repos[['CRAN']]
        }
        repos[['CRAN']] <- url
    }

    options(repos = repos)
}
