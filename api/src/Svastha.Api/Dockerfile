FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Svastha.Api/Svastha.Api.csproj", "src/Svastha.Api/"]
RUN dotnet restore "src/Svastha.Api/Svastha.Api.csproj"
COPY . .
RUN dotnet publish "src/Svastha.Api/Svastha.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Svastha.Api.dll"]
