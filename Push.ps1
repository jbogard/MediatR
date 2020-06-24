$scriptName = $MyInvocation.MyCommand.Name
$artifacts = "./artifacts"

if ($Env:MYGET_MEDIATR_CI_API_KEY -eq $null) {
    Write-Host "${scriptName}: MYGET_MEDIATR_CI_API_KEY is empty or not set. Skipped pushing package(s)."
} else {
    Get-ChildItem $artifacts -Filter "*.nupkg" | ForEach-Object {
        Write-Host "$($scriptName): Pushing $($_.Name)"
        dotnet nuget push $_ --source https://www.myget.org/F/mediatr-ci/api/v3/index.json --api-key $Env:MYGET_MEDIATR_CI_API_KEY
        if ($lastexitcode -ne 0) {
            throw ("Exec: " + $errorMessage)
        }
    }
}
