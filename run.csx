using System;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;


public static void Run(TimerInfo myTimer, ILogger log)
{
    log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
    var armClient = new ArmClient(new DefaultAzureCredential());
    var agentExt = armClient.GetVirtualMachineScaleSetExtension("/subscriptions/088c3066-f1e5-4dd4-88ca-9e03cab2a11d/resourceGroups/AzurePipelines/providers/Microsoft.Compute/virtualMachineScaleSets/AgentPool-Standard_F2s_v2-EastUS2/extensions/Microsoft.Azure.DevOps.Pipelines.Agent");

    agentExt = agentExt.Get();
    var settings = agentExt.Data.Settings as IDictionary<string, object>;
    var agentDownloadUrl = settings["agentDownloadUrl"] as string;

    var forcedUrl = "https://vstsagentpackage.azureedge.net/agent/2.195.2/vsts-agent-linux-x64-2.195.2.tar.gz";

    log.LogInformation($"current agentDownloadUrl: {agentDownloadUrl}");
    if(agentDownloadUrl != forcedUrl)
    {
        settings["agentDownloadUrl"] = forcedUrl;
        var update = new VirtualMachineScaleSetExtensionUpdate()
        {
            Settings = settings
        };
        log.LogInformation($"forcing agentDownloadUrl to {forcedUrl}");
        try {
            agentExt.Update(update);
        }
        catch(Exception e)
        {
            if(e is ArgumentException && e.Message.Contains("expected Microsoft.Compute/virtualMachineScaleSets/extensions"))
            {
                log.LogInformation("Update succeeded (stupid client bug notwithstanding)");
            }
            else {
                log.LogError($"error updating: {e.ToString()}");
                throw;
            }
        }
        
    }
    else
        log.LogInformation("agentDownloadUrl does not need update");

}

