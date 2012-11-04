@echo off
call "%VS90COMNTOOLS%vsvars32.bat" >nul
echo Making %1 large address aware.
editbin /nologo /largeaddressaware %1
