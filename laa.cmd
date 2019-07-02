@echo off
call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\Tools\VsDevCmd.bat" >nul
echo Making %1 large address aware.
editbin /nologo /largeaddressaware %1
