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
        [Alias("Xml")]
        [ValidateNotNullOrEmpty]
        public string XmlFile { get; set; }

        [Parameter(Position = 1, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        [Alias("Xdt")]
        public string XdtFile { get; set; }

        protected override void BeginProcessing()
        {
            XmlFile = RootPath(XmlFile);
            XdtFile = RootPath(XdtFile);
            if (File.Exists(XmlFile) == false)
                throw new IOException($"Xml file not found at: {XmlFile}");
            if (File.Exists(XdtFile) == false)
                throw new IOException($"Xdt file not found at: {XdtFile}");
        }

        protected override void ProcessRecord()
        {
            var patcher = new XdtConfigFileTransform(Log);
            patcher.Transform(XdtFile.ToFile(), XmlFile.ToFile());
            WriteObject(XmlFile);
        }
    }
}
