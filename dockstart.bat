dotnet publish Server -c release --runtime ubuntu.20.04-x64 
docker build -t chat . && docker create --name ServerChatCT -it -p 25565:25566 chat
docker start -i ServerChatCT