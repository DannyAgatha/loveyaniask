# Cambiar al directorio donde se encuentra el script
Set-Location -Path $PSScriptRoot

# Translations keys #
$resources_src = '../../server-translations'
$files = Get-ChildItem $resources_src -Filter "*.yaml"

$resources_dst = "../dist/translation-server/translations"
New-Item -ItemType Directory -Force -Path $resources_dst
Copy-Item "$resources_src\en" -Destination $resources_dst -Force -Recurse
Copy-Item "$resources_src\fr" -Destination $resources_dst -Force -Recurse
Copy-Item "$resources_src\pl" -Destination $resources_dst -Force -Recurse
Copy-Item "$resources_src\es" -Destination $resources_dst -Force -Recurse
Copy-Item "$resources_src\it" -Destination $resources_dst -Force -Recurse
Copy-Item "$resources_src\de" -Destination $resources_dst -Force -Recurse
Copy-Item "$resources_src\tr" -Destination $resources_dst -Force -Recurse
Copy-Item "$resources_src\cz" -Destination $resources_dst -Force -Recurse
Write-Host "Copied $($files.Count) generic translations files to translations"

# Dat
$resources_src = '../../client-files/dat'
$files = Get-ChildItem $resources_src

$resources_dst = "../dist/game-server/resources/dat"
New-Item -ItemType Directory -Force -Path $resources_dst
Copy-Item "$resources_src\*" -Destination $resources_dst -Force -Recurse
Write-Host "Copied $($files.Count) .dat files to Game Server"

$resources_dst = "../dist/bazaar-server/resources/dat"
New-Item -ItemType Directory -Force -Path $resources_dst
Copy-Item "$resources_src\*" -Destination $resources_dst -Force -Recurse
Write-Host "Copied $($files.Count) .dat files to Bazaar Server"

$resources_dst = "../dist/discord-notifier/resources/dat"
New-Item -ItemType Directory -Force -Path $resources_dst
Copy-Item "$resources_src\*" -Destination $resources_dst -Force -Recurse
Write-Host "Copied $($files.Count) .dat files to Discord Notifier"

# Lang keys #
$resources_src = '../../client-files/lang'
$files = Get-ChildItem $resources_src

$resources_dst = "../dist/game-server/resources/lang"
New-Item -ItemType Directory -Force -Path $resources_dst
Copy-Item "$resources_src\*" -Destination $resources_dst -Force -Recurse
Write-Host "Copied $($files.Count) lang files to Game Server"

$resources_dst = "../dist/communicator/resources/lang"
New-Item -ItemType Directory -Force -Path $resources_dst
Copy-Item "$resources_src\*" -Destination $resources_dst -Force -Recurse
Write-Host "Copied $($files.Count) lang files to Communicator"

# Maps
$resources_src = '../../client-files/maps'
$files = Get-ChildItem $resources_src

$resources_dst = "../dist/game-server/resources/maps"
New-Item -ItemType Directory -Force -Path $resources_dst
Copy-Item "$resources_src\*" -Destination $resources_dst -Force -Recurse
Write-Host "Copied $($files.Count) maps files to Game Server"

$resources_dst = "../dist/communicator/resources/maps"
New-Item -ItemType Directory -Force -Path $resources_dst
Copy-Item "$resources_src\*" -Destination $resources_dst -Force -Recurse
Write-Host "Copied $($files.Count) maps files to Communicator"

# Config files
$server_config_src = '../../server-files'
$files = Get-ChildItem $server_config_src

$server_config_target = '../dist/game-server/config'
New-Item -ItemType Directory -Force -Path $server_config_target
Copy-Item "$server_config_src\*" -Destination $server_config_target -Force -Recurse
Write-Host "Copied $($files.Count) config files to Game Server"

$server_config_target = '../dist/family-server/config'
New-Item -ItemType Directory -Force -Path $server_config_target
Copy-Item "$server_config_src\*" -Destination $server_config_target -Force -Recurse
Write-Host "Copied $($files.Count) config files to Family Server"

$server_config_target = '../dist/translation-server/config'
New-Item -ItemType Directory -Force -Path $server_config_target
Copy-Item "$server_config_src\*" -Destination $server_config_target -Force -Recurse
Write-Host "Copied $($files.Count) config files to Translation Server"
