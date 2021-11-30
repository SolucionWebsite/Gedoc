FROM mcr.microsoft.com/dotnet/aspnet:5.0
ARG source
WORKDIR /inetpub/wwwroot
COPY ${source:-obj/Docker/publish} .

