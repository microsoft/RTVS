#! /bin/sh
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.
#
# Description: Remote R Server 

. /etc/init.d/functions

start() {
        initlog -c "echo -n Starting Remote R Server: "
        dotnet /usr/bin/Microsoft.R.Host.Broker.dll --config "/etc/rtvs/Microsoft.R.Host.Broker.config.json" &
        touch /var/lock/subsys/Microsoft.R.Host.Broker.dll
        success $"Microsoft.R.Host.Broker server startup"
        echo
}

stop() {
        initlog -c "echo -n Stopping Remote R Server: "
        killproc /usr/bin/Microsoft.R.Host.Broker.dll
        rm -f /var/lock/subsys/Microsoft.R.Host.Broker.dll
        echo
}

case "$1" in
  start)
        start
        ;;
  stop)
        stop
        ;;
  status)
        status FOO
        ;;
  restart|reload|condrestart)
        stop
        start
        ;;
  *)
        echo $"Usage: $0 {start|stop|restart|reload|status}"
        exit 1
esac

exit 0