FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS build
WORKDIR /src
COPY ["Gedoc.WebApp/Gedoc.WebApp.csproj", "Gedoc.WebApp/"]
RUN dotnet restore "Gedoc.WebApp/Gedoc.WebApp.csproj"
COPY . .
WORKDIR "/src/Gedoc.WebApp"
RUN dotnet build "Gedoc.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gedoc.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gedoc.WebApp.dll"]
