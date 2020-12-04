#!/bin/bash
DOCKER_ENV=''
DOCKER_TAG=''

echo Branch Name is ${GITHUB_REF#refs/heads/}

case "${GITHUB_REF#refs/heads/}" in
  "master")
    DOCKER_TAG=latest
    ;;
  "develop")
    DOCKER_TAG=dev
    ;;    
esac
docker login -u $DOCKER_USERNAME -p $DOCKER_PASSWORD
docker build  -t game.services.api-gateway:$DOCKER_TAG -f ./samples/Game-Microservices-Sample/Game.APIGateway/Dockerfile ./samples/Game-Microservices-Sample/Game.APIGateway 
docker tag game.services.api-gateway:$DOCKER_TAG $DOCKER_USERNAME/game.services.api-gateway:$DOCKER_TAG
docker push $DOCKER_USERNAME/game.services.api-gateway:$DOCKER_TAG