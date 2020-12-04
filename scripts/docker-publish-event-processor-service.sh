#!/bin/bash
DOCKER_ENV=''
DOCKER_TAG=''

echo Branch Name is $Branch_Name

case "$Branch_Name" in
  "master")
    DOCKER_TAG=latest
    ;;
  "develop")
    DOCKER_TAG=dev
    ;;    
esac
docker login -u $DOCKER_USERNAME -p $DOCKER_PASSWORD
docker build  -t game.services.event-processor:$DOCKER_TAG -f ./samples/Game-Microservices-Sample/Game.Services.EventProcessor/Dockerfile ./samples/Game-Microservices-Sample/Game.Services.EventProcessor
docker tag game.services.event-processor:$DOCKER_TAG $DOCKER_USERNAME/game.services.event-processor:$DOCKER_TAG
docker push $DOCKER_USERNAME/game.services.event-processor:$DOCKER_TAG