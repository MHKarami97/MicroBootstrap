#!/bin/bash
echo Executing after success scripts on branch $Branch_Name
echo Triggering MyGet package build

cd src/MicroBootstrap
dotnet pack /p:PackageVersion=1.0.$TRAVIS_BUILD_NUMBER --no-restore -o .

echo Uploading MicroBootstrap package to MyGet using branch $Branch_Name

case "$Branch_Name" in
  "master")
    dotnet nuget push *.nupkg -k $MYGET_API_KEY -s https://www.myget.org/F/micro-bootstrap/api/v3/index.json
    ;;
  "develop")
    dotnet nuget push *.nupkg -k $MYGET_DEV_API_KEY -s https://www.myget.org/F/micro-bootstrap-dev/api/v3/index.json
    ;;    
esac

echo Uploading MicroBootstrap package to Nuget using branch $Branch_Name

case "$Branch_Name" in
  "master")
    dotnet nuget push *.nupkg -k $NUGET_API_KEY -s https://api.nuget.org/v3/index.json
    ;;
  "develop")
    dotnet nuget push *-dev.nupkg -k $NUGET_API_KEY -s https://api.nuget.org/v3/index.json
    ;;    
esac

