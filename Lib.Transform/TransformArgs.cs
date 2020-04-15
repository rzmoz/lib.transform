using DotNet.Basics.Diagnostics;
using DotNet.Basics.Sys;

namespace Lib.Transform
{
    public class TransformArgs
    {
        public DirPath RootDir { get; set; }
        public LogLevel NoChangesLogLevel { get; set; }
    }
}
