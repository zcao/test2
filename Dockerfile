# Use the official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy only the project file first
COPY ./ConsoleApp1/ConsoleApp1.csproj ./

# Restore dependencies as separate layer
RUN dotnet restore

# Now copy everything else
COPY ./ConsoleApp1/. ./

# Build the application
RUN dotnet publish -c Release -o out

# Use the official .NET runtime image for running the application
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime

# Set the working directory inside the container
WORKDIR /app

# Copy the built application from the build stage
COPY --from=build /app/out .

# Add labels for better maintainability
LABEL maintainer="your-email@example.com"
LABEL version="1.0"
LABEL description="Console Application 1"

# Set the entry point for the container
ENTRYPOINT ["dotnet", "ConsoleApp1.dll"]
