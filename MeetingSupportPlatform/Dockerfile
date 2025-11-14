# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["MeetingSupportPlatform.sln", "."]
COPY ["MSP.WebAPI/MSP.WebAPI.csproj", "MSP.WebAPI/"]
COPY ["MSP.Application/MSP.Application.csproj", "MSP.Application/"]
COPY ["MSP.Infrastructure/MSP.Infrastructure.csproj", "MSP.Infrastructure/"]
COPY ["MSP.Domain/MSP.Domain.csproj", "MSP.Domain/"]
COPY ["MSP.Shared/MSP.Shared.csproj", "MSP.Shared/"]

# Restore dependencies
RUN dotnet restore "MeetingSupportPlatform.sln"

# Copy all source code
COPY . .

# Build the application
RUN dotnet build "MeetingSupportPlatform.sln" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "MSP.WebAPI/MSP.WebAPI.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080
EXPOSE 8443

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production


# Run the application
ENTRYPOINT ["dotnet", "MSP.WebAPI.dll"]
