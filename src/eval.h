#pragma once
#include "stdafx.h"
#include "util.h"

namespace rhost {
    namespace eval {
        template <class T>
        struct r_eval_result {
            bool has_value;
            T value;
            int has_error;
            std::string error;
        };

        std::vector<r_eval_result<util::unique_sexp>> r_eval(const std::string& expr, SEXP env, ParseStatus& parse_status);

        r_eval_result<std::string> r_eval_str(const std::string& expr, SEXP env, ParseStatus& parse_status);
    }
}
