#Install-Module -Name PowerShellGet -Force -AllowClobber
#Install-Module -Name PackageManagement -Force -AllowClobber
#Install-Module -Name PowerShellGet -Force -AllowClobber

$baseURL="https://threatfox.abuse.ch/downloads/misp/"

$url = "https://threatfox.abuse.ch/downloads/misp/"
$response = Invoke-WebRequest -Uri $url
$content = $response.Content
$pattern = '[\w-]+\.json'


$matches = [regex]::Matches($content, $pattern)

$downloadableLinks = $matches | ForEach-Object {
    $_.Value    
} | Where-Object { $_ -notlike "*manifest.json" } | Select-Object -Unique


foreach($link in $downloadableLinks)
{
    $fileName = [System.IO.Path]::GetFileNameWithoutExtension($link)
    $completeURl=$baseURL+$link
     & python create_rules.py $completeURl threatfox_$fileName

}
