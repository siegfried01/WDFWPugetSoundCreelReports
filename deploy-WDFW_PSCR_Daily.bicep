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
   az.cmd deployment group create --name $name --resource-group $rg   --template-file  deploy-WDFW_PSCR_Daily.bicep
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
param fileShare bool = false
param table bool = false

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-05-01' = {
  name: '${name}stgacc'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    dnsEndpointType: 'Standard'
    defaultToOAuthAuthentication: false
    publicNetworkAccess: 'Enabled'
    allowCrossTenantReplication: false
    isLocalUserEnabled: true
    isSftpEnabled: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: true
    allowSharedKeyAccess: true
    isHnsEnabled: true
    networkAcls: {
      bypass: 'AzureServices'
      virtualNetworkRules: []
      ipRules: []
      defaultAction: 'Allow'
    }
    supportsHttpsTrafficOnly: false
    encryption: {
      services: {
        file: {
          keyType: 'Account'
          enabled: true
        }
        blob: {
          keyType: 'Account'
          enabled: true
        }
      }
      keySource: 'Microsoft.Storage'
    }
    accessTier: 'Cool'
  }

  resource blobSvcs 'blobServices@2022-05-01' = {
    name: 'default'
    properties: {
      cors: {
        corsRules: []
      }
      deleteRetentionPolicy: {
        allowPermanentDelete: true
        enabled: false
      }
    }
    resource siegblobcontainer 'containers@2022-05-01' = {
      name: '${name}-siegblobcontainer'
      properties: {
        immutableStorageWithVersioning: {
          enabled: false
        }
        defaultEncryptionScope: '$account-encryption-key'
        denyEncryptionScopeOverride: false
        publicAccess: 'None'
      }
    }
  }

  resource fileShareSvcs 'fileServices@2022-05-01' = if (fileShare) {
    name: 'default'
    properties: {
      protocolSettings: {
        smb: {
        }
      }
      cors: {
        corsRules: []
      }
      shareDeleteRetentionPolicy: {
        enabled: true
        days: 1
      }
    }
    resource siegFileShare 'shares@2022-05-01' = {
      name: 'siegfileshare'
      properties: {
        accessTier: 'Cool'
        shareQuota: 5120
        enabledProtocols: 'SMB'
      }
    }
  }

  resource queueSvcs 'queueServices@2022-05-01' = {
    name: 'default'
    properties: {
      cors: {
        corsRules: []
      }
    }
    resource storageAccounts_pzveowxpswgjastgacc_name_default_siegqueue 'queues@2022-05-01' = {
      name: 'siegqueue'
      properties: {
        metadata: {
        }
      }
    }
  }

  resource tableSvcs 'tableServices@2022-05-01' = if (table)  {
    name: 'default'
    properties: {
      cors: {
        corsRules: []
      }
    }

    resource siegtable 'tables@2022-05-01' = {
      name: '${name}-siegtable'
      properties: {
      }
    }
  }
}
