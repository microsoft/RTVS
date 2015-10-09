.rtvs.datainspect.eval_into <<- function(expr, env) {
	obj <- eval(parse(text = expr), env);
	repr <- capture.output(str(obj, max.level = 0));

	fmt <- '"name":  "%s", "class": "%s", "value": "%s", "type": "%s"';
	return (gettextf(fmt, expr, class(obj), repr[1], typeof(obj)));
}
.rtvs.datainspect.eval <<- function(expr, env) {
	json <- "{}";
	tryCatch({
		fmt <- '{%s}';
		json <- gettextf(fmt, .rtvs.datainspect.eval_into(expr, env));
	}, finally = {
	});
	return(json);
}
.rtvs.datainspect.env_vars <<- function(env) {
	json <- "[]";
	tryCatch({
		get_object <- function(expr) { 
			object_fmt <- '{%s}'
			return(gettextf(object_fmt, .rtvs.datainspect.eval_into(expr, env)));
		}

		list <- sapply(ls(env), get_object, USE.NAMES = FALSE);
		array_fmt <- '[%s]';
		json <- gettextf(array_fmt, paste(list, collapse = ','));

	}, finally = {
	});
	return(json);
}
