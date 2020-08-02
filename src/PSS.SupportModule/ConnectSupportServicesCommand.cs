using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell.Commands;

namespace PSS.SupportModule
{
    [Cmdlet(VerbsCommunications.Connect,"SupportService")]
    [OutputType(typeof(ClientBundle))]
    public class ConnectSupportServicesCommand : PSCmdlet
    {
        [Parameter(
            Mandatory = false,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public Uri Uri { get; set; }

        // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
        protected override void BeginProcessing()
        {
            InvokeCommand.InvokeScript(true, ScriptBlock.Create("Import-Module Microsoft.PowerShell.Utility"), null);
            WriteVerbose("Begin!");
        }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            //var powerShell = new GetPSSessionCommand().Invoke<PssCmdlet>().FirstOrDefault();

                var supportClient = new SupportClient(this);

                supportClient.Connect(Uri);

                Bundle = supportClient.Bundle;

                WriteObject(supportClient.Bundle);
        }

        public ClientBundle Bundle { get; set; }

        public void WriteError(Exception ex)
        {
            base.WriteError(new ErrorRecord(ex, null, ErrorCategory.NotSpecified, this));
        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            WriteVerbose($"IsConnected to {this.Uri.AbsoluteUri}: {Bundle.IsConnected}");
        }
    }
}
