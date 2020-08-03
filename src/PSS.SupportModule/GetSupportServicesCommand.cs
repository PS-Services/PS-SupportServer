using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell.Commands;

namespace PSS.SupportModule
{
    [Cmdlet(VerbsCommon.Get,"SupportService")]
    [OutputType(typeof(ClientBundle))]
    public class GetSupportServicesCommand : PSCmdlet
    {
        protected override void BeginProcessing()
        {
        }

       protected override void ProcessRecord()
        {
            WriteObject(SupportClient.Current.Bundle);
        }

        protected override void EndProcessing()
        {
        }
    }
}
