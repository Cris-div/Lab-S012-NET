# Genera en .local para conservar las anotaciones agregadas a los modelos finales.
# Revisar el resultado antes de copiarlo a Models; no sobrescribe tu código.
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root
$env:DOTNET_CLI_HOME = Join-Path $root '.local/dotnet'
if (!$env:ConnectionStrings__Veterinaria) {
    $file = Join-Path $root '.local/connection.txt'
    if (!(Test-Path $file)) { throw 'Define ConnectionStrings__Veterinaria o ejecuta Start-Local.ps1 -DatabaseOnly.' }
    $env:ConnectionStrings__Veterinaria = (Get-Content $file -Raw).Trim()
}
dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw 'No se pudo restaurar dotnet-ef.' }
dotnet ef dbcontext scaffold 'Name=ConnectionStrings:Veterinaria' Npgsql.EntityFrameworkCore.PostgreSQL --project Veterinaria.Api --output-dir ../.local/scaffold --context VeterinariaContext --namespace Veterinaria.Api.Models --no-onconfiguring --no-pluralize --force
if ($LASTEXITCODE -ne 0) { throw 'Scaffold falló.' }
Write-Host 'Modelos generados en .local/scaffold. Revisa README y docs/INFORME.md para los ajustes finales.'
