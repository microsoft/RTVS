grid.trim <- function(str, max_length = 100) {
  if (nchar(str) > (100 - 3)) {
    paste(substr(str, 1, 97), '...', sep='');
  } else {
    str;
  }
}

grid.format <- function(x) {
  sapply(format(x, trim = TRUE), grid.trim);
}

grid.data <- function(x, rows, cols) {
  d <- dim(x);
  if (is.null(d) || (length(d) != 2)) {
    stop('grid.data requires two dimensional object');
  }

  isMatrix <- is.matrix(x);

  rn <- NULL;
  cn <- NULL;
  if (isMatrix) {
    x0 <- as.data.frame(x[rows, cols]);
    rn <- row.names(x)[rows];
    cn <- colnames(x)[cols];
  } else {
    x0 <- as.data.frame(x)[rows, cols];
    rn <- row.names(x[rows,]);
    cn <- colnames(x[,cols]);
  }

  if ((length(rows) == 1) || (length(cols) == 1)) {
    x1 <- grid.format(x0);
  } else {
    x1 <- sapply(x0, grid.format, USE.NAMES=FALSE);
  }

  vp<-list();

  dnvalue <- 0;
  vp$dimnames <- dnvalue;

  if (!is.null(rn) && (length(rn)>0)) {
    vp$row.names <- sapply(rn, format, USE.NAMES = FALSE);
    dnvalue <- dnvalue + 1;
  } else {
    vp$row.names <- 'dummy';
  }

  if (!is.null(cn) && (length(cn)>0)) {
    vp$col.names <- sapply(cn, format, USE.NAMES = FALSE);
    dnvalue <- dnvalue + 2;
  } else {
    vp$col.names <- 'dummy';
  }
  vp$dimnames <- format(dnvalue);

  vp$data<-x1;

  vp;
}

grid.dput <- function(obj) {
    conn <- memory_connection(NA, 0x10000);
    json <- "{}";
    tryCatch({
        dput(obj, conn);
        json <- memory_connection_tochar(conn);
    }, finally = {
        close(conn);
    });
    json;
}

