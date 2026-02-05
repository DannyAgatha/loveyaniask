# Ruta base donde se encuentran los proyectos en el directorio 'server'
$rootPath = "C:\Users\emrec\OneDrive\Belgeler\GitHub\NosEmre\server"

# Buscar y reemplazar 'net9.0' por 'net10.0' en todos los archivos .csproj en la ruta y subcarpetas
Get-ChildItem -Path $rootPath -Recurse -Filter *.csproj | ForEach-Object {
    $csprojPath = $_.FullName
    Write-Host "Procesando $csprojPath"

    # Leer el contenido del archivo .csproj como texto
    $content = Get-Content -Path $csprojPath -Raw

    # Reemplazar 'net9.0' por 'net10.0' en el texto
    $updatedContent = $content -replace "<TargetFramework>net9.0</TargetFramework>", "<TargetFramework>net10.0</TargetFramework>"

    # Guardar los cambios solo si hubo un reemplazo
    if ($updatedContent -ne $content) {
        Set-Content -Path $csprojPath -Value $updatedContent
        Write-Host "Actualizado $csprojPath a .NET 10"
    } else {
        Write-Host "No se encontr  'net9.0' en $csprojPath"
    }
}

Write-Host "Actualizaci n de todos los proyectos completada."
