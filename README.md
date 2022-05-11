# masstransit-helloworld

My implementation of distributed dotnet tutorial from @MantaresBV
[Distributed .Net MassTransit and RabbitMQ](https://medium.com/mantares/distributed-net-application-with-masstransit-and-rabbitmq-97cc467179e4)

My implementation uses the lasted dotnet 6 Min Web Api, uses a `BackgroundService` for the 'Service', and uses [Redis](https://redis.io/docs/getting-started/) in place of MySql.

To setup the project, create a solution with the following four components:

    dotnet new sln -n masstran.hellow
    
    dotnet new web -o src\Gateway
    dotnet sln add src\Gateway
    
    dotnet new classlib -o src\Messages
    dotnet sln add src\Messages
    
    dotnet new classlib -o src\Models
    dotnet sln add src\Models
    
    dotnet new worker -o src\Service
    dotnet sln add src\Service

Included is a docker-compose file that sets up Redis and RabbitMQ (using the MassTransit image), along with some script lines to bring the services up and down. 

My particular example runs the web api ('Gateway') and worker listener ('Service') locally. They could easily be run as two more containers managed by the same docker-compose file.
