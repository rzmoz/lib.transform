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
        [Alias("Target")]
        [ValidateNotNullOrEmpty]
        public string TargetJsonPath { get; set; }

        [Parameter(Position = 1, Mandatory = true, ParameterSetName = _byValueParamSetName)]
        [ValidateNotNullOrEmpty]
        [Alias("Value")]
        public string PatchValue { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = _byFileParamSetName)]
        [ValidateNotNullOrEmpty]
        [Alias("Source")]
        public string JsonPatchPath { get; set; }

        protected override void BeginProcessing()
        {
            TargetJsonPath = RootPath(TargetJsonPath);
            JsonPatchPath = RootPath(JsonPatchPath);
            if (File.Exists(TargetJsonPath) == false)
                throw new IOException($"Json file not found at: {TargetJsonPath}");
            if (File.Exists(JsonPatchPath) == false)
                throw new IOException($"JsonPatch file not found at: {JsonPatchPath}");
        }

        protected override void ProcessRecord()
        {
            var targetJson = TargetJsonPath.ToFile().ReadAllText();

            var jsonPatch = PatchValue;
            if (ParameterSetName == _byFileParamSetName)
                jsonPatch = JsonPatchPath.ToFile().ReadAllText();


            var patcher = new JsonPatchConfigFileTransform(Log);
            var result = patcher.Transform(jsonPatch, targetJson);
            if (result == null)
            {

            }
            else
                WriteObject(result);
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
}
