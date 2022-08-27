docker build . -f ..\..\..\Dockerfile.Release -t 192.168.2.184:5000/alexandra:latest

docker tag 192.168.2.184:5000/alexandra:latest 192.168.2.100:5000/alexandra:latest

docker push 192.168.2.184:5000/alexandra:latest
docker push 192.168.2.100:5000/alexandra:latest

docker image rm 192.168.2.184:5000/alexandra:latest
docker image rm 192.168.2.100:5000/alexandra:latest