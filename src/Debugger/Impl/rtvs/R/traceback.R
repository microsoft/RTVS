# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

describe_traceback <- function() {
  calls <- sys.calls();
  nframe <- sys.nframe();

  frames <- vector('list', nframe);

  for (i in 1:nframe) {
    frame <- new.env();
    call <- sys.call(i);
    
    frame$call <- if (is.null(call)) NULL else dput_str(call);
    frame$filename <- force_toString(getSrcFilename(call, full.names = TRUE));
    if (frame$filename == '') {
      frame$filename <- NA;
    }
    
    frame$line_number <- force_number(getSrcLocation(call, which = 'line'));
    
    if (identical(globalenv(), sys.frame(i - 1))) {
      frame$is_global <- TRUE;
    }
    
    frames[[i]] <- frame;
  }

  frames
}
