#include "stdafx.h"
#include "Rapi.h"

namespace {
    extern "C" SEXP unevaluated_promise(SEXP name, SEXP env) {
        if (!Rf_isEnvironment(env)) {
            Rf_error("env is not an environment");
        }
        if (!Rf_isString(name) || Rf_length(name) != 1) {
            Rf_error("name is not a single string");
        }

        SEXP value = Rf_findVar(Rf_installChar(STRING_ELT(name, 0)), env);
        if (TYPEOF(value) != PROMSXP || PRVALUE(value) != R_UnboundValue) {
            return R_NilValue;
        }

        return PRCODE(value);
    }

    R_CallMethodDef call_methods[] = {
        { ".rtvs.Call.unevaluated_promise", (DL_FUNC)unevaluated_promise, 2 },
        { }
    };
}

void R_init_util(DllInfo *dll) {
    R_registerRoutines(dll, nullptr, call_methods, nullptr, nullptr);
}
