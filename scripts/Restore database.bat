@echo OFF
:: Make sure we are in current DIR
CD /D %~dp0
set currentDir=%~dp0%

ECHO Start local mongo-demon, then press any key to continue
pause

:: Custom settings
set mongoBinDir=C:\Mongo\bin

if not exist "%mongoBinDir%" (
	set mongoBinDir=C:\MongoDB\bin
) 

ECHO Deleting existing database

%mongoBinDir%\mongo.exe localhost/messageAudit --eval "db.dropDatabase();"

ECHO Importing

%mongoBinDir%\mongorestore --db auditHub "%currentDir%\databasedump"


pause