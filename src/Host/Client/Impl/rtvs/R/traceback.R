# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

describe_traceback <- function() {
  calls <- sys.calls();
  nframe <- sys.nframe();

  frames <- vector('list', nframe);

  for (i in 1:nframe) {
    frame <- new.env();
    call <- sys.call(i);
    env <- sys.frame(i - 1);
    
    frame$call <- if (is.null(call)) NULL else dput_str(call);
    frame$filename <- force_toString(getSrcFilename(call, full.names = TRUE));
    if (frame$filename == '') {
      frame$filename <- NA;
    }
    
    frame$line_number <- force_number(getSrcLocation(call, which = 'line'));

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
