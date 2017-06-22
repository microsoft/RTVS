#!/usr/bin/env bash

usage()
{
    cat << EOF
Usage: $0 [options]
 OPTIONS:
    -h          Print this message.
    -t type     Set build type (Debug or Release).
    -a arch     Set target architecture (x86 or x64).
    -o dir      Use the specified directory for build output.
    -i dir      Use the specified directory for build artifacts.
    -m          Don't colorize build output.
EOF
}

ROOT_DIR=$(dirname "$0")
BUILD_TYPE=Release
COLORIZE=yes

OPTIND=1

while getopts "h?t:a:o:i:m" opt; do
    case "$opt" in
    h|\?)
        usage
        exit 0
        ;;
    t)  
        BUILD_TYPE=$OPTARG
        ;;
    a)  
        TARGET_ARCH=$OPTARG
        ;;
    o)  
        OUT_DIR=$OPTARG
        ;;
    i)  
        INT_DIR=$OPTARG
        ;;
    m)  
        COLORIZE=no
        ;;
    esac
done

shift $((OPTIND-1))
[ "$1" = "--" ] && shift

if [ "$INT_DIR" = "" ]; then
    INT_DIR=$(dirname "$0")/obj/$BUILD_TYPE/$TARGET_ARCH
fi

pushd $ROOT_DIR >/dev/null
ROOT_DIR=$(pwd)

mkdir -p "$INT_DIR" && \
    cd "$INT_DIR" && \
    cmake -G "Unix Makefiles" -DCMAKE_BUILD_TYPE=$BUILD_TYPE -DTARGET_ARCH=$TARGET_ARCH -DCMAKE_COLOR_MAKEFILE=$COLORIZE "-DCMAKE_RUNTIME_OUTPUT_DIRECTORY=$OUT_DIR" "$ROOT_DIR" && \
    make

popd >/dev/null
