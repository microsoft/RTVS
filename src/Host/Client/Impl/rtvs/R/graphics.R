# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

graphics.xaml <- function(filename, width, height) {
   invisible(external_embedded('xaml_graphicsdevice_new', filename, width, height))
}

graphics.ide.resize <- function(width, height, resolution) {
   invisible(external_embedded('ide_graphicsdevice_resize', width, height, resolution))
}

graphics.ide.new <- function() {
   invisible(external_embedded('ide_graphicsdevice_new'))
}

graphics.ide.nextplot <- function() {
   invisible(external_embedded('ide_graphicsdevice_next_plot'))
}

graphics.ide.previousplot <- function() {
   invisible(external_embedded('ide_graphicsdevice_previous_plot'))
}

graphics.ide.clearplots <- function() {
   invisible(external_embedded('ide_graphicsdevice_clear_plots'))
}

graphics.ide.removeplot <- function() {
   invisible(external_embedded('ide_graphicsdevice_remove_plot'))
}
