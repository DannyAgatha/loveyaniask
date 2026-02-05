
# Act4 Channel
$env:GAME_SERVER_CHANNEL_ID = "51"
$env:HTTP_LISTEN_PORT = "17551"
$env:GAME_SERVER_PORT= "8051"
$env:GAME_SERVER_CHANNEL_TYPE = "1"
Start-Process -FilePath "../dist/game-server/GameChannel.exe" -WorkingDirectory "../dist/game-server"