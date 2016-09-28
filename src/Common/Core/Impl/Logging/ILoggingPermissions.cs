using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Logging {
    public interface ILoggingPermissions {
        /// <summary>
        /// Defines maximum allowable logging level.
        /// </summary>
        LogVerbosity MaxVerbosity { get; }

        /// <summary>
        /// Is user permitted to send feedback
        /// </summary>
        bool IsFeedbackPermitted { get; }

        /// <summary>
        /// Currently set logging level (usually via Tools | Options). 
        /// Cannot exceeed maximum level.
        /// </summary>
        LogVerbosity CurrentVerbosity { get; set; }
    }
}
