

@echo off

set H=%KSPDIR%
set GAMEDIR=FuseBoxContinued
set VERSIONFILE=FuseBoxContinued

echo %H%

copy /Y "%1%2" "GameData\%GAMEDIR%\Plugins"
copy /Y %VERSIONFILE%.version GameData\%GAMEDIR%

xcopy /y /s /I GameData\%GAMEDIR% "%H%\GameData\%GAMEDIR%"

