using System;
using DotNet.Basics.Diagnostics;
using DotNet.Basics.IO;
using DotNet.Basics.Sys;
using Microsoft.Web.XmlTransform;

namespace Lib.Transform
{
    public class XdtConfigFileTransform : ConfigFileTransform
    {
        public XdtConfigFileTransform(ILogDispatcher log) : base(log)
        { }

        public override string ConfigFileExtension { get; } = ".config";
        public override string TransformFileExtension { get; } = ".xdt";

        public override bool Transform(FilePath transformFile, FilePath targetFile)
        {
            try
            {
                var configFile = new XmlTransformableDocument { PreserveWhitespace = true };
                configFile.Load(targetFile.FullName());
                new XmlTransformation(transformFile.FullName()).Apply(configFile);
                configFile.Save(targetFile.FullName());
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return false;
            }
        }
    }
}
