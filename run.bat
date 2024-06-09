@echo off

set "BATDIR=%~dp0"

set PROG=%1
shift
if "%PROG%"=="" goto :BADARGS

set RUNARGS=
:NEXTARG
    if "%~1"=="" goto :ENDARGS
    set "RUNARGS=%RUNARGS% %1"
    shift
    goto :NEXTARG
:ENDARGS

if "%PROG%"=="console"  call "%BATDIR%build.bat" console -nobuild -run %RUNARGS%
if "%PROG%"=="server"   call "%BATDIR%build.bat" server -nobuild -run %RUNARGS%
if "%PROG%"=="terminal" call "%BATDIR%build.bat" terminal -nobuild -run %RUNARGS%

goto :EOF

:BADARGS
echo Usage: run [program] (args to program)
echo server         Server
echo console        Command console
echo terminal       Terminal
