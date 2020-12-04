#!/bin/bash
 MYGET_ENV=""
 case "$Branch_Name" in
   "develop")
     MYGET_ENV="-dev"
     ;;
 esac

dotnet publish ./samples/Game-Microservices-Sample/Game.Services.Messaging/src/Game.Services.Messaging.API -c Release -o ./samples/Game-Microservices-Sample/Game.Services.Messaging/src/Game.Services.Messaging.API/bin/Docker  --source https://api.nuget.org/v3/index.json --source https://www.myget.org/F/micro-bootstrap$MYGET_ENV/api/v3/index.json