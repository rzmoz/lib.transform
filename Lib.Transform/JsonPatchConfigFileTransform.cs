using System.IO;
using System.Text;
using System.Text.Json;
using DotNet.Basics.Diagnostics;
using DotNet.Basics.IO;
using DotNet.Basics.Sys;
using Json.Patch;

namespace Lib.Transform
{
    public class JsonPatchConfigFileTransform : ConfigFileTransform
    {
        public JsonPatchConfigFileTransform(ILogger log) : base(log)
        { }

        public override string ConfigFileExtension { get; } = ".json";
        public override string TransformFileExtension { get; } = ".jsonpatch";
        public override bool Transform(FilePath transformFile, FilePath targetFile)
        {

            var rawPatchJson = transformFile.ReadAllText();
            var rawConfigDoc = targetFile.ReadAllText();

            try
            {
                //arrange
                var jsonPatch = JsonSerializer.Deserialize<JsonPatch>(rawPatchJson)!;

                var configJsonValue = JsonDocument.Parse(rawConfigDoc).RootElement;
                //act
                var actual = jsonPatch.Apply(configJsonValue);
                
                if (actual.Error == null)
                {
                    targetFile.WriteAllText(actual.Result.GetRawText());
                    return true;
                }

                var errorMessage = new StringBuilder(7);
                errorMessage.AppendLine($"{actual.Error.TrimEnd('.')} in {targetFile.FullName().Highlight()}");
                errorMessage.AppendLine($"Source json BEFORE PATCH:".Highlight());
                errorMessage.AppendLine(rawConfigDoc);
                errorMessage.AppendLine($"PATCH:".Highlight());
                errorMessage.AppendLine(rawPatchJson);
                errorMessage.AppendLine($"Json when PATCH ERROR occured:".Highlight());
                errorMessage.AppendLine(configJsonValue.GetRawText().Replace("\n", "\r\n"));
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
