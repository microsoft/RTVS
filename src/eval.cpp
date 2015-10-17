#include "eval.h"

using namespace rhost::util;

namespace rhost {
    namespace eval {
        std::vector<r_eval_result<unique_sexp>> r_eval(const std::string& expr, SEXP env, ParseStatus& parse_status) {
            std::vector<r_eval_result<unique_sexp>> results;

            unique_sexp sexp_expr(Rf_allocVector3(STRSXP, 1, nullptr));
            SET_STRING_ELT(sexp_expr.get(), 0, Rf_mkChar(expr.c_str()));

            unique_sexp sexp_parsed(Rf_protect(R_ParseVector(sexp_expr.get(), -1, &parse_status, R_NilValue)));
            if (parse_status == PARSE_OK) {
                results.resize(Rf_length(sexp_parsed.get()));
                for (int i = 0; i < results.size(); ++i) {
                    auto& result = results[i];

                    result.value.reset(R_tryEvalSilent(VECTOR_ELT(sexp_parsed.get(), i), env, &result.has_error));
                    if (result.value) {
                        result.has_value = true;
                    }
                    if (result.has_error) {
                        result.error = R_curErrorBuf();
                    }
                }
            }

            return results;
        }

        r_eval_result<std::string> r_eval_str(const std::string& expr, SEXP env, ParseStatus& parse_status) {
            r_eval_result<std::string> result;

            auto results = r_eval(expr, env, parse_status);
            if (!results.empty()) {
                auto& last = *(results.end() - 1);
                result.has_error = last.has_error;
                result.error = last.error;

                if (last.value) {
                    const char* s = R_CHAR(Rf_protect(Rf_asChar(last.value.get())));
                    if (s) {
                        result.has_value = true;
                        result.value = s;
                    } else {
                        result.has_value = false;
                    }

                    Rf_unprotect(1);
                }
            }

            return result;
        }
    }
}