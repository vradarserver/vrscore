@echo off
set "BATDIR=%~dp0"

rem ##################################################
rem ## Parse arguments

:ARGS
set TARGET=""
set CONFIG=Debug
set BUILD=YES
set RUN=NO
set BADARG=""
set RUNARGS=
:NEXTARG
    if "%1"=="" goto :ENDARGS
    if "%RUN%"=="YES" goto :ADDRUN

    set BADARG=BAD
    if "%1"=="solution"      set BADARG=OK & set TARGET=SLN
    if "%1"=="console"       set BADARG=OK & set TARGET=CONSOLE
    if "%1"=="server"        set BADARG=OK & set TARGET=SERVER
    if "%1"=="terminal"      set BADARG=OK & set TARGET=TERMINAL
    if "%1"=="restore"       set BADARG=OK & set TARGET=RESTORE

    if "%1"=="-debug"        set BADARG=OK & set CONFIG=Debug
    if "%1"=="-nobuild"      set BADARG=OK & set BUILD=NO
    if "%1"=="-release"      set BADARG=OK & set CONFIG=Release
    if "%1"=="-run"          set BADARG=OK & set RUN=YES

    if %BADARG%==BAD goto :USAGE
    shift
    goto :NEXTARG

:ADDRUN
    set "RUNARGS=%RUNARGS% %1"
    shift
    goto :NEXTARG
    
:ENDARGS
set "COMBIN=%BATDIR%bin\%CONFIG%\net8.0"

if "%TARGET%"=="CONSOLE"    goto :CONSOLE
if "%TARGET%"=="SERVER"     goto :SERVER
if "%TARGET%"=="TERMINAL"   goto :TERMINAL
if "%TARGET%"=="RESTORE"    goto :RESTORE
if "%TARGET%"=="SLN"        goto :SLN

:USAGE

echo Usage: build command options
echo restore      Restore all NuGet packages
echo solution     Build the solution
echo server       Build the server
echo console      Build the console
echo terminal     Build the terminal
echo.
echo -debug        Use Debug configuration (default)
echo -nobuild      Skip the build phase
echo -release      Use Release configuration
echo -run          Run the target after compilation

if %BADARG%==OK goto :EOF
echo.
echo Unknown parameter "%1"
goto :EOF

rem ##################################################
rem ## Common actions

:BUILD
    if %BUILD%==NO goto :NOBUILD
    echo.
    echo dotnet build --configuration %CONFIG% "%PROJ%" /p:"SolutionDir=%BATDIR%"
    dotnet build --configuration %CONFIG% "%PROJ%" /p:"SolutionDir=%BATDIR%"
    if ERRORLEVEL 1 goto :EOF
:NOBUILD
    exit /b 0

:PUBLISH
    IF %BUILD%==NO goto :NOBUILD
    echo.
    echo dotnet publish "%PROJ%" /p:"SolutionDir=%BATDIR%"
    dotnet publish "%PROJ%" /p:"SolutionDir=%BATDIR%"
    if ERRORLEVEL 1 goto :EOF
    exit /b 0

rem ##################################################
rem ## Pseudo targets

:RESTORE
    dotnet restore "%BATDIR%VirtualRadar.sln"
    goto :EOF

rem ##################################################
rem ## Build targets

:CONSOLE
    set "PROJ=%BATDIR%Utility\Console\Console.csproj"
    call :BUILD
    if ERRORLEVEL 1 goto :EOF
    if "%RUN%"=="YES" "%COMBIN%\Console.exe" %RUNARGS%
    goto :EOF

:SERVER
    set "PROJ=%BATDIR%Apps\VirtualRadar.Server\VirtualRadar.Server.csproj"
    call :BUILD
    if ERRORLEVEL 1 goto :EOF
    rem At the moment the csproj has a publish step at the end of it, but leaving
    rem this here just in case that causes trouble and we need to go back to a
    rem separate publish step...
    rem call :PUBLISH
    rem if ERRORLEVEL 1 goto :EOF
    if "%RUN%"=="YES" "%COMBIN%\VirtualRadar.Server.exe" %RUNARGS%
    goto :EOF

:SLN
    set "PROJ=%BATDIR%VirtualRadar.sln"
    call :BUILD
    if ERRORLEVEL 1 goto :EOF
    if "%RUN%"=="YES" echo "The run option doesn't make sense for the solution, ignoring it"
    goto :EOF

:TERMINAL
    set "PROJ=%BATDIR%Utility\Terminal\Terminal.csproj"
    call :BUILD
    if ERRORLEVEL 1 goto :EOF
    if "%RUN%"=="YES" "%COMBIN%\Terminal.exe" %RUNARGS%
    goto :EOF
