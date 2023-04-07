copy %localappdata%\VirtualRadar\StandingData.sqb
sqlite3 StandingData.sqb < PrepareStandingDataSqb.sql

echo.
echo You will need to clean the test project before this will get picked up
pause