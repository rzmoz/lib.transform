using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNet.Basics.Collections;
using DotNet.Basics.Diagnostics;
using DotNet.Basics.IO;
using DotNet.Basics.Sys;

namespace Lib.Transform.Kernel
{
    public class ConfigFileTransformDispatcher
    {
        private readonly ILogger _log;
        private readonly IReadOnlyList<ConfigFileTransform> _transforms;

        public ConfigFileTransformDispatcher(ILogger log)
        {
            _log = log ?? new NullLogger();
            _transforms = new List<ConfigFileTransform>
            {
                new XdtConfigFileTransform(log),
                new JsonPatchConfigFileTransform(log)
            };
        }

        public bool Transform(DirPath rootDir, IReadOnlyCollection<string> environments)
        {
            //arrange
            var operations = InitOperations(rootDir, environments);

            _log.Info($@"
***********************************************************************************************

Config Transform Operations for {environments.JoinString()}

***********************************************************************************************");

            //Act
            var results = environments.SelectMany(env => operations[env].ForEachParallel(op => op.Transform(_log))).ToList();

            foreach (var result in results.Where(result => result.Success))
            {
                if (result.WasUpdated)
                {
                    _log.Success($@"Transform succeed for: {result}");
                    _log.Debug($"{nameof(result.ConfigFileAfterTransform).Highlight()}:\r\n{result.ConfigFileAfterTransform}");
                }
                else
                {
                    _log.Info($@"Transform succeed but config file was NOT updated for: {result}");
                    _log.Debug($@"{nameof(result.TransformFileContent).Highlight()}:
{result.TransformFileContent}
{nameof(result.ConfigFileAfterTransform).Highlight()}:
{result.ConfigFileAfterTransform}");
                }
                
            }


            foreach (var result in results.Where(result => result.Success))
                _log.Success($"Transform succeed for: {result}");

            foreach (var result in results.Where(result => result.Success && result.WasUpdated == false))
                _log.Info($"Transform succeed but config file was NOT updated for: {result}. See log for details");

            foreach (var result in results.Where(result => result.Success == false))
                _log.Error($"Transform failed for: {result}\r\n{result.Exception}");

            return true;
        }

        private ConcurrentDictionary<string, ConcurrentBag<TransformOperation>> InitOperations(DirPath rootDir, IReadOnlyCollection<string> environments)
        {
            var operations = new ConcurrentDictionary<string, ConcurrentBag<TransformOperation>>();

            foreach (var environment in environments)
            {
                operations.TryAdd(environment, new ConcurrentBag<TransformOperation>());

                _log.Verbose($"Scanning for environment: {environment.Highlight()}");

                _transforms.ForEachParallel(transform =>
                {
                    //find transform files for Environment
                    rootDir.EnumerateFiles($"*.{environment}.{transform.TransformFileExtension.TrimStart('.')}", SearchOption.AllDirectories).OrderBy(f => f.FullName())
                        .ForEachParallel(transformFile => //and convert to config operations
                        {
                            _log.Verbose($"Found transform file: {transformFile.RawPath}");

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
    }
}

