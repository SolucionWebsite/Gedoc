FROM mcr.microsoft.com/dotnet/sdk AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY Gedoc.WebApp.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet
WORKDIR /app
COPY --from=build-env /app /out.
ENTRYPOINT ["dotnet", "aspnetapp.dll"]
