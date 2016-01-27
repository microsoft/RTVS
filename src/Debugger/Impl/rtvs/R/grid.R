grid.header <- function(obj, range, isRow) {
  vp <- list();

  dn <- dimnames(obj);
  if (!is.null(dn) && (length(dn)==2)) {
    if (isRow) {
      vp$headers<-grid.str.vector(dn[[1]][range]);
    } else {
      vp$headers<-grid.str.vector(dn[[2]][range])
    }
  } else {
    vp$headers<-list();
  }
  vp;
}

grid.trim <- function(str, max_length = 100) {
  if (nchar(str) > (100 - 3)) {
    x <- paste(substr(str, 1, 97), '...', sep='');
  } else {
    str;
  }
}

grid.format <- function(x) {
    y <- sapply(format(x), grid.trim);
}

grid.data <- function(x, rows, cols) {
  d <- dim(x);
  if (is.null(d) || (length(d) != 2)) {
    stop('grid.data requires two dimensional object');
  }

  if ((length(rows) == 1) || (length(cols) == 1)) {
      x1 <- grid.format(x[rows, cols]);
  } else {
      x0 <- as.data.frame(x[rows, cols]);
      x1 <- apply(x0, 2, grid.format);
  }

  vp<-list();

  dn <- dimnames(x);
  if (!is.null(dn) && (length(dn)==2)) {
    dnvalue <- 0;
    vp$dimnames <- dnvalue;
    if (!is.null(dn[[1]])) {
      vp$row.names <- sapply(row.names(x)[rows], format, USE.NAMES = FALSE);
      dnvalue <- dnvalue + 1;
    } else {
      vp$row.names <- 'dummy';
    }
    
    if (!is.null(dn[[2]])) {
      vp$col.names <- sapply(colnames(x)[cols], format, USE.NAMES = FALSE);
      dnvalue <- dnvalue + 2;
    } else {
      vp$col.names <- 'dummy';
    }
    vp$dimnames <- format(dnvalue);
  } else {
    vp$dimnames <- '0';
    vp$row.names <- 'dummy';  # dummy required for parser
    vp$col.names <- 'dummy';
  }

  vp$data<-x1;

  vp;
}

grid.str.vector<-function(v) {
  vr <- list();
  index<-1;
  for (item in v) {
    if (is.character(item)){
      vr[[index]]<-item;
    } else {
      vr[[index]]<-capture.output(str(item, give.head = FALSE));
    }
    index<-index+1;
  }
  vr;
};

grid.dput2 <- function(obj) {
    capture.output(cat(capture.output(dput(obj))));
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

grid.toJSON <- function(obj) {
    conn <- textConnection(NULL, open = "w");
    json <- "{}";
    tryCatch({
        rtvs:::toJSON(obj, conn);
        cat('\n', file = conn, sep = '');
        json <- textConnectionValue(conn);
    }, finally = {
        close(conn);
    });
    json;
}
