signature.help2 <- function(f, p) {
    x <- help(paste(f), paste(p))
    y <- utils:::.getHelpFile(x)
    paste0(y, collapse = '')
}

signature.help1 <- function(f) {
    x <- help(paste(f))
    y <- utils:::.getHelpFile(x)
    paste0(y, collapse = '')
}
