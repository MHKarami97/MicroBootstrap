#!/bin/bash
MYGET_ENV=""
echo Branch Name is ${Branch_Name}
 case "${Branch_Name}" in
   "develop")
     MYGET_ENV="-dev"
     ;;
 esac
cd ./samples/Game-Microservices-Sample/Game.APIGateway/src/Game.API
dotnet build -c Release --no-cache --source https://api.nuget.org/v3/index.json --source https://www.myget.org/F/micro-bootstrap$MYGET_ENV/api/v3/index.json