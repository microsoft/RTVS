# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

graphics.xaml <- function(filename, width, height) {
   invisible(external_embedded('xaml_graphicsdevice_new', filename, width, height))
}

graphics.ide.resize <- function(width, height) {
   invisible(external_embedded('ide_graphicsdevice_resize', width, height))
}

graphics.ide.new <- function() {
   invisible(external_embedded('ide_graphicsdevice_new'))
}

graphics.ide.exportimage <- function(filename, device, width, height) {
    dev.copy(device=device,filename=filename,width=width,height=height,res=96)
    dev.off()
}

graphics.ide.exportpdf <- function(filename, width, height) {
    dev.copy(device=pdf,file=filename,width=width,height=height)
    dev.off()
}

graphics.ide.nextplot <- function() {
   invisible(external_embedded('ide_graphicsdevice_next_plot'))
}

graphics.ide.previousplot <- function() {
   invisible(external_embedded('ide_graphicsdevice_previous_plot'))
}

graphics.ide.historyinfo <- function() {
   external_embedded('ide_graphicsdevice_history_info')
}

graphics.ide.clearplots <- function() {
   invisible(external_embedded('ide_graphicsdevice_clear_plots'))
}

graphics.ide.removeplot <- function() {
   invisible(external_embedded('ide_graphicsdevice_remove_plot'))
}
