#include "eval.h"

namespace rhost {
    namespace eval {
        bool was_eval_canceled;

        void interrupt_eval() {
            was_eval_canceled = true;
            Rf_onintr();
        }
    }
}
