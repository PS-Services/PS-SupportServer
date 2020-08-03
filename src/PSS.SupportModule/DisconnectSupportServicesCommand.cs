using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell.Commands;

namespace PSS.SupportModule
{
    [Cmdlet(VerbsCommunications.Disconnect,"SupportService")]
    [OutputType(typeof(ClientBundle))]
    public class DisconnectSupportServicesCommand : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            WriteVerbose("Begin!");
        }

       protected override void ProcessRecord()
        {
            var supportClient = SupportClient.Current;

            supportClient.Disconnect();
        }

        protected override void EndProcessing()
        {
            WriteVerbose($"Disconnected.");
        }
    }
}
