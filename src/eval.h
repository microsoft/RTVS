#pragma once
#include "stdafx.h"
#include "util.h"

namespace rhost {
    namespace eval {
        template <class T>
        struct r_eval_result {
            bool has_value;
            T value;
            bool has_error;
            std::string error;
            bool is_canceled;
        };

        template <class FBefore, class FAfter>
        inline std::vector<r_eval_result<util::unique_sexp>> r_try_eval(const std::string& expr, SEXP env, ParseStatus& parse_status, FBefore before = [] {}, FAfter after = [] {}) {
            using namespace rhost::util;

            std::vector<r_eval_result<unique_sexp>> results;

            unique_sexp sexp_expr(Rf_allocVector3(STRSXP, 1, nullptr));
            SET_STRING_ELT(sexp_expr.get(), 0, Rf_mkChar(expr.c_str()));

            unique_sexp sexp_parsed(Rf_protect(R_ParseVector(sexp_expr.get(), -1, &parse_status, R_NilValue)));
            if (parse_status == PARSE_OK) {
                results.resize(Rf_length(sexp_parsed.get()));
                for (int i = 0; i < results.size(); ++i) {
                    auto& result = results[i];
                    result.is_canceled = true;

                    struct eval_data_t {
                        SEXP expr;
                        SEXP env;
                        decltype(result)& result;
                        FBefore& before;
                        FAfter& after;
                    } eval_data = { VECTOR_ELT(sexp_parsed.get(), i), env, result, before, after };

                    auto protected_eval = [](void* pdata) {
                        auto& eval_data = *static_cast<eval_data_t*>(pdata);
                        eval_data.before();
                        eval_data.result.value.reset(Rf_eval(eval_data.expr, eval_data.env));
                        eval_data.result.is_canceled = false;
                        eval_data.after();
                    };

                    result.has_error = !R_ToplevelExec(protected_eval, &eval_data);

                    if (result.value) {
                        result.has_value = true;
                    }
                    if (result.has_error) {
                        if (result.is_canceled) {
                            // R_curErrorBuf will be bogus in this case.
                            result.error = "Evaluation canceled.";
                        } else {
                            result.error = R_curErrorBuf();
                        }
                    }
                }
            }

            return results;
        }

        template <class FBefore, class FAfter>
        inline r_eval_result<std::string> r_try_eval_str(const std::string& expr, SEXP env, ParseStatus& parse_status, FBefore before = [] {}, FAfter after = [] {}) {
            using namespace rhost::util;

            r_eval_result<std::string> result;

            auto results = r_try_eval(expr, env, parse_status, before, after);
            if (!results.empty()) {
                auto& last = *(results.end() - 1);

                result.is_canceled = last.is_canceled;
                result.has_error = last.has_error;
                result.error = last.error;

                if (last.has_value) {
                    unique_sexp sexp_char(Rf_asChar(last.value.get()));
                    const char* s = R_CHAR(sexp_char.get());
                    if (s) {
                        result.has_value = true;
                        result.value = s;
                    } else {
                        result.has_value = false;
                    }
                } else {
                    result.has_value = false;
                }
            }

            return result;
        }
    }
}
