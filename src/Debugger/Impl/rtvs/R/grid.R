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

grid.data <- function(x, rows, cols) {
  d <- dim(x);
  if (is.null(d) || (length(d) != 2)) {
    stop('grid.data requires two dimensional object');
  }
  
  x0 <- as.data.frame(x[rows, cols]);
  x1 <- lapply(x0, as.character);

  vp<-list();

  dn <- dimnames(x);
  if (!is.null(dn) && (length(dn)==2)) {
    vp$dimnames <- 'true';
  } else {
    vp$dimnames <- 'false';
  }

  vp$row.names <- row.names(x0);
  vp$col.names <- colnames(x0);
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
