#!/bin/bash
echo Executing after success scripts on branch ${GITHUB_REF#refs/heads/}
echo Triggering MyGet package build

cd src/MicroBootstrap
dotnet pack /p:PackageVersion=1.0.$github.run_id --no-restore -o .

echo Uploading MicroBootstrap package to MyGet using branch ${GITHUB_REF#refs/heads/}

case "${GITHUB_REF#refs/heads/}" in
  "master")
    dotnet nuget push *.nupkg -k $MYGET_API_KEY -s https://www.myget.org/F/micro-bootstrap/api/v3/index.json
    ;;
  "develop")
    dotnet nuget push *.nupkg -k $MYGET_DEV_API_KEY -s https://www.myget.org/F/micro-bootstrap-dev/api/v3/index.json
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

