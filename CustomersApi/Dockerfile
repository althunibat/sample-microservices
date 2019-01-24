FROM microsoft/dotnet:2.2.1-aspnetcore-runtime-alpine
WORKDIR /app
COPY customer-api/ /app/
ARG ELK_URLS
ARG JAEGER_SERVICE_NAME
ARG JAEGER_AGENT_HOST
ARG JAEGER_AGENT_PORT
ARG JAEGER_SAMPLER_MANAGER_HOST_PORT
CMD ["dotnet", "CustomersApi.dll"]