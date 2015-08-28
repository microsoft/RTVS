x <- function (x, y, wt = NULL, intercept = TRUE, tolerance = 1e-07, 
    yname = NULL) 
{
    x <- as.matrix(x)
    y <- as.matrix(y)
    xnames <- colnames(x)
    if (is.null(xnames)) {
        if (ncol(x) == 1L) 
            xnames <- "X"
        else xnames <- paste0("X", 1L:ncol(x))
    }
    if (intercept) {
        x <- cbind(1, x)
        xnames <- c("Intercept", xnames)
    }
    if (is.null(yname) && ncol(y) > 1) 
        yname <- paste0("Y", 1L:ncol(y))
    good <- complete.cases(x, y, wt)
    dimy <- dim(as.matrix(y))
    if (any(!good)) {
        warning(sprintf(ngettext(sum(!good), "%d missing value deleted", 
            "%d missing values deleted"), sum(!good)), domain = NA)
        x <- as.matrix(x)[good, , drop = FALSE]
        y <- as.matrix(y)[good, , drop = FALSE]
        wt <- wt[good]
    }
    nrx <- NROW(x)
    ncx <- NCOL(x)
    nry <- NROW(y)
    ncy <- NCOL(y)
    nwts <- length(wt)
    if (nry != nrx) 
        stop(sprintf(paste0(ngettext(nrx, "'X' matrix has %d case (row)", 
            "'X' matrix has %d cases (rows)"), ", ", ngettext(nry, 
            "'Y' has %d case (row)", "'Y' has %d cases (rows)")), 
            nrx, nry), domain = NA)
    if (nry < ncx) 
        stop(sprintf(paste0(ngettext(nry, "only %d case", "only %d cases"), 
            ", ", ngettext(ncx, "but %d variable", "but %d variables")), 
            nry, ncx), domain = NA)
    if (!is.null(wt)) {
        if (any(wt < 0)) 
            stop("negative weights not allowed")
        if (nwts != nry) 
            stop(gettextf("number of weights = %d should equal %d (number of responses)", 
                nwts, nry), domain = NA)
        wtmult <- wt^0.5
        if (any(wt == 0)) {
            xzero <- as.matrix(x)[wt == 0, ]
            yzero <- as.matrix(y)[wt == 0, ]
        }
        x <- x * wtmult
        y <- y * wtmult
        invmult <- 1/ifelse(wt == 0, 1, wtmult)
    }
    z <- .Call(C_Cdqrls, x, y, tolerance, FALSE)
    resids <- array(NA, dim = dimy)
    dim(z$residuals) <- c(nry, ncy)
    if (!is.null(wt)) {
        if (any(wt == 0)) {
            if (ncx == 1L) 
                fitted.zeros <- xzero * z$coefficients
            else fitted.zeros <- xzero %*% z$coefficients
            z$residuals[wt == 0, ] <- yzero - fitted.zeros
        }
        z$residuals <- z$residuals * invmult
    }
    resids[good, ] <- z$residuals
    if (dimy[2L] == 1 && is.null(yname)) {
        resids <- drop(resids)
        names(z$coefficients) <- xnames
    }
    else {
        colnames(resids) <- yname
        colnames(z$effects) <- yname
        dim(z$coefficients) <- c(ncx, ncy)
        dimnames(z$coefficients) <- list(xnames, yname)
    }
    z$qr <- as.matrix(z$qr)
    colnames(z$qr) <- xnames
    output <- list(coefficients = z$coefficients, residuals = resids)
    if (z$rank != ncx) {
        xnames <- xnames[z$pivot]
        dimnames(z$qr) <- list(NULL, xnames)
        warning("'X' matrix was collinear")
    }
    if (!is.null(wt)) {
        weights <- rep.int(NA, dimy[1L])
        weights[good] <- wt
        output <- c(output, list(wt = weights))
    }
    rqr <- list(qt = drop(z$effects), qr = z$qr, qraux = z$qraux, 
        rank = z$rank, pivot = z$pivot, tol = z$tol)
    oldClass(rqr) <- "qr"
    output <- c(output, list(intercept = intercept, qr = rqr))
    return(output)
}
