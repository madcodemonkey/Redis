# Redis


# Docker image
In the [StackExchange.Redis documentation](https://stackexchange.github.io/StackExchange.Redis/Server), it references the [following docker image](https://hub.docker.com/_/redis/)

## Run the Docker container
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

