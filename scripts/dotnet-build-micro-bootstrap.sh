#!/bin/bash
MYGET_ENV=""
case "${GITHUB_REF#refs/heads/}" in
  "develop")
    MYGET_ENV="-dev"
    ;;
esac

dotnet build -c Release --no-cache