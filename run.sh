#!/bin/bash
SHDIR="$(cd "$(dirname "$0")" && pwd)"
RUNARGS=${@:2}

SHOW_USAGE() {
    echo "Usage: run [program] (args to program)"
    echo "server         Web server"
    echo "console        Command console"
    echo "terminal       Terminal"
}

RUN_BUILD() {
    "$SHDIR/build.sh" "$1" -nobuild -run ${RUNARGS[@]}
}

if [ "$#" -eq 0 ]; then
    SHOW_USAGE
    exit 1
fi

case $1 in
    console)
        RUN_BUILD console
        ;;
    server)
        RUN_BUILD server
        ;;
    terminal)
        RUN_BUILD terminal
        ;;
esac
