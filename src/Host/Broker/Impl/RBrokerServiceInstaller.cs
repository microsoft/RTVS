using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Broker {
    [RunInstaller(true)]
    public partial class RBrokerServiceInstaller : System.Configuration.Install.Installer {
        public RBrokerServiceInstaller() {
            InitializeComponent();
        }
    }
}
