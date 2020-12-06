#!/bin/bash
echo Executing after success scripts on branch ${Branch_Name}
echo Triggering MyGet package build
echo 1.2.${Github_ID}

 MYGET_ENV=""
 case "${Branch_Name}" in
   "develop")
     MYGET_ENV="-dev"
     ;;
 esac

cd src/MicroBootstrap
dotnet pack /p:PackageVersion=1.2.${Github_ID}${MYGET_ENV}  --no-restore -o .

echo Uploading MicroBootstrap package to MyGet using branch ${Branch_Name}

case "${Branch_Name}" in
  "master")
    dotnet nuget push *.nupkg -s https://www.myget.org/F/micro-bootstrap/api/v3/index.json -k $MYGET_API_KEY 
    ;;
  "develop")
    dotnet nuget push *.nupkg -s https://www.myget.org/F/micro-bootstrap-dev/api/v3/index.json -k $MYGET_DEV_API_KEY 
    ;;    
esac

echo Uploading MicroBootstrap package to Nuget using branch ${Branch_Name}

case "${Branch_Name}" in
  "master")
    dotnet nuget push *.nupkg -k $NUGET_API_KEY -s https://api.nuget.org/v3/index.json
    ;;
  "develop")
    dotnet nuget push *-dev.nupkg -k $NUGET_API_KEY -s https://api.nuget.org/v3/index.json
    ;;    
esac

