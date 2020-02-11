using System.IO;
using System.Management.Automation;
using DotNet.Basics.IO;
using Lib.Transform.Kernel;

namespace Lib.Transform
{
    [Cmdlet("Patch", "Json")]
    [OutputType(typeof(string))]
    public class PatchJsonCmdlet : LibTransformCmdlet
    {
        private const string _byValueParamSetName = "ByValue";
        private const string _byFileParamSetName = "ByFile";

        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [Alias("Json")]
        [ValidateNotNullOrEmpty]
        public string JsonFile { get; set; }

        [Parameter(Position = 1, Mandatory = true, ParameterSetName = _byValueParamSetName)]
        [ValidateNotNullOrEmpty]
        [Alias("Value")]
        public string PatchValue { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = _byFileParamSetName)]
        [ValidateNotNullOrEmpty]
        [Alias("Patch")]
        public string PatchFile { get; set; }

        protected override void BeginProcessing()
        {
            JsonFile = RootPath(JsonFile);
            PatchFile = RootPath(PatchFile);
            if (File.Exists(JsonFile) == false)
                throw new IOException($"Json file not found at: {JsonFile}");
            if (File.Exists(PatchFile) == false)
                throw new IOException($"JsonPatch file not found at: {PatchFile}");
        }

        protected override void ProcessRecord()
        {
            var json = JsonFile.ToFile().ReadAllText();

            var jsonPatch = ParameterSetName == _byValueParamSetName
                ? PatchValue
                : PatchFile.ToFile().ReadAllText();

            var patcher = new JsonPatchConfigFileTransform(Log);
            var result = patcher.Transform(jsonPatch, json);
            if (result != null)
                WriteObject(result);
        }
    }
}
