# Phạm Quang Minh — 23103100201: Chạy demo Module 5 trên database riêng, không đổi appsettings chung.
param([string]$SqlServer = '.\SQLEXPRESS', [int]$Port = 5075)
$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot '../QuanLyKyThi_UNETI1_TI17A5HN/QuanLyKyThi_UNETI1_TI17A5HN.csproj'
$saved = @{}
$settings = @{
    ASPNETCORE_ENVIRONMENT = 'Development'
    ConnectionStrings__DefaultConnection = "Server=$SqlServer;Database=QuanLyKyThi_UNETI1_TI17A5HN_Module5Demo;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
    Module5__SeedDemo = 'true'
}
try {
    foreach ($key in $settings.Keys) {
        $saved[$key] = [Environment]::GetEnvironmentVariable($key, 'Process')
        [Environment]::SetEnvironmentVariable($key, $settings[$key], 'Process')
    }
    Write-Host "Module 5 demo: http://127.0.0.1:$Port"
    Write-Host 'Tai khoan: m5_canbo / Mat khau demo: Module5@123'
    dotnet run --project $project --no-launch-profile --urls "http://127.0.0.1:$Port"
} finally {
    foreach ($key in $settings.Keys) { [Environment]::SetEnvironmentVariable($key, $saved[$key], 'Process') }
}
