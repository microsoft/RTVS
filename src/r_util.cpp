#include "stdafx.h"
#include "Rapi.h"

namespace {
    extern "C" SEXP is_unseen_promise(SEXP name, SEXP env) {
        if (!Rf_isEnvironment(env)) {
            Rf_error("env is not an environment");
        }
        if (!Rf_isString(name) || Rf_length(name) != 1) {
            Rf_error("name is not a single string");
        }

        SEXP value = Rf_findVar(Rf_installChar(STRING_ELT(name, 0)), env);
        return (TYPEOF(value) == PROMSXP && !PRSEEN(value)) ? R_TrueValue : R_FalseValue;
    }

    R_CallMethodDef call_methods[] = {
        { ".rtvs.Call.is_unseen_promise", (DL_FUNC)&is_unseen_promise, 2 },
        { }
    };
}

void R_init_util(DllInfo *dll) {
    R_registerRoutines(dll, nullptr, call_methods, nullptr, nullptr);
}
