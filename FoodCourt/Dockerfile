# Use .NET 8 SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./ 
RUN dotnet restore

# Copy everything and build the app
COPY . ./ 
RUN dotnet publish -c Release -o out

# Use .NET 8 ASP.NET runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expose port 80
EXPOSE 80

# Start your project
ENTRYPOINT ["dotnet", "FoodCourt.dll"]