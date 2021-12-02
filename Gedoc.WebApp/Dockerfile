FROM mcr.microsoft.com/dotnet/aspnet 
ARG source
WORKDIR /inetpub/wwwroot
COPY ${source:-obj/Docker/publish} .
