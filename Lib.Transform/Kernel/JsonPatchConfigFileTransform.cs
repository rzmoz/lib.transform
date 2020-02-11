using System.IO;
using System.Text;
using DotNet.Basics.Diagnostics;
using DotNet.Basics.IO;
using DotNet.Basics.Sys;
using Manatee.Json;
using Manatee.Json.Patch;
using Manatee.Json.Serialization;


namespace Lib.Transform.Kernel
{
    public class JsonPatchConfigFileTransform : ConfigFileTransform
    {
        public JsonPatchConfigFileTransform(ILogger log) : base(log)
        { }

        public override string ConfigFileExtension { get; } = ".json";
        public override string TransformFileExtension { get; } = ".jsonpatch";
        public override bool Transform(FilePath transformFile, FilePath targetFile)
        {
            var serializer = new JsonSerializer();

            var rawPatchJson = transformFile.ReadAllText();
            var rawConfigDoc = targetFile.ReadAllText();

            try
            {
                //arrange
                var patchJsonValue = JsonValue.Parse(rawPatchJson);
                var jsonPatch = serializer.Deserialize<JsonPatch>(patchJsonValue);

                var configJsonValue = JsonValue.Parse(rawConfigDoc);
                //act
                var result = jsonPatch.TryApply(configJsonValue);
                if (result.Success)
                {
                    targetFile.WriteAllText(configJsonValue.GetIndentedString());
                    return true;
                }

                var errorMessage = new StringBuilder(7);
                errorMessage.AppendLine($"{result.Error.TrimEnd('.')} in {targetFile.FullName().Highlight()}");
                errorMessage.AppendLine($"Source json BEFORE PATCH:".Highlight());
                errorMessage.AppendLine(rawConfigDoc);
                errorMessage.AppendLine($"PATCH:".Highlight());
                errorMessage.AppendLine(rawPatchJson);
                errorMessage.AppendLine($"Json when PATCH ERROR occured:".Highlight());
                errorMessage.AppendLine(configJsonValue.GetIndentedString().Replace("\n", "\r\n"));
                Log.Error(errorMessage.ToString());
                return false;
            }
            catch (IOException e)
            {
                Log.Error(e.Message);
                return false;
            }
        }
    }
}
