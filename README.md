# System Integration
This is a demo blood donation managment system which helps to make blood donation seemless
## Implementation 
We uses microservices with grpc for service to service communications and each service have it's own database 
## Build
Clone the repo and open with visual studio and make sure that the docker compose project is the stratup project then run the solution , thats build the docker images and start the project
## Tests

### AuthService
* [POST]   [SERVICE_URL]/login
* [POST]   [SERVICE_URL]/register

### Inventory Service 
* [POST]   [SERVICE_URL]/blood
* [PUT]    [SERVICE_URL]/blood/{id}
* [DELETE] [SERVICE_URL]/blood/{id}
* [GET]    [SERVICE_URL]/blood/{id}
* [GET]    [SERVICE_URL]/bloodS

### Donation Service 
* [POST]   [SERVICE_URL]/donation
* [PUT]    [SERVICE_URL]/donation/cancel/{id}
* [PUT]    [SERVICE_URL]/donation/collect/{id}
* [DELETE] [SERVICE_URL]/donation/{id}
* [GET]    [SERVICE_URL]/donation/{id}
* [GET]    [SERVICE_URL]/donations

