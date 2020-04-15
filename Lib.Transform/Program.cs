using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotNet.Basics.Cli;
using DotNet.Basics.Collections;
using DotNet.Basics.Diagnostics;
using DotNet.Basics.IO;
using DotNet.Basics.Sys;

namespace Lib.Transform
{
    class Program
    {
        internal const string _rootDirFlag = "rootDir";
        internal const string _noChangesLogLevelFlag = "noChangesLogLevel";

        static async Task<int> Main(string[] args)
        {
#if DEBUG
            args.PauseIfDebug();
#endif
            //init host
            var host = new CliHostBuilder(args)
                .Build(mappings =>
                {
                    mappings.Add("path", _rootDirFlag);
                    mappings.Add("dir", _rootDirFlag);
                    mappings.Add("d", _rootDirFlag);
                    mappings.Add("noChangeLogLevel", _noChangesLogLevelFlag);
                });

            //init root dir
            var rootDir = host[_rootDirFlag, 0]?.ToDir();//try full name with fallback to first arg
            if (rootDir == null) //root dir not set
            {
                host.Log.Error($"{_rootDirFlag} parameter not set.");
                return (int)HttpStatusCode.BadRequest;
            }

            if (rootDir.Exists())
                host.Log.Debug($"Root Directory Dir Set: {rootDir.FullName()}");
            else
            {
                host.Log.Error($"Root Directory Not Found: {rootDir.FullName()}");
                return (int)HttpStatusCode.NotFound;
            }

            if (host.Environments.None())
            {
                host.Log.Error($"No Environments specified");
                return (int)HttpStatusCode.BadRequest;
            }

            var noChangesLogLevel = LogLevel.Warning;

            if (host.IsSet(_noChangesLogLevelFlag))
            {
                if (host[_noChangesLogLevelFlag].IsEnum<LogLevel>())
                    noChangesLogLevel = host[_noChangesLogLevelFlag].ToEnum<LogLevel>();
                else
                {
                    var logLevels = ((LogLevel[])Enum.GetValues(typeof(LogLevel))).Select(val => val.ToName()).JoinString();
                    host.Log.Error($"LogLevel for no changes not supported: {host[_noChangesLogLevelFlag]}. Valid values [{logLevels}]");
                    return (int)HttpStatusCode.BadRequest;
                }
            }

            //run build
            return await host.RunAsync($"Config Transform",
                (config, log) =>
                {
                    var dispatcher = new ConfigFileTransformDispatcher(host.Log);
                    if (dispatcher.Transform(rootDir, noChangesLogLevel, host.Environments.ToArray()))
                        return Task.FromResult(0);
                    throw new CliException($"Config Transform failed. See log for details");

                }).ConfigureAwait(false);
        }
    }
}
