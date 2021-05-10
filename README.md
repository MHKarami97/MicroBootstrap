[![Twitter URL](https://img.shields.io/badge/-@mehdi_hadeli-%231DA1F2?style=flat-square&logo=twitter&logoColor=ffffff)](https://twitter.com/mehdi_hadeli)
[![Linkedin Url URL](https://img.shields.io/badge/-mehdihadeli-blue?style=flat-square&logo=linkedin&logoColor=ffffff)](https://www.linkedin.com/in/mehdihadeli/)
[![blog](https://img.shields.io/badge/blog-dotnetuniversity.com-brightgreen?style=flat-square)](https://dotnetuniversity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg?style=flat-square)](https://opensource.org/licenses/MIT)
[![Tweet](https://img.shields.io/twitter/url/http/shields.io.svg?style=social)][tweet] 

# MicroBootstrap

MicroBootstrap is a framework for quickly and conveniently creating microservices on .NET Core including some infrastructures for Service Discovery, Load Balancing, Routing, Mediator, CQRS, Outbox Messages, Inbox Messages, MongoDb, Message Brokers (RabbitMQ, In-Memory), DDD, Tracing, Logging, Monitoning and Microservices.

[![master branch build status](https://api.travis-ci.org/mehdihadeli/MicroBootstrap.svg?branch=master)](https://travis-ci.org/mehdihadeli/MicroBootstrap)
[![Actions Status](https://github.com/mehdihadeli/MicroBootstrap/workflows/publish/badge.svg?branch=master)](https://github.com/mehdihadeli/MicroBootstrap/actions)
<a href="https://www.nuget.org/packages/MicroBootstrap/" alt="nuget package"><img src="https://buildstats.info/nuget/Microbootstrap?includePreReleases=true" /></a>

## Support ⭐
If you like my work, feel free to:

- ⭐ this repository. And we will be happy together :)
- [![Tweet](https://img.shields.io/twitter/url/http/shields.io.svg?style=social)][tweet] about MicroBootstrap

Thanks a bunch for supporting me!

[tweet]: https://twitter.com/intent/tweet?url=https://github.com/mehdihadeli/MicroBootstrap&text=MicroBootstrap%20is%20a%20framework%20for%20quickly%20and%20conveniently%20creating%20microservices%20on%20.NET%20Core&hashtags=dotnetcore,dotnet,csharp,microservices,netcore,aspnetcore,ddd,cqrs

## How to use?
For using [this package](https://www.nuget.org/packages/MicroBootstrap/) you can easily add it to your microservices project and use its infrastructure in your project to get rid of any annoying configuration and implement infrastructural stuff for microservice purpose. just add this command to add NuGet package:

``` bash
dotnet add package MicroBootstrap
```

## Scaling Microservices
 ----------------
 For scaling microservice in this project we have 2 option:
 * Using Consul and Fabio: for scaling our microservices we can use of consul and fabio and use of a customize algorithm for load balancing
 * Using Kubernetes: use of kubernetes for scaling but kubernetes limted to round robin aprouch for load balancing      
 
 For `load testing` we can use different tools but I use this tool [NBomber](https://nbomber.com/). you can use visual studio load test project or other solutions.
 
## How to start with Docker Compose?
----------------

Open `samples\Game-Microservices-Sample\deployments\docker-compose` directory and execute bellow command:

```
docker-compose -f infrastructure.yml up -d
```
you can also execute other scripts in [docker-compose](https://github.com/mehdihadeli/MicroBootstrap/tree/master/samples/Game-Microservices-Sample/deployments/docker-compose) folder like `mongo-rabbit-redis.yml` to run only theses infrastructure on your machine.

It will start the required infrastructure in the background. Then, you can start the services independently of each other via `dotnet run` or `./scripts/start.sh` command in each microservice or run them all at once using Docker that create and run needed docker images in compose file:

```
docker-compose -f services-local.yml up
```
or using pre-build docker images in docker hub with using this docker compose:

```
docker-compose -f services.yml up
```

## How to start with Kubernetes?
----------------
For setup your local environment for using kubernetes you can use different approuch but I personally perfer to use [K3s](https://k3s.io/) from rancher team, it is awsome like [rancher](https://rancher.com/) for kubernetes management :)        

Open `samples\Game-Microservices-Sample\deployments\k8s` directory, in this directory, there are two folders [infrastructure](https://github.com/mehdihadeli/MicroBootstrap/tree/master/samples/Game-Microservices-Sample/deployments/k8s/infrastructure) and [micro-services](https://github.com/mehdihadeli/MicroBootstrap/tree/master/samples/Game-Microservices-Sample/deployments/k8s/micro-services). in `infrastructure` folder exits all needed infrastructure for executing our microservices that we use `kubectl apply` for running them. for example for running `mongodb` on our cluster we should use these commands:

```
kubectl apply -f mongo-persistentvolumeclaim.yaml
kubectl apply -f mongo-deployment.yaml
kubectl apply -f mongo-service.yaml
```
In `micro-services` folder there are our services. for running our services on our cluster we should `kubectl apply` command for example:

```
kubectl apply -f messaging-service-deployment.yaml
kubectl apply -f messaging-service-service.yaml
```

## Thecnologies & Stack
----------------
* .Net Core 3.1
* RabbitMQ
* MongoDB
* Docker
* RESTEeas
* Consul
* Fabio
* Kubernetes
* Docker
* Redis
* Vault
* Jaeger
* Prometheus
* DDD
* Clean Architecture
* SignalR
* Seq
* Serilog

## Future Works
----------------
-  [ ] Integration with Service Mesh and Istio
-  [ ] Integration with Marten and Event Sourcing
-  [ ] Integration with MediateR
-  [ ] Integration with Kafka

## 🤝 Contributing
----------------
1. Fork it ( https://github.com/mehdihadeli/MicroBootstrap/fork )
2. Create your feature branch (`git checkout -b my-new-feature`)
3. Commit your changes (`git commit -am 'Add some feature'`)
4. Push to the branch (`git push origin my-new-feature`)
5. Create a new Pull Request 
