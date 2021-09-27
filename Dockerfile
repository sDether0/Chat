FROM ubuntu

RUN apt-get update
RUN apt install wget -y
RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb
RUN apt-get update
RUN apt-get install -y apt-transport-https 
RUN apt-get update 
RUN apt-get install -y dotnet-runtime-5.0

COPY /Server/bin/Release/net5.0/ubuntu.20.04-x64/publish/. /DocServer

WORKDIR /DocServer
CMD ./Server