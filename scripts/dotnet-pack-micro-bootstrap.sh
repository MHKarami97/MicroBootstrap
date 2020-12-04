#!/bin/bash
echo Executing after success scripts on branch ${GITHUB_REF#refs/heads/}
echo Triggering MyGet package build
echo 1.0.$CI_RUN_ID

cd src/MicroBootstrap
dotnet pack /p:PackageVersion=1.0.$CI_RUN_ID  --no-restore -o .

echo Uploading MicroBootstrap package to MyGet using branch ${GITHUB_REF#refs/heads/}

case "${GITHUB_REF#refs/heads/}" in
  "master")
    dotnet nuget push *.nupkg -s https://www.myget.org/F/micro-bootstrap/api/v3/index.json -k $MYGET_API_KEY 
    ;;
  "develop")
    dotnet nuget push *.nupkg -s https://www.myget.org/F/micro-bootstrap-dev/api/v3/index.json -k $MYGET_DEV_API_KEY 
    ;;    
esac

echo Uploading MicroBootstrap package to Nuget using branch ${GITHUB_REF#refs/heads/}

case "${GITHUB_REF#refs/heads/}" in
  "master")
    dotnet nuget push *.nupkg -k $NUGET_API_KEY -s https://api.nuget.org/v3/index.json
    ;;
  "develop")
    dotnet nuget push *-dev.nupkg -k $NUGET_API_KEY -s https://api.nuget.org/v3/index.json
    ;;    
esac

