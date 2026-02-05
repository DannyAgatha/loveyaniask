# Cambiar al directorio donde se encuentra el script
Set-Location -Path $PSScriptRoot

$list = @(
    @('Master', 'Master.exe'), 
    @('database-server', 'DataBaseServer.exe'), 
    @('Bazaar-Server', 'BazaarServer.exe'),
    @('Family-Server', 'FamilyServer.exe'), 
    @('Login-Server', 'LoginServer.exe'), 
    @('Logs-Server', 'LogsServer.exe'),
    @('Mail-Server', 'MailServer.exe'),
    @('Relation-Server', 'RelationServer.exe'),
    @('translation-server', 'TranslationsServer.exe'),
    @('discord-notifier', 'DiscordNotifier.exe'),
    @('scheduler', 'Scheduler.exe'),
    @('communicator', 'Communicator.exe') # Añadido Communicator a la lista
)

$distFolder = "../dist/"
$gameChannelAmount = 2

$GAME_SERVER_CHANNEL_ID = 1
$HTTP_LISTEN_PORT = 17500

# Ejecutar cada servidor en la lista
foreach ($fdp in $list)
{    
    $path = $distFolder + $fdp[0] + '/' + $fdp[1]
    $directory = $distFolder + $fdp[0]
    Start-Process -FilePath $path -WorkingDirectory $directory
}

# Game Channel
for ($runChannel = 1; $runChannel -le $gameChannelAmount; $runChannel++)
{
    $env:GAME_SERVER_CHANNEL_TYPE = "0"
    $env:HTTP_LISTEN_PORT = $HTTP_LISTEN_PORT
    $env:GAME_SERVER_CHANNEL_ID = $GAME_SERVER_CHANNEL_ID
    Start-Process -FilePath "../dist/game-server/GameChannel.exe" -WorkingDirectory "../dist/game-server"
    
    $GAME_SERVER_CHANNEL_ID++
    $HTTP_LISTEN_PORT++
    
    Start-Sleep -Seconds 10 # Añadir un retraso de 10 segundos antes de abrir el siguiente canal
}

# Act4 Channel
$env:GAME_SERVER_CHANNEL_ID = "51"
$env:HTTP_LISTEN_PORT = "17551"
$env:GAME_SERVER_PORT = "8051"
$env:GAME_SERVER_CHANNEL_TYPE = "1"
Start-Process -FilePath "../dist/game-server/GameChannel.exe" -WorkingDirectory "../dist/game-server"
