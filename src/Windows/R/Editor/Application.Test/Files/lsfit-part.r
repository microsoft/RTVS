  if (nry != nrx) 
    stop(sprintf(paste0(ngettext(nrx, "'X' matrix has %d case (row)", 
                                 "'X' matrix has %d cases (rows)"), ", ", ngettext(nry, 
                                                                                   "'Y' has %d case (row)", "'Y' has %d cases (rows)")), 
                 nrx, nry), domain = NA)
  if (nry < ncx) 
    stop(sprintf(paste0(ngettext(nry, "only %d case", "only %d cases"), 
                        ", ", ngettext(ncx, "but %d variable", "but %d variables")), 
                 nry, ncx), domain = NA)
