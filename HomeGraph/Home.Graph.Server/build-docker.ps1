Get-Date | Write-Host 

docker build . -f ..\..\..\Dockerfile.Release -t 192.168.2.100:5000/home-graph:latest

docker push 192.168.2.100:5000/home-graph:latest

docker image rm 192.168.2.100:5000/home-graph:latest