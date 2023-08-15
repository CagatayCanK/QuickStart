[string]$MigrationName = $null
while ([string]::IsNullOrWhitespace($MigrationName)) {
	$MigrationName = Read-Host "Migration name"
}

dotnet ef migrations add $MigrationName --project ../Libraries/QuickStart.Data -s ../Presentation/QuickStart.Api/QuickStart.Api.csproj

dotnet ef database update --project ../Libraries/QuickStart.Data -s ../Presentation/QuickStart.Api/QuickStart.Api.csproj
