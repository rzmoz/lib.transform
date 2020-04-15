using System;
using DotNet.Basics.Diagnostics;
using DotNet.Basics.IO;
using DotNet.Basics.Sys;

namespace Lib.Transform
{
    public class TransformOperation
    {
        public TransformOperation(FilePath transformFile, FilePath configFile, ConfigFileTransform configTransform)
        {
            TransformFile = transformFile;
            ConfigFile = configFile;
            ConfigTransform = configTransform;
        }

        public FilePath TransformFile { get; }
        public FilePath ConfigFile { get; }
        public ConfigFileTransform ConfigTransform { get; }

        public bool AssertFilesExist(ILogger log)
        {
            var allExists = TransformFile.Exists();
            if (allExists == false)
                log.Error($"{nameof(TransformFile)} not found: {TransformFile.FullName().Highlight()}");
            allExists = allExists && ConfigFile.Exists();
            if (allExists == false)
                log.Error($"{nameof(ConfigFile)} not found: {ConfigFile.FullName().Highlight()}");
            allExists = allExists && ConfigTransform != null;//should never fail here. Then it's a code error and not application error

            return allExists;
        }

        public TransformResult Transform()
        {
            var result = new TransformResult
            {
                TransformFilePath = TransformFile,
                ConfigFilePath = ConfigFile,
                TransformFileContent = TransformFile.ReadAllText(),
                ConfigFileBeforeTransform = ConfigFile.ReadAllText(),
            };
            try
            {
                result.Success = ConfigTransform.Transform(TransformFile, ConfigFile);
                result.ConfigFileAfterTransform = ConfigFile.ReadAllText();
            }
            catch (Exception e)
            {
                result.Exception = e;//don't throw here. Let dispatcher handle errors
            }
            return result;
        }

        public override string ToString()
        {
            return $"{TransformFile.Name.Highlight()} => {ConfigFile.RawPath.Highlight()}";
        }
    }
}
