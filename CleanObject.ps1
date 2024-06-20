$SourceRoot = $PSScriptRoot

Write-Output "Removing obj folders from $SourceRoot"
Dir -Path "$SourceRoot" obj -Directory -Recurse | Remove-Item -Force -Recurse

Write-Output "Removing bin folders from $SourceRoot"
Dir -Path "$SourceRoot" bin -Directory -Recurse | Remove-Item -Force -Recurse

Write-Output "Removing Distributables folder from $SourceRoot\DevOps"
Dir -Path "$SourceRoot\DevOps" Distributables -Directory -Recurse | Remove-Item -Force -Recurse

Write-Output "Removing TestResults folder from $SourceRoot"
Dir -Path "$SourceRoot" TestResults -Directory | Remove-Item -Force -Recurse
