using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNet.Basics.Collections;
using DotNet.Basics.Diagnostics;
using DotNet.Basics.IO;
using DotNet.Basics.Sys;

namespace Lib.Transform
{
    public class ConfigFileTransformDispatcher
    {
        private readonly ILogger _log;
        private readonly IReadOnlyList<ConfigFileTransform> _transforms;

        public ConfigFileTransformDispatcher(ILogger log)
        {
            _log = log ?? Logger.NullLogger;
            _transforms = new List<ConfigFileTransform>
            {
                new XdtConfigFileTransform(log),
                new JsonPatchConfigFileTransform(log)
            };
        }

        public bool Transform(DirPath rootDir, params string[] environments)
        {
            return Transform(rootDir, LogLevel.Verbose, environments);
        }
        public bool Transform(DirPath rootDir, LogLevel noChangesLogLevel, params string[] environments)
        {
            //arrange
            var operations = ScanForTransformOperations(rootDir, environments);

            //Act
            var results = environments.ForEach(env =>
            {
                LogHeader(LogLevel.Verbose, $"Config Transform Operations for {env.Highlight()}");

                return operations[env].ForEachParallel(op =>
                {
                    var result = op.Transform();

                    if (result.Success)
                    {
                        if (result.WasUpdated)
                        {
                            _log.Success($@"Transform succeed for: {result}");

                            
                            _log.Debug($"{nameof(result.ConfigFileAfterTransform).Highlight()}:\r\n{result.ConfigFileAfterTransform}");
                        }
                        else
                        {
                            _log.Write(noChangesLogLevel, $@"Transform succeed but config file was NOT updated for: {result}");
                            _log.Debug($@"{nameof(result.ConfigFileBeforeTransform).Highlight()}:
{result.ConfigFileAfterTransform}

{nameof(result.TransformFileContent).Highlight()}:
{result.TransformFileContent}");
                        }
                    }
                    else
                        _log.Error($"Transform failed for: {result}\r\n{result.Exception}");

                    return result;
                });
            }).SelectMany(result => result).ToList();

            LogHeader(LogLevel.Info, $"Config Transform Operations Overview");

            foreach (var result in results.Where(result => result.Success))
                _log.Success($"Transform succeed for: {result}");

            foreach (var result in results.Where(result => result.Success && result.WasUpdated == false))
                _log.Write(noChangesLogLevel, $"Transform succeed but config file was NOT updated for: {result}. See log for details");

            foreach (var result in results.Where(result => result.Success == false))
                _log.Error($"Transform failed for: {result}\r\n{result.Exception}");

            var anyFailures = results.Any(r => r.Success == false);
            return anyFailures == false;
        }

        private ConcurrentDictionary<string, ConcurrentBag<TransformOperation>> ScanForTransformOperations(DirPath rootDir, IReadOnlyCollection<string> environments)
        {
            var operations = new ConcurrentDictionary<string, ConcurrentBag<TransformOperation>>();

            foreach (var environment in environments)
            {
                operations.TryAdd(environment, new ConcurrentBag<TransformOperation>());

                _log.Info($"Scanning for environment: {environment.Highlight()}");

                _transforms.ForEachParallel(transform =>
                {
                    //find transform files for Environment
                    rootDir.EnumerateFiles($"*.{environment}.{transform.TransformFileExtension.TrimStart('.')}", SearchOption.AllDirectories).OrderBy(f => f.FullName())
                        .ForEachParallel(transformFile => //and convert to config operations
                        {
                            _log.Debug($"{transformFile.RawPath.Replace(environment, environment.Highlight())}");

                            //arrange
                            var targetName = transformFile.NameWoExtension
                                .Replace($".{environment}", string.Empty, StringComparison.InvariantCultureIgnoreCase)
                                .EnsureSuffix($".{transform.ConfigFileExtension.TrimStart('.')}");
                            var targetConfigFile = transformFile.Directory().ToFile(targetName);

                            var operation = new TransformOperation(transformFile, targetConfigFile, transform);
                            operation.AssertFilesExist(_log);
                            operations[environment].Add(operation);
                        });
                });
            }

            return operations;
        }

        private void LogHeader(LogLevel level, string msg)
        {
            _log.Write(level, $@"
***********************************************************************************************

                    {msg}

***********************************************************************************************");
        }
    }
}
