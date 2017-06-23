#! /bin/sh
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.
#
# Description: Remote R Server 

. /etc/init.d/functions

start() {
        initlog -c "echo -n Starting Remote R Server: "
        /usr/bin/rtvsd &&
        touch /var/lock/subsys/rtvsd
        success $"Remote R Server startup"
        echo
}

stop() {
        initlog -c "echo -n Stopping Remote R Server: "
        killproc /usr/bin/rtvsd
        rm -f /var/lock/subsys/rtvsd
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