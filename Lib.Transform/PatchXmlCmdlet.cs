using System.IO;
using System.Management.Automation;
using DotNet.Basics.IO;
using Lib.Transform.Kernel;

namespace Lib.Transform
{
    [Cmdlet("Patch", "Xml")]
    [OutputType(typeof(string))]
    public class PatchXmlCmdlet : LibTransformCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [Alias("Target")]
        [ValidateNotNullOrEmpty]
        public string TargetXmlPath { get; set; }

        [Parameter(Position = 1, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        [Alias("Source")]
        public string XdtPath { get; set; }

        protected override void BeginProcessing()
        {
            TargetXmlPath = RootPath(TargetXmlPath);
            XdtPath = RootPath(XdtPath);
            if (TargetXmlPath.ToFile().Exists() == false)
                throw new IOException($"Xml file not found at: {TargetXmlPath.ToFile().FullName()}");
            if (XdtPath.ToFile().Exists() == false)
                throw new IOException($"Xdt file not found at: {XdtPath.ToFile().FullName()}");
        }

        protected override void ProcessRecord()
        {
            var patcher = new XdtConfigFileTransform(Log);
            patcher.Transform(XdtPath.ToFile(), TargetXmlPath.ToFile());
            WriteObject(TargetXmlPath);
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
}
