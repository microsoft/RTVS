.rtvs.datainspect.print_into <<- function(con, obj) {
  repr <- capture.output(str(obj, max.level = 0))
  
  cat('"class": "', file = con, sep = '');
  cat(class(obj), file = con);
  cat('"', file = con, sep = '');
  cat(',"value": ', file = con, sep = '');
  dput(repr[1], file = con);
  cat(',"type": ', file = con, sep = '');
  dput(typeof(obj), file = con);
}

.rtvs.datainspect.env_children <<- function(con, obj) {
	cat('[', file = con, sep = '');
	is_first <- TRUE;
	for (var in as.list(obj)) {
		if (is_first) {
			is_first <- FALSE;
		}
		else {
			cat(', ', file = con, sep = '');
		}
		cat('{', file = con, sep = '');
		.rtvs.datainspect.print_into(con, var);
		cat('}', file = con, sep = '');
	}
	cat(']', file = con, sep = '');
}

.rtvs.datainspect.append_children <<- function(con, obj) {
  cat(',"children": ', file = con, sep = '');
  .rtvs.datainspect.env_children(con, obj);
}

.rtvs.datainspect.eval_into <<- function(con, expr, env) {
	obj <- eval(parse(text = expr), env);
	cat('"name": ', file = con, sep = '');
	dput(expr, file = con);
	cat(',', file = con, sep = '');
	.rtvs.datainspect.print_into(con, obj);
	.rtvs.datainspect.append_children(con,obj);
}

.rtvs.datainspect.eval <<- function(expr, env) {
	con <- textConnection(NULL, open = "w");
	json <- "{}";
	tryCatch({
		cat('{', file = con, sep = '');
		.rtvs.datainspect.eval_into(con, expr, env);
		cat('}\n', file = con, sep = '');
		json <- textConnectionValue(con);
	}, finally = {
		close(con);
	});
	return(paste(json, collapse=''));
}

.rtvs.datainspect.env_vars <<- function(env) {
	con <- textConnection(NULL, open = "w");
	json <- "{}";
	tryCatch({
		cat('[', file = con, sep = '');
		is_first <- TRUE;
		for (varname in ls(env)) {
			if (is_first) {
				is_first <- FALSE;
			}
			else {
				cat(', ', file = con, sep = '');
			}
			cat('{', file = con, sep = '');
			.rtvs.datainspect.eval_into(con, varname, env);
			cat('}', file = con, sep = '');
		}
		cat(']\n', file = con, sep = '');
		json <- textConnectionValue(con);
	}, finally = {
		close(con);
	});
	
	return(paste(json, collapse=''))
}
