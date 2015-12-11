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
    stop('gridata requires two dimensional object');
  }
  
  x0 <- as.data.frame(x);
  x0 <- x0[rows, cols];
  x <- lapply(x0, as.character);

  vp<-list();
  vp$row.names <- row.names(x0);
  vp$col.names <- colnames(x0);
  vp$data<-x;

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

gdJson <- function(obj) {
  conn <- textConnection(NULL, open="w");
  json <- "{}";
  tryCatch({
    rtvs:::toJSON(obj, conn);
    cat('\n', file=conn, sep='');
    json <- textConnectionValue(conn);
  }, finally = {
    close(conn);
  });
  json;
}
