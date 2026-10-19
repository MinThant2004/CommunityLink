Write-Host "===================================================" -ForegroundColor Cyan
Write-Host " CommunityLink Presentation Database Setup" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan

$scriptPath = Join-Path $PSScriptRoot "CommunityLink.Database\Scripts\Seed_Presentation_Showcase_Data.sql"

if (-not (Test-Path $scriptPath)) {
    Write-Error "Could not find $scriptPath"
    exit 1
}

Write-Host "Executing SQL script against local SQL Server (CommunityLink)..." -ForegroundColor Yellow
& sqlcmd -S . -d CommunityLink -E -C -i $scriptPath

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nSUCCESS: Presentation database seeded successfully!" -ForegroundColor Green
    Write-Host "All accounts (User and Admin) password: '123456789'" -ForegroundColor Green
} else {
    Write-Host "`nERROR: Execution failed. Please ensure SQL Server is running." -ForegroundColor Red
}
