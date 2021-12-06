using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using DotNet.Basics.Diagnostics;
using DotNet.Basics.IO;
using DotNet.Basics.Sys;
using FluentAssertions;
using Xunit.Abstractions;
using Xunit;

namespace Lib.Transform.Tests
{
    public class ProgramTests
    {
        private readonly ConfigFileTransformDispatcher _dispatcher;
        private readonly DirPath _testRootDir;
        private readonly DirPath _artifactsSourceDir;

        public ProgramTests(ITestOutputHelper output)
        {
            var log = new NullLogger();
            log.MessageLogged += (lvl, msg, e) => { output.WriteLine($"[{lvl}] {msg}\r\n{e}".TrimEnd('\n').TrimEnd('\r')); };
            _dispatcher = new ConfigFileTransformDispatcher(log);
            _testRootDir = typeof(ProgramTests).Assembly.Location.ToFile().Directory;
            _artifactsSourceDir = _testRootDir.Add("Source");
        }

        [Fact]
        public void Main_NamedEnvironment_TransformIsApplied()
        {
            var artifactsDir = InitTestArtifacts();
            //arrange

            var testConfig = artifactsDir.ToFile("test.config");
            GetXmlValue(testConfig, "hello").Should().Be("World!");
            var testJson = artifactsDir.ToFile("test.json");
            GetJsonValue(testJson, "hello").Should().Be("World!");

            //act
            var result = _dispatcher.Transform(artifactsDir, "all");

            //Assert
            result.Should().BeTrue();
            GetXmlValue(testConfig, "hello").Should().Be("All!");
            GetJsonValue(testJson, "hello").Should().Be("All!");
        }
        [Fact]
        public void Main_MultipleEnvironments_EnvironmentsAreAppliedInOrder()
        {
            var artifactsDir = InitTestArtifacts();
            //arrange

            var testConfig = artifactsDir.ToFile("test.config");
            GetXmlValue(testConfig, "hello").Should().Be("World!");
            var testJson = artifactsDir.ToFile("test.json");
            GetJsonValue(testJson, "hello").Should().Be("World!");

            //act
            var result = _dispatcher.Transform(artifactsDir, "all","some.environment");

            //Assert
            result.Should().BeTrue();
            GetXmlValue(testConfig, "hello").Should().Be("Some.Environment!");
            GetJsonValue(testJson, "hello").Should().Be("Some.Environment!");
        }

        private string GetJsonValue(FilePath jsonPath, string key)
        {
            if (jsonPath.Exists() == false)
                throw new FileNotFoundException(jsonPath.FullName());
            var root = JsonDocument.Parse(jsonPath.ReadAllText()).RootElement;
            return root.GetProperty(key).GetString();
        }

        private DirPath InitTestArtifacts()
        {
            var stackTrace = new StackTrace();
            var testName = stackTrace.GetFrame(1).GetMethod().Name;
            var testArtifactsDir = _testRootDir.Add(testName);
            testArtifactsDir.CreateIfNotExists();
            testArtifactsDir.CleanIfExists();
            _artifactsSourceDir.CopyTo(testArtifactsDir, true);
            return testArtifactsDir;
        }

        private string GetXmlValue(FilePath xmlDoc, string key)
        {
            if (xmlDoc.Exists() == false)
                throw new FileNotFoundException(xmlDoc.FullName());
            var doc = XDocument.Load(xmlDoc.FullName());
            return doc.Root?.Elements("add").FirstOrDefault(e => (string)e.Attribute("key") == key)?.Attribute("value")?.Value;

        }
    }
}
