using System;
using System.Management.Automation;
using DotNet.Basics.Diagnostics;
using DotNet.Basics.Diagnostics.Console;

namespace Lib.Transform
{
    public class PsCmdletLogTarget : ConsoleLogTarget
    {
        private readonly PSCmdlet _cmdlet;

        public PsCmdletLogTarget(PSCmdlet cmdlet)
        {
            _cmdlet = cmdlet ?? throw new ArgumentNullException(nameof(cmdlet));
        }

        public override void WriteFormattedOutput(LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Error:
                    //write non terminating error - process continues
                    Console.WriteLine(message);
                    _cmdlet.WriteError(new ErrorRecord(
                        new Exception(message),
                        Guid.NewGuid().ToString(),
                        ErrorCategory.NotSpecified,
                        message));
                    break;
                case LogLevel.Warning:
                    _cmdlet.WriteWarning(message);
                    break;
                case LogLevel.Info:
                case LogLevel.Raw:
                case LogLevel.Success:
                    Console.WriteLine(message);
                    break;
                case LogLevel.Debug:
                    _cmdlet.WriteDebug(message);
                    break;
                case LogLevel.Verbose:
                    _cmdlet.WriteVerbose(message);
                    break;

                default:
                    throw new NotSupportedException($"LogLevel not supportd: {level}");
            }
        }
    }
}
