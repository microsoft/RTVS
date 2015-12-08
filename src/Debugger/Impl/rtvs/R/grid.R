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

grid.data <- function(obj, rows, cols) {
  d <- dim(obj);
  if (is.null(d) || (length(d) != 2)) {
    stop('gridata requires two dimensional object');
  }
  
  vp<-list();
  if (is.matrix(obj)) {
    vp$matrix<-obj[rows, cols];
  } else if (is.data.frame(obj)) {
    df<-obj[rows, cols];attributes(df)$names<-cols
    vp$dataframe<-df;
  } else {
    # error
  }

#  dn <- dimnames(obj);
#  if (!is.null(dn) && (length(dn) == 2)) {
#    vp$rownames<-handle.vector(dn[[1]][rows]);
#    vp$colnames<-handle.vector(dn[[2]][cols]);
#  }

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
