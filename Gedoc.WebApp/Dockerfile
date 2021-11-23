#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat 

FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2019
ARG source
WORKDIR /inetpub/wwwroot
COPY ${source:-obj/Docker/publish} .

FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2019 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2019 AS build
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