#!/bin/bash
SHDIR="$(cd "$(dirname "$0")" && pwd)"

SHOW_USAGE() {
    echo "Usage: build.sh command options"
    echo "restore      Restore all NuGet packages"
    echo "solution     Build the solution"
    echo "server       Build the server"
    echo "console      Build the console"
    echo "terminal     Build the terminal"
    echo
    echo "-debug       Use Debug configuration (default)"
    echo "-nobuild     Skip the build phase"
    echo "-release     Use Release configuration"
    echo "-run         Run the target after compilation"
}

BUILD_DOTNET() {
    if [ "$BUILD" = "YES" ]; then
        echo
        echo dotnet build --configuration $CONFIG "$1" /p:"SolutionDir=$SHDIR"
             dotnet build --configuration $CONFIG "$1" /p:"SolutionDir=$SHDIR"
        if [ $? -ne 0 ]; then
            exit 1
        fi
    fi
}

PUBLISH_DOTNET() {
    if [ "$BUILD" = "YES" ]; then
        echo
        echo dotnet publish "$1" /p:"SolutionDir=$SHDIR"
             dotnet publish "$1" /p:"SolutionDir=$SHDIR"
        if [ $? -ne 0 ]; then
            exit 1
        fi
    fi
}

RUN_PROGRAM() {
    if [ "$RUN" = "YES" ]; then
        $1 ${RUNARGS[@]}
        exit $?
    fi
}

RUN_DOTNET() {
    RUNARGS=("$1" "${RUNARGS[@]}")
    RUN_PROGRAM "dotnet"
}

TARGET=""
CONFIG=Debug
BUILD=YES
RUN=NO
declare -a RUNARGS=()

for arg in "$@"
do
    if [ "$RUN" = "YES" ]; then
        RUNARGS+=("$arg")
    else
        case $arg in
            solution | console | server | terminal | restore)
                TARGET="$arg"
                ;;
            -run)
                RUN=YES
                ;;
            -debug)
                CONFIG=Debug
                ;;
            -release)
                CONFIG=Release
                ;;
            -nobuild)
                BUILD=NO
                ;;
            *)
                SHOW_USAGE
                exit 1
                ;;
        esac
    fi
done

case $TARGET in
    console)
        BUILD_DOTNET "$SHDIR/Utility/Console/Console.csproj"
        RUN_DOTNET "$SHDIR/bin/$CONFIG/net8.0/Console.dll"
        ;;
    restore)
        dotnet restore "$SHDIR/VirtualRadar.sln"
        ;;
    server)
        BUILD_DOTNET   "$SHDIR/Apps/VirtualRadar.Server/VirtualRadar.Server.csproj"
        #PUBLISH_DOTNET "$SHDIR/Apps/VirtualRadar.Server/VirtualRadar.Server.csproj"
        RUN_DOTNET     "$SHDIR/bin/$CONFIG/net8.0/VirtualRadar.Server.dll"
        ;;
    solution)
        BUILD_DOTNET "$SHDIR/VirtualRadar.sln"
        RUNARGS="The run option makes no sense for the solution, ignoring it"
        RUN_PROGRAM "echo"
        ;;
    terminal)
        BUILD_DOTNET "$SHDIR/Utility/Terminal/Terminal.csproj"
        RUN_DOTNET "$SHDIR/bin/$CONFIG/net8.0/Terminal.dll"
        ;;
    *)
        SHOW_USAGE
        exit 1
        ;;
esac
