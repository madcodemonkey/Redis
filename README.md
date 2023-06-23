# Redis


# Docker Setup
In the [StackExchange.Redis documentation](https://stackexchange.github.io/StackExchange.Redis/Server), it references the [following docker image](https://hub.docker.com/_/redis/)

## Run the Docker container
```
docker run -p 6379:6379 --name some-redis -d redis:latest
```
OR
## Docker compose file for it 
Create a text file and name it: ```docker-compose.yml```
Put this in the text file:
```yml
version: '3.8'
services:
  redis:
    image: redis:latest
    container_name: some-redis
    restart: unless-stopped
    user: 0:0
    ports:
      - 6379:6379
```
You can run the docker compose command by navigating to the folder where the file is located and running these commands
- ```docker compose up -d```  Creates a container in detached mode so you run CLI commands as specified below.
- ```docker compose down```   Stops the container and removes it 

##  Run redis CLI commands within the Docker container
Attach to the running container using this command.  It assumes the name of the continaer is "some-redis"
```
docker exec -it some-redis redis-cli
```

# Redis Commands
- [Commands](https://redis.io/commands/)  
   - Appear to be executed with the execute command: ```db.ExecuteAsync("PING")```

## Useful CLI Commands
- Clear out the cache completely: ```FLUSHALL``` OR ```FLUSHDB```
- See all the keys: ```SCAN 0 COUNT 1000 MATCH "*"```
- Strings 
   - Get a key's value: ```GET <key name>```
- Hash
   - ```hkeys <keyname>```   (e.g., hkeys mykey)
   - ```hmget <keyname> <fieldname>```  (e.g., hmget mykey myfield)


# Application setup
You'll need to update your Redis connection string.  If you are using the Docker container, you can use this Redis connection string:
```
localhost:6379,ssl=false,abortConnect=False
```
If you are using Azure, you will need obtain a connection string from the portal.  For Azure Cahce  for Redis, it's under "Access Keys" and you want either the primary or secondary connection string.
