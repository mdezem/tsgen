using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VLab.TSGen.Tests.Models;

namespace VLab.TSGen.Tests
{
    [TestClass]
    public class TsGenTests
    {
        public TsGenTests()
        {
        }

        [TestMethod]
        public void Should_gen_type_without_annotations()
        {
            var tsgen = new TsGen();
            var str = tsgen.GetTypeDeclaration<CustomerModel>();
            Debug.Write(str);
            Assert.IsTrue(str.Contains(" interface "));
        }

        [TestMethod]
        public void Shoul_gen_enum()
        {
            var tsgen = new TsGen();
            var str = tsgen.GetTypeDeclaration<Gender>();
            Debug.Write(str);
            Assert.IsTrue(str.Contains(" enum "));
        }

        [TestMethod]
        public void Should_gen_nullable_members()
        {
            var tsgen = new TsGen();
            var str = tsgen.GetTypeDeclaration<CustomerModel>();
            Debug.Write(str);
            // place the ?
            Assert.IsTrue(str.Contains("age?:"), "Fail to append ? to the member name");
            Assert.IsTrue(str.Contains("age?: number"), "Fail to get the underlying type of Nullable<> type");
        }

        [TestMethod]
        public void Should_resolve_enum_typenames()
        {
            var tsgen = new TsGen();
            var  str = tsgen.GetTypeDeclaration<CustomerModel>();
            Debug.Write(str);
            Assert.IsTrue(str.Contains("gender: Gender"));
        }

        [TestMethod]
        public void Should_resolve_enumerable_typenames()
        {
            var tsgen = new TsGen();
            var str = tsgen.GetTypeDeclaration<CustomerModel>();
            Debug.Write(str);
            Assert.IsFalse(str.Contains("IList`1"));
        }

        [TestMethod]
        public void Should_convert_Object_to_any()
        {
            var tsgen = new TsGen();
            var str = tsgen.GetTypeDeclaration<Prop>();
            Debug.Write(str);
            Assert.IsTrue(str.Contains("value: any"));
        }
    }
}
