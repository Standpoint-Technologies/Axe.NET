using System;
using Axe.FieldParsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AxeTests.FieldParserTests
{
    [TestClass]
    public class GoogleParserTests
    {
        private GoogleParser _parser;


        [TestInitialize]
        public void InitializeParser()
        {
            _parser = new GoogleParser();
        }


        [TestMethod]
        public void TestSingle()
        {
            var fields = _parser.ParseFields("id");
            Assert.AreEqual(fields.Fields.Count, 1);
            Assert.IsTrue(!fields.NestedRings.Any());
            Assert.AreEqual(fields.Fields.First(), "id");
        }

        [TestMethod]
        public void TestSimpleList()
        {
            var fields = _parser.ParseFields("id,name");
            Assert.AreEqual(fields.Fields.Count, 2);
            Assert.IsTrue(!fields.NestedRings.Any());
            Assert.AreEqual(fields.Fields.ElementAt(0), "id");
            Assert.AreEqual(fields.Fields.ElementAt(1), "name");
        }

        [TestMethod]
        public void TestSimpleNestedObject()
        {
            var fields = _parser.ParseFields("id,nested(id2)");
            Assert.AreEqual(fields.Fields.Count, 1);
            Assert.AreEqual(fields.NestedRings.Count, 1);
            Assert.AreEqual(fields.Fields.First(), "id");
            Assert.AreEqual(fields.NestedRings.First().Key, "nested");

            var nested = fields.NestedRings.First().Value;
            Assert.IsTrue(nested.Fields.Count == 1);
            Assert.IsTrue(!nested.NestedRings.Any());
            Assert.AreEqual(nested.Fields.First(), "id2");
        }

        [TestMethod]
        public void TestMultipleNesting()
        {
            var fields = _parser.ParseFields("id,nested(id2,nested2(id3))");
            Assert.AreEqual(fields.Fields.Count, 1);
            Assert.AreEqual(fields.NestedRings.Count, 1);
            Assert.AreEqual(fields.Fields.First(), "id");
            Assert.AreEqual(fields.NestedRings.First().Key, "nested");

            var nested = fields.NestedRings.First().Value;
            Assert.AreEqual(nested.Fields.Count, 1);
            Assert.AreEqual(nested.NestedRings.Count, 1);
            Assert.AreEqual(nested.Fields.First(), "id2");
            Assert.AreEqual(nested.NestedRings.First().Key, "nested2");

            var nested2 = nested.NestedRings.First().Value;
            Assert.AreEqual(nested2.Fields.Count, 1);
            Assert.IsTrue(!nested2.NestedRings.Any());
            Assert.AreEqual(nested2.Fields.First(), "id3");
        }

        [TestMethod]
        public void TestMultipleNestedObjects()
        {
            var fields = _parser.ParseFields("id,nested(id2),nested2(id3)");
            Assert.AreEqual(fields.Fields.Count, 1);
            Assert.AreEqual(fields.NestedRings.Count, 2);
            Assert.AreEqual(fields.Fields.First(), "id");
            Assert.AreEqual(fields.NestedRings.First().Key, "nested");
            Assert.AreEqual(fields.NestedRings.ElementAt(1).Key, "nested2");

            var nested = fields.NestedRings.First().Value;
            Assert.IsTrue(nested.Fields.Count == 1);
            Assert.IsTrue(!nested.NestedRings.Any());
            Assert.AreEqual(nested.Fields.First(), "id2");

            var nested2 = fields.NestedRings.ElementAt(1).Value;
            Assert.IsTrue(nested2.Fields.Count == 1);
            Assert.IsTrue(!nested2.NestedRings.Any());
            Assert.AreEqual(nested2.Fields.First(), "id3");
        }
    }
}
