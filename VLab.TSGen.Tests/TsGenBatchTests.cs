using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VLab.TSGen.Tests.Models;

namespace VLab.TSGen.Tests
{
    /// <summary>
    /// Summary description for TsGenBatchTests
    /// </summary>
    [TestClass]
    public class TsGenBatchTests
    {

        public TsGenBatchTests()
        {
            _tsbatch = new TsGenBatch();
            _tsbatch.Assemblies.Add(typeof(CustomerModel).Assembly);
            _tsbatch.Namespaces.Add(typeof(CustomerModel).Namespace);
        }

        private readonly TsGenBatch _tsbatch;

        [TestMethod]
        public void Should_filter_by_namespace()
        {
            // test the default instance (CustomerModel asm and namespace)
            Assert.IsTrue(_tsbatch.GetTypes().All(type => type.Namespace == typeof (CustomerModel).Namespace));
        }

        [TestMethod]
        public void Should_filter_by_suffix()
        {
            _tsbatch.Suffixes.Add("Model");
            Assert.IsTrue(_tsbatch.GetTypes().All(type => type.Name.EndsWith("Model")));
        }

        [TestMethod]
        public void Should_filter_by_prefix()
        {
            _tsbatch.Suffixes.Add("Customer");
            Assert.IsTrue(_tsbatch.GetTypes().All(type => type.Name.StartsWith("Customer")));
        }

        [TestMethod]
        public void Should_append_extra_classes()
        {
            _tsbatch.Assemblies.Add(typeof(ExtraModel).Assembly);
            _tsbatch.Classes.Add(typeof(ExtraModel).FullName);
            Assert.IsTrue(_tsbatch.GetTypes().Any(type => type == typeof(ExtraModel)));
        }

        [TestMethod]
        public void Should_append_module_name()
        {
            _tsbatch.ModuleName = "TestModule";
            var str = _tsbatch.GetDeclarations();
            Debug.Write(str);
            Assert.IsTrue(str.Contains("module TestModule {"));
        }
    }

    public class ExtraModel
    {
        public string ExtraModelId { get; set; }
        public string Comments { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
