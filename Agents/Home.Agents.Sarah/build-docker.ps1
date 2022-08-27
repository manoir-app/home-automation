docker build . -f ..\..\..\Dockerfile.Release -t 192.168.2.184:5000/sarah:latest

docker tag 192.168.2.184:5000/sarah:latest 192.168.2.100:5000/sarah:latest

#docker push 192.168.2.184:5000/sarah:latest
docker push 192.168.2.100:5000/sarah:latest

docker image rm 192.168.2.184:5000/sarah:latest
docker image rm 192.168.2.100:5000/sarah:latest