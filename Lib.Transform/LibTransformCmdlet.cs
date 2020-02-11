using System.IO;
using System.Management.Automation;
using DotNet.Basics.Diagnostics;
using DotNet.Basics.IO;
using DotNet.Basics.Sys;
using PathInfo = System.Management.Automation.PathInfo;

namespace Lib.Transform
{
    public class LibTransformCmdlet : PSCmdlet
    {
        public LibTransformCmdlet()
        {
            Log = new Logger();
            Log.AddLogTarget(new PsCmdletLogTarget(this));
        }

        protected ILogger Log { get; }
        protected bool Debug => IsSwitchSet(nameof(Debug));
        protected bool Verbose => IsSwitchSet(nameof(Verbose));

        protected PathInfo CurrentDir => SessionState.Path.CurrentFileSystemLocation;

        protected string RootPath(string rawPath)
        {
            return Path.IsPathRooted(rawPath) 
                ? rawPath 
                : CurrentDir.Path.ToFile(rawPath.RemovePrefix(".\\")).RawPath;
        }

        protected bool IsSwitchSet(string name)
        {
            return MyInvocation.BoundParameters.ContainsKey(name)
                   && ((SwitchParameter)MyInvocation.BoundParameters[name]).ToBool();
        }
    }
}
