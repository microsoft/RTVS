# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

describe_traceback <- function() {
    calls <- sys.calls();
    nframe <- sys.nframe();

    frames <- vector('list', nframe);

    for (i in 1:nframe) {
        frame <- list();
        call <- sys.call(i);
        env <- sys.frame(i - 1);

        frame$call <- if (is.null(call)) NULL else deparse_str(call);

        frame$filename <- NA_if_error(force_toString(utils::getSrcFilename(call, full.names = TRUE)));
        if (identical(frame$filename, '')) {
            frame$filename <- NA;
        }

        frame$line_number <- NA_if_error(force_number(utils::getSrcLocation(call, which = 'line')));

        # For nameless environments like those of functions, it will be something useless like
        # <environment: 0x0000000012aaabb0>, so omit it for them - callers are expected to use
        # $call of the calling environment in that case if they need a UI label.
        if (!identical(environmentName(env), '')) {
            frame$env_name <- format(env);
        }

        frames[[i]] <- frame;
    }

    frames
}
