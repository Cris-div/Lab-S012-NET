param([switch]$DatabaseOnly)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root
$env:DOTNET_CLI_HOME = Join-Path $root '.local/dotnet'
New-Item -ItemType Directory -Force (Join-Path $root '.local') | Out-Null
$connectionFile = Join-Path $root '.local/connection.txt'
if (!(Test-Path $connectionFile)) {
    $password = [guid]::NewGuid().ToString('N')
    "Host=localhost;Port=55432;Database=veterinaria;Username=postgres;Password=$password" | Set-Content $connectionFile
}
$connection = (Get-Content $connectionFile -Raw).Trim()
$password = (($connection -split ';' | Where-Object { $_ -like 'Password=*' }) -replace '^Password=', '')
docker container inspect labnet-s12-postgres *> $null
if ($LASTEXITCODE -ne 0) {
    $mount = "$(Join-Path $root 'database'):/docker-entrypoint-initdb.d:ro"
    docker run --name labnet-s12-postgres -e "POSTGRES_PASSWORD=$password" -e POSTGRES_DB=veterinaria -p 127.0.0.1:55432:5432 -v $mount -d postgres:17-alpine
} else {
    docker start labnet-s12-postgres | Out-Null
}
if ($LASTEXITCODE -ne 0) { throw 'No fue posible iniciar PostgreSQL. Comprueba Docker Desktop.' }
$ready = $false
for ($i = 0; $i -lt 30; $i++) {
    docker exec labnet-s12-postgres pg_isready -U postgres -d veterinaria *> $null
    if ($LASTEXITCODE -eq 0) { $ready = $true; break }
    Start-Sleep -Seconds 1
}
if (!$ready) { throw 'PostgreSQL no está disponible.' }
if ($DatabaseOnly) { return }
$env:ConnectionStrings__Veterinaria = $connection
dotnet run --project Veterinaria.Api --launch-profile https
