#!/bin/bash

#!/bin/bash
 MYGET_ENV=""
 case "${GITHUB_REF#refs/heads/}" in
   "develop")
     MYGET_ENV="-dev"
     ;;
 esac
cd ./samples/Game-Microservices-Sample/Game.Services.EventProcessor/src/Game.Services.EventProcessor.API
dotnet publish  -c Release -o ./bin/Docker  --source https://api.nuget.org/v3/index.json --source https://www.myget.org/F/micro-bootstrap$MYGET_ENV/api/v3/index.json