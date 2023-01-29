@echo off
powershell -ExecutionPolicy Bypass -File "%~dp0CleanObjAndBin.ps1" -sourceRoot %~dp0
