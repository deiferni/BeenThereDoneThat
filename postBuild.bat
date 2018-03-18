set DIR=%1\Release\GameData\BeenThereDoneThat\

if not exist %DIR% mkdir %DIR%
copy BeenThereDoneThat.dll %DIR%

cd %1
copy PayloadSeparatorPartModuleManager.cfg %DIR%

call test.bat