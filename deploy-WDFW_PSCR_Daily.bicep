/**
   Begin common prolog commands
   write-output "Begin common prolog"
   $name='WDFW_PSCR_Daily'
   $rg="rg_$name"
   $loc='westus2'
   write-output "End common prolog"
   End common prolog commands

   emacs F10
   Begin commands to deploy this file using Azure CLI with PowerShell
   write-output WaitForBuildComplete
   WaitForBuildComplete
   write-output "Previous build is complete. Begin deployment build."
   az.cmd deployment group create --name $name --resource-group $rg   --template-file  deploy-WDFW_PSCR_Daily.bicep --parameters '@deploy-WDFW_PSCR_Daily-parameters.json'
   write-output "end deploy"
   Get-AzResource -ResourceGroupName $rg | ft
   End commands to deploy this file using Azure CLI with PowerShell

   emacs ESC 2 F10
   Begin commands to shut down this deployment using Azure CLI with PowerShell
   write-output CreateBuildEvent.exe
   CreateBuildEvent.exe&
   write-output "begin shutdown"
   az.cmd deployment group create --mode complete --template-file ./clear-resources.json --resource-group $rg
   BuildIsComplete.exe
   Get-AzResource -ResourceGroupName $rg | ft
   write-output "showdown is complete"
   End commands to shut down this deployment using Azure CLI with PowerShell

   emacs ESC 3 F10
   Begin commands for one time initializations using Azure CLI with PowerShell
   az.cmd group create -l $loc -n $rg
   $id=(az.cmd group show --name $rg --query 'id' --output tsv)
   write-output "id=$id"
   $sp="spad_$name"
   az.cmd ad sp create-for-rbac --name $sp --sdk-auth --role contributor --scopes $id
   write-output "go to github settings->secrets and create a secret called AZURE_CREDENTIALS with the above output"
   write-output "{`n`"`$schema`": `"https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#`",`n `"contentVersion`": `"1.0.0.0`",`n `"resources`": [] `n}" | Out-File -FilePath clear-resources.json
   End commands for one time initializations using Azure CLI with PowerShell

 */


param location string = resourceGroup().location
param name string = uniqueString(resourceGroup().id)
@secure()
param testSecret0001 string

output testSecret0001 string = testSecret0001
