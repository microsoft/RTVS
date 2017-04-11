# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

# Display a help page for a given topic. Tries ? first, and if that does not produce a page, uses ?? as a fallback.
show_help <- function(topic) {
    help <- eval(substitute(?topic, list(topic = topic)))
    if (length(help) == 0 || is.na(help)) {
        help <- eval(substitute(??topic, list(topic = topic)))
    }

    # Printing out the result will cause the help page to be displayed. 
    print(help)
}