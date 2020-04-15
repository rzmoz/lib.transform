using DotNet.Basics.Diagnostics;
using DotNet.Basics.Sys;

namespace Lib.Transform
{
    public abstract class ConfigFileTransform
    {
        protected ConfigFileTransform(ILogger log)
        {
            Log = log ?? Logger.NullLogger;
        }

        protected ILogger Log { get; }

        public abstract string ConfigFileExtension { get; }
        public abstract string TransformFileExtension { get; }

        public abstract bool Transform(FilePath transformFile, FilePath targetFile);
    }
}
