using System.IO;
using System.Management.Automation;
using DotNet.Basics.IO;
using Lib.Transform.Kernel;

namespace Lib.Transform
{
    [Cmdlet("Patch", "Directory")]
    [OutputType(typeof(string))]
    public class PatchDirectoryCmdlet : LibTransformCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [Alias("Root")]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        [Parameter(Position = 1, Mandatory = true)]
        public string[] Environments { get; set; }

        protected override void BeginProcessing()
        {
            Path = RootPath(Path);
            if (File.Exists(Path) == false)
                throw new IOException($"Xml file not found at: {Path}");
        }

        protected override void ProcessRecord()
        {
            var transformDispatcher = new ConfigFileTransformDispatcher(Log);
            transformDispatcher.Transform(Path.ToDir(), Environments);
        }
    }
}
