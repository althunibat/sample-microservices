#!/bin/bash
tag="$1"
rm -rf customer-api/ frontend-web/ orders-api/ traffic-gen/
dotnet publish -c Release -o ../docker/customer-api ../CustomersApi/CustomersApi.csproj && \
dotnet publish -c Release -o ../docker/orders-api ../OrdersApi/OrdersApi.csproj && \
dotnet publish -c Release -o ../docker/frontend-web ../FrontendWeb/FrontendWeb.csproj && \
dotnet publish -c Release -o ../docker/traffic-gen ../TrafficGenerator/TrafficGenerator.csproj

docker build . -f ../CustomersApi/Dockerfile -t althunibat/customer-api:${tag} && \
docker build . -f ../FrontendWeb/Dockerfile -t althunibat/front-web:${tag} && \
docker build . -f ../OrdersApi/Dockerfile -t althunibat/orders-api:${tag} && \
docker build . -f ../TrafficGenerator/Dockerfile -t althunibat/traffic-gen:${tag}

docker push althunibat/traffic-gen:${tag} && \
docker push althunibat/front-web:${tag} && \
docker push althunibat/orders-api:${tag} && \
docker push althunibat/customer-api:${tag}