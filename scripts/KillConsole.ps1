# Cambiar al directorio donde se encuentra el script
Set-Location -Path $PSScriptRoot

# Lista de procesos a detener
$List = 'Master', 'BazaarServer', 'FamilyServer', 'LoginServer', 'LogsServer', 'GameChannel', 'DataBaseServer', 'MailServer', 'RelationServer', 'TranslationsServer', 'DiscordNotifier', 'Scheduler', 'Communicator'

# Detener cada proceso en la lista
foreach ($fdp in $List)
{
    Stop-Process -Name $fdp -Force -ErrorAction SilentlyContinue
}

Write-Host "Todos los procesos han sido detenidos."
