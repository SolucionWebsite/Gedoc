FROM mcr.microsoft.com/dotnet/sdk  AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY GedocMvc.sln .
COPY aspnetapp/Gedoc.WebApp.csproj ./aspnetapp/
RUN dotnet restore

# copy everything else and build app
COPY aspnetapp/. ./aspnetapp/
WORKDIR /source/aspnetapp
RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "gedocwebapp.dll"]
