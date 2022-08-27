Get-Date | Write-Host 

docker build . -f ..\..\..\Dockerfile.Release -t 192.168.2.100:5000/home-public:dev

docker push 192.168.2.100:5000/home-public:dev

docker image rm 192.168.2.100:5000/home-public:dev