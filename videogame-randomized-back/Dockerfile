# Use the official .NET 10 SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["videogame-randomized-back.csproj", "./"]
RUN dotnet restore "videogame-randomized-back.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "videogame-randomized-back.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "videogame-randomized-back.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "videogame-randomized-back.dll"]
