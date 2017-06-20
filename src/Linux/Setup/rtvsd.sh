#!/usr/bin/env bash

usage()
{
    cat << EOF
 Usage: $0 JSON-config-file
EOF
}

if [ -z "$1" ]
  then
    usage()
fi

dotnet /usr/bin/Microsoft.R.host.Broker.Linux.dll --config $1