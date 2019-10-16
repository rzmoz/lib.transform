using System.Net;
using System.Threading.Tasks;
using DotNet.Basics.Cli;
using DotNet.Basics.Collections;
using DotNet.Basics.IO;

namespace Lib.Transform
{
    class Program
    {
        internal const string _rootDirFlag = "rootDir";

        static async Task<int> Main(string[] args)
        {
#if DEBUG
            args.PauseIfDebug();
#endif
            //init host
            var host = new CliHostBuilder(args, mappings =>
            {
                mappings.Add("root", _rootDirFlag);
                mappings.Add("dir", _rootDirFlag);
                mappings.Add("d", _rootDirFlag);
            }).Build();

            //init root dir
            var rootDir = host[_rootDirFlag, 0]?.ToDir();//try full name with fallback to first arg
            if (rootDir == null) //root dir not set
            {
                host.Log.Error($"{_rootDirFlag} parameter not set.");
                return (int)HttpStatusCode.BadRequest;
            }

            if (rootDir.Exists() == false) //root dir not found
            {
                host.Log.Error($"Directory Not Found: {rootDir.FullName()}");
                return (int)HttpStatusCode.NotFound;
            }

            host.Log.Debug($"Config Transform root dir set: {rootDir.FullName()}");

            if (host.Environments.None())
            {
                host.Log.Error($"No Environments specified");
                return (int)HttpStatusCode.BadRequest;
            }

            //run build
            return await host.RunAsync($"Config Transform",
                (config, log) =>
                {
                    var dispatcher = new ConfigFileTransformDispatcher(host.Log);
                    if (dispatcher.Transform(rootDir, host.Environments))
                        return Task.FromResult(0);
                    throw new CliException($"Config Transform failed. See log for details", LogOptions.ExcludeStackTrace);

                }).ConfigureAwait(false);
        }
    }
}
