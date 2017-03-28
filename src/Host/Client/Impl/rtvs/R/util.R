# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

version <- 1

call_embedded <- function(name, ...) {
    .Call(paste0('Microsoft.R.Host::Call.', name, collapse = ''), ..., PACKAGE = '(embedding)')
}

external_embedded <- function(name, ...) {
    .External(paste0('Microsoft.R.Host::External.', name, collapse = ''), ..., PACKAGE = '(embedding)')
}

send_notification <- function(name, ...) {
    call_embedded('send_notification', name, list(...))
}

send_request_and_get_response <- function(name, ...) {
    call_embedded('send_request_and_get_response', name, list(...))
}

loc_message <- function(id, ...) {
    send_notification("!LocMessage", id, list(...))
}

loc_warning <- function(id, ...) {
    send_notification("!LocWarning", id, list(...))
}

loc_askYesNo <- function(id, ...) {
    send_request_and_get_response('?LocYesNo', id, list(...))[[1]]
}

memory_connection <- function(max_length = NA, expected_length = NA, overflow_suffix = '', eof_marker = '') {
    call_embedded('memory_connection', max_length, expected_length, overflow_suffix, eof_marker)
}

memory_connection_overflown <- function(con) {
    call_embedded('memory_connection_overflown', con)
}

memory_connection_tochar <- function(con) {
    call_embedded('memory_connection_tochar', con)
}

unevaluated_promise <- function(name, env) {
    call_embedded("unevaluated_promise", name, env)
}

is_missing <- function(name, env) {
    call_embedded("is_missing", name, env)
}

is_rdebug <- function(obj) {
    call_embedded("is_rdebug", obj)
}

set_rdebug <- function(obj, debug) {
    call_embedded("set_rdebug", obj, debug)
}

browser_set_debug <- function(n = 1, skip_toplevel = 0) {
    call_embedded("browser_set_debug", n, skip_toplevel)
}

toJSON <- function(obj) {
    call_embedded("toJSON", obj)
}

create_blob <- function(obj) {
    call_embedded("create_blob", obj)
}

create_compressed_blob <- function(obj) {
    call_embedded("create_compressed_blob", obj)
}

get_blob <- function(blob_id) {
    call_embedded("get_blob", blob_id)
}

destroy_blob <- function(blob_id) {
    invisible(call_embedded("destroy_blob", blob_id))
}

set_disconnect_callback <- function(callback) {
    invisible(call_embedded('set_disconnect_callback', callback))
}

NA_if_error <- function(expr) {
    tryCatch(expr, error = function(e) { NA })
}

NULL_if_error <- function(expr) {
    tryCatch(expr, error = function(e) { NULL })
}

# Like toString, but guarantees that result is a single-element character vector.
force_toString <- function(obj) {
    if (is.null(obj) || (length(obj) == 1 && is.atomic(obj) && is.na(obj) && !is.nan(obj))) {
        return('');
    }
    s <- paste0(toString(obj), collapse = '');
    if (!is.character(s) || length(s) != 1 || is.na(s)) '' else s;
}

# Guarantees that result is a single-element numeric vector or NA.
force_number <- function(x) {
    if (!is.numeric(x) || length(x) != 1) NA else x;
}

# Like dput, but returns the value as string rather than printing it, and can limit
# the output to a desired length.
dput_str <- function(obj, max_length = NA, expected_length = NA, overflow_suffix = '...') {
    con <- memory_connection(max_length, expected_length, overflow_suffix);
    on.exit(close(con), add = TRUE);

    tryCatch({
        dput(obj, con);
    }, error = function(e) {
    });

  # Strip leading and trailing whitespace - it is never significant, and there's always
  # at least a trailing '\n' that dput always outputs.
    gsub("^\\s+|\\s+$", "", memory_connection_tochar(con))
}

# Like deparse, but always returns a single string.
deparse_str <- function(x)
paste0(deparse(x), collapse = '')

# Makes a symbol token (properly quoted with backticks if necessary) out of a symbol or a string.
symbol_token <- function(name) {
    s <- force_toString(name);

  # If it's an empty string, it's not a valid symbol, even if quoted.
    if (identical(s, '')) {
        return(NULL);
    }

  # If it's a valid identifier, it's good to go as is. Because the definition of identifier in R
  # is locale-dependent, be conservative and match ASCII only; excessive quoting is always safe.
    if (grepl('^[A-Za-z_.][A-Za-z0-9_.]*$', name)) {
        return(s);
    }

  # Deparse it - this will take care of all the necessary escaping for everything other than
  # backticks, but will also put double quotes around that we'll remove later.
    s <- deparse_str(force_toString(s));

  # Escape any backticks.
    s <- gsub('`', '\\`', s, fixed = TRUE);

  # Replace surrounding quotes with backticks.
    paste0('`', substr(s, 2, nchar(s) - 1), '`', collapse = '')
}

# Like eval, but will not enter Browse mode if env has its debug bit set
# (e.g. when it has just been stepped into).
safe_eval <- function(expr, env) {
    debug <- is_rdebug(env);
    tryCatch({
        set_rdebug(env, FALSE);
        eval(expr, env)
    }, finally = {
        set_rdebug(env, debug);
    });
}

# Helper to export variable to CSV
export_to_csv <- function(expr, sep, dec) {
    res <- expr
    ln <- length(res) + 1
    filepath <- tempfile('export_', fileext = '.csv')
    on.exit(unlink(filepath))
    write.table(res, file = filepath, qmethod = 'double', col.names = NA, sep = sep, dec = dec)
    create_compressed_blob(readBin(filepath, 'raw', file.info(filepath)$size))
}

# Helper to export current plot to image
export_to_image <- function(device_id, plot_id, device, width, height, resolution) {
    prev_device_num <- dev.cur()
    graphics.ide.setactivedeviceid(device_id)
    graphics.ide.selectplot(device_id, plot_id, force_render = FALSE)
    filepath <- tempfile('plot_', fileext = '.dat')
    on.exit(unlink(filepath))
    dev.copy(device = device, filename = filepath, width = width, height = height, res = resolution)
    dev.off()
    dev.set(prev_device_num)
    create_blob(readBin(filepath, 'raw', file.info(filepath)$size))
}

# Helper to export current plot to pdf
export_to_pdf <- function(device_id, plot_id, pdf_device, width, height, ...) {
    prev_device_num <- dev.cur()
    graphics.ide.setactivedeviceid(device_id)
    graphics.ide.selectplot(device_id, plot_id, force_render = FALSE)
    filepath <- tempfile('plot_', fileext = '.pdf')
    on.exit(unlink(filepath))

    dev.copy(device = pdf_device, file = filepath, width = width, height = height, ...)
    dev.off()
    dev.set(prev_device_num)
    create_blob(readBin(filepath, 'raw', file.info(filepath)$size))
}

# Helper to publish rmarkdown files remotely
rmarkdown_publish <- function(blob_id, output_format, encoding) {
    # Create temp file to store the rmarkdown file
    rmdpath <- tempfile('rmd_', fileext = '.rmd');
    on.exit(unlink(rmdpath));
    writeBin(get_blob(blob_id), rmdpath);

    # Get file extension from format
    fileext <- if (identical(output_format, 'html_document')) {
        '.html'
    } else if (identical(output_format, 'pdf_document')) {
        '.pdf'
    } else if (identical(output_format, 'word_document')) {
        '.docx'
    } else {
        '.tmp'
    }

    # Create temp file to store the markdown render output
    output_filepath <- tempfile('rmd_', fileext = fileext);
    on.exit(unlink(output_filepath));

    rmarkdown::render(rmdpath, output_format = output_format, output_file = output_filepath, output_dir = tempdir(), encoding = encoding);
    create_blob(readBin(output_filepath, 'raw', file.info(output_filepath)$size));
}

as.lock_state <- function(x) {
    factor(x, levels = c(0, 1, 2), labels = c('unlocked', 'locked_by_r_session', 'locked_by_other'));
}

package_lock_state <- function(package_name, lib_path) {
    files <- dir(path = paste0(lib_path, '/', package_name), pattern = '\\.', recursive = TRUE, ignore.case = TRUE, full.names = TRUE)
    fstate <- call_embedded('get_file_lock_state', files);
    as.lock_state(fstate);
}

package_uninstall <- function(package_name, lib_path) {
    lock_state <- package_lock_state(package_name, lib_path);
    if (lock_state == 'unlocked') {
        remove.packages(package_name, lib = lib_path);
    }
    lock_state
}

package_update <- function(package_name, lib_path) {
    lock_state <- package_uninstall(package_name, lib_path)
    if (lock_state == 'unlocked') {
        install.packages(package_name, lib = lib_path);
    }
    lock_state
}

# Helper to download a file from remote host
fetch_file <- function(remotePath, localPath = '', silent = FALSE) {
    invisible(call_embedded('fetch_file', path.expand(remotePath), path.expand(localPath), silent));
}

save_to_project_folder <- function(blob_id, project_name, dest_dir) {
    temp_dir <- paste0(tempdir(), '/RTVSProjects');
    invisible(call_embedded('save_to_project_folder', blob_id, project_name, path.expand(dest_dir), temp_dir));
}

save_to_temp_folder <- function (blob_id, file_name) {
    temp_file <- paste0(tempdir(), '/', file_name);
    unlink(temp_file);
    writeBin(get_blob(blob_id), temp_file);
    temp_file;
}

autosave_filename <- '~/.Autosave.RData';

query_reload_autosave <- function() {
    if (!file.exists(autosave_filename)) {
        return(FALSE);
    }

    res <- loc_askYesNo('rtvs_SessionTerminatedUnexpectedly', autosave_filename)[[1]];

    if (identical(res, 'Y')) {
        # Use try instead of tryCatch, so that any errors are printed as usual.
        loaded <- FALSE;
        try({
            load(autosave_filename, envir = .GlobalEnv);
            loaded <- TRUE;
        });

        if (loaded) {
            loc_warning('rtvs_LoadedWorkspace', autosave_filename);
            # If we loaded the file successfully, it's safe to delete it - this session contains the reloaded
            # state now, and if there's another disconnect, it will be autosaved again.
            return(TRUE);
        } else {
            loc_warning('rtvs_FailedToLoadWorkspace', autosave_filename);
            return(FALSE);
        }
    } else {
        res <-loc_askYesNo('rtvs_ConfirmDeleteWorkspace', autosave_filename)[[1]];
        return(identical(res, 'Y'));
    }
}

save_state <- function() {
    # This function runs when client is already disconnected, so loc_message() cannot be used, since it requires
    # a connected client to provide translated strings. However, since messages are not user-visible and are
    # here for logging purposes only, they don't need to be localized in the first place. Also, they 
    # cannot be localized since 'message' and 'sprintf' depend on currely set R locale which may or 
    # may not be the same as current client UI locale (VS UI language is set indedendently from OS and R).
    message(sprintf('Autosaving workspace to image "%s" ...', autosave_filename), appendLF = FALSE);
    save.image(autosave_filename);
    message(' workspace saved successfully.');
}

enable_autosave <- function(delete_existing) {
    try({
        set_disconnect_callback(save_state);

        if (delete_existing) {
            loc_warning('rtvs_DeletingWorkspace', autosave_filename);
            unlink(autosave_filename);
        }
    });
}

exists_on_path_windows <- function(filename) {
    folders <- strsplit(Sys.getenv('PATH'), ';')[[1]];
    folders <- gsub("\\", "/", folders, fixed = TRUE);
    folders <- sub("(\\/$)", "", folders);
    folders <- paste0(folders, "/", filename);
    for (f in folders) {
        if (file.exists(f)) {
            return(TRUE);
        }
    }
    return(FALSE);
}

exists_on_path_posix <- function(filename) {
    folders <- strsplit(Sys.getenv('PATH'), ':')[[1]];
    folders <- paste0(folders, "/", filename);
    for (f in folders) {
        if (file.exists(f)) {
            return(TRUE);
        }
    }
    return(FALSE);
}

exists_on_path <- if(.Platform$OS.type == "windows") exists_on_path_windows else exists_on_path_posix;

executable_exists_on_path <- function(filename_no_extension) {
    filename <- filename_no_extension;
    if(.Platform$OS.type == "windows"){
        filename <- paste0(filename_no_extension, ".exe");
    }
    return(exists_on_path(filename));
}
