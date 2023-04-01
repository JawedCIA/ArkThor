#Current Directory
$scriptPath = $MyInvocation.MyCommand.Path
$FolderPath=Split-Path $scriptPath -Parent
#Load ArkThor Ui Image
#docker load -i arkthor-ui
#Load ArkThor APU Image
#docker load -i arkthor-api
#Check For Folder Mapping
$FolderPathUploadedFiles =Join-Path $FolderPath "UploadedFiles";
$FolderPathArkThorDB =Join-Path $FolderPath "ArkThorDatabase";
 
If (-not (Test-Path $FolderPathUploadedFiles)) {
    # Folder does not exist, create it
    New-Item -Path $FolderPathUploadedFiles -ItemType Directory
    #Write-host "New Folder Created at '$FolderPathUploadedFiles'!" -f Green
}
Else {
    Write-host "Folder '$FolderPathUploadedFiles' already exists!" -f Red
}

If (-not (Test-Path $FolderPathArkThorDB)) {
    # Folder does not exist, create it
    New-Item -Path $FolderPathArkThorDB -ItemType Directory
    #Write-host "New Folder Created at '$FolderPathUploadedFiles'!" -f Green
}
Else {
    Write-host "Folder '$FolderPathArkThorDB' already exists!" -f Red
}

#Run Docker Compose WithOut RabbitMQ
docker-compose -f DockerCompose.yml up -d 