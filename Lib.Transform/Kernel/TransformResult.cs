using System;
using DotNet.Basics.Diagnostics;
using DotNet.Basics.IO;
using DotNet.Basics.Sys;

namespace Lib.Transform.Kernel
{
    public class TransformResult
    {
        public FilePath TransformFilePath { get; set; }
        public FilePath ConfigFilePath { get; set; }

        public bool Success { get; set; }
        public string ConfigFileBeforeTransform { get; set; }
        public string ConfigFileAfterTransform { get; set; }
        public string TransformFileContent { get; set; }
        public bool WasUpdated => ConfigFileBeforeTransform.Equals(ConfigFileAfterTransform) == false;
        public Exception Exception { get; set; } = null;

        public override string ToString()
        {
            return $"{TransformFilePath.Name.Highlight()} => {ConfigFilePath.FullName().Highlight()}";
        }
    }
}
