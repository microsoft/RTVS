# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

graphics.xaml <- function(filename, width, height) {
   invisible(external_embedded('xaml_graphicsdevice_new', filename, width, height))
}

graphics.ide.resize <- function(device_id, width, height, resolution) {
    invisible(external_embedded('ide_graphicsdevice_resize', device_id, width, height, resolution))
}

graphics.ide.new <- function() {
   invisible(external_embedded('ide_graphicsdevice_new'))
}

graphics.ide.nextplot <- function(device_id) {
    invisible(external_embedded('ide_graphicsdevice_next_plot', device_id))
}

graphics.ide.previousplot <- function(device_id) {
    invisible(external_embedded('ide_graphicsdevice_previous_plot', device_id))
}

graphics.ide.clearplots <- function(device_id) {
    invisible(external_embedded('ide_graphicsdevice_clear_plots', device_id))
}

graphics.ide.copyplot <- function(source_device_id, source_plot_id, target_device_id) {
   invisible(external_embedded('ide_graphicsdevice_copy_plot', source_device_id, source_plot_id, target_device_id))
}

graphics.ide.removeplot <- function(device_id, plot_id) {
    invisible(external_embedded('ide_graphicsdevice_remove_plot', device_id, plot_id))
}

graphics.ide.selectplot <- function(device_id, plot_id, force_render=TRUE) {
    invisible(external_embedded('ide_graphicsdevice_select_plot', device_id, plot_id, force_render))
}

graphics.ide.getactivedeviceid <- function() {
    device_num <- dev.cur()
    if (!is.null(device_num)) {
        device_id <- graphics.ide.getdeviceid(device_num)
    } else {
        device_id <- NULL
    }
    device_id
}

graphics.ide.setactivedeviceid <- function(device_id) {
    device_num <- graphics.ide.getdevicenum(device_id)
    if (!is.null(device_num)) {
        dev.set(device_num)
    }
}

graphics.ide.getdeviceid <- function(device_num) {
    invisible(external_embedded('ide_graphicsdevice_get_device_id', device_num))
}

graphics.ide.getdevicenum <- function(device_id) {
    invisible(external_embedded('ide_graphicsdevice_get_device_num', device_id))
}

graphics.ide.getactiveplotid <- function(device_id) {
    invisible(external_embedded('ide_graphicsdevice_get_active_plot_id', device_id))
}

# Helper to export current plot to image
graphics.ide.exportimage <- function(device_id, plot_id, filename, device, width, height, resolution) {
    prev_device_num <- dev.cur()
    graphics.ide.selectplot(device_id, plot_id, force_render=FALSE)
    dev.copy(device=device,filename=filename,width=width,height=height,res=resolution)
    dev.off()
    dev.set(prev_device_num)
}

graphics.ide.exportpdf <- function(device_id, plot_id, filename, width, height) {
    prev_device_num <- dev.cur()
    graphics.ide.selectplot(device_id, plot_id, force_render=FALSE)
    dev.copy(device=pdf,file=filename,width=width,height=height)
    dev.off()
    dev.set(prev_device_num)
}
