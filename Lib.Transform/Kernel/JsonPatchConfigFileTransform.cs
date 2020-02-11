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

        public string Transform(string sourceJsonPatch, string targetJson)
        {
            var serializer = new JsonSerializer();

            //arrange
            var patchJsonValue = JsonValue.Parse(sourceJsonPatch);
            var jsonPatch = serializer.Deserialize<JsonPatch>(patchJsonValue);

            var targetJsonValue = JsonValue.Parse(targetJson);
            //act
            var result = jsonPatch.TryApply(targetJsonValue);
            if (result.Success)
                return targetJsonValue.GetIndentedString();

            var errorMessage = new StringBuilder(7);
            errorMessage.AppendLine($"{result.Error.TrimEnd('.')}");
            errorMessage.AppendLine($"Source json BEFORE PATCH:".Highlight());
            errorMessage.AppendLine(targetJson);
            errorMessage.AppendLine($"PATCH:".Highlight());
            errorMessage.AppendLine(sourceJsonPatch);
            errorMessage.AppendLine($"Json when PATCH ERROR occured:".Highlight());
            errorMessage.AppendLine(targetJsonValue.GetIndentedString().Replace("\n", "\r\n"));
            Log.Error(errorMessage.ToString());
            return null;
        }

        public override bool Transform(FilePath transformFile, FilePath targetFile)
        {
            try
            {
                var rawPatchJson = transformFile.ReadAllText();
                var rawConfigDoc = targetFile.ReadAllText();

                var updatedJson = Transform(rawPatchJson, rawConfigDoc);
                targetFile.WriteAllText(updatedJson);
                return true;
            }
            catch (IOException e)
            {
                Log.Error(e.Message);
                return false;
            }
        }
    }
}
