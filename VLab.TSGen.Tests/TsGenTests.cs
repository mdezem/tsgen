using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VLab.TSGen.Tests.Models;

namespace VLab.TSGen.Tests
{
    [TestClass]
    public class TsGenTests
    {
        [TestMethod]
        public void Should_ignore_abstract_types()
        {
            var tsgen = new TsGen();
            var str = tsgen.GetTypeDeclaration<AbstractModel>();
            Assert.IsTrue(String.IsNullOrWhiteSpace(str));
        }

        [TestMethod]
        public void Should_ignore_static_types()
        {
            var tsgen = new TsGen();
            var str = tsgen.GetTypeDeclaration(typeof(StaticType));
            Assert.IsTrue(String.IsNullOrWhiteSpace(str));
        }

        [TestMethod]
        public void Should_gen_underscore_lowercase_member_name()
        {
            var tsgen = new TsGen();
            var str = tsgen.GetTypeDeclaration<ExtraModel>();
            Debug.Write(str);
            Assert.IsTrue(str.Contains("extra_model_id:"));
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
        public void Should_resolve_list_properties_typenames()
        {
            var tsgen = new TsGen();
            var str = tsgen.GetTypeDeclaration<CustomerModel>();
            Debug.Write(str);
            Assert.IsFalse(str.Contains("IList`1"));
        }

        [TestMethod]
        public void Should_resolve_enumerable_properties_typenames()
        {
            var tsgen = new TsGen();
            var str = tsgen.GetTypeDeclaration<ModelWithIEnumerable>();
            Debug.Write(str);
            Assert.IsFalse(str.Contains("IEnumerable`1"));
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
