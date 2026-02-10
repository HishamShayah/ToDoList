# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution file and project files
COPY *.sln .
COPY Api/*.csproj ./Api/
COPY Application/*.csproj ./Application/
COPY Core/*.csproj ./Core/
COPY Infrastructure/*.csproj ./Infrastructure/
COPY Api.IntegrationTests/*.csproj ./Api.IntegrationTests/
COPY Application.UnitTests/*.csproj ./Application.UnitTests/
# Restore NuGet packages
RUN dotnet restore

# Copy the entire source code
COPY . .

# Publish the API project
WORKDIR /app/Api
RUN dotnet publish -c Release -o /out

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

# Start the application
ENTRYPOINT ["dotnet", "Api.dll"]
