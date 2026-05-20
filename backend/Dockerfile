FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln ./
COPY src/VPS.Domain/*.csproj ./src/VPS.Domain/
COPY src/VPS.Application/*.csproj ./src/VPS.Application/
COPY src/VPS.Infrastructure/*.csproj ./src/VPS.Infrastructure/
COPY src/VPS.API/*.csproj ./src/VPS.API/
RUN dotnet restore

COPY . .
RUN dotnet publish src/VPS.API/VPS.API.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:5050
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5050

ENTRYPOINT ["dotnet", "VPS.API.dll"]
