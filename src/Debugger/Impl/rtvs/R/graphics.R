graphics.xaml <- function(filename, width, height) {
   invisible(external_embedded('xaml_graphicsdevice_new', filename, width, height))
}
