using System;
using System.Collections.Generic;
using System.Linq;
using Axe.ExpressionBuilders;
using Axe.FieldParsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AxeTests.ExpressionBuilderTests
{
    [TestClass]
    public class CollectionTests
    {
        private DefaultExpressionBuilder _builder;
        private Axe.FieldRing _fieldRing;
        private Axe.AxeProfile _profile;


        [TestInitialize]
        public void InitializeExpressionBuilder()
        {
            _builder = new DefaultExpressionBuilder();
            _fieldRing = new Axe.FieldRing()
            {
                Fields = new List<string>() { "id" },
                NestedRings = new Dictionary<string, Axe.FieldRing>()
                {
                    {"items", new Axe.FieldRing()
                        {
                            Fields = new List<string>() { "id" }
                        }
                    }
                }
            };
            _profile = new Axe.AxeProfile()
            {
                ExpressionBuilder = _builder,
                ExtendTypesDynamically = true,
                FieldParser = new GoogleParser(),
                IgnoreCase = true
            };
        }


        [TestMethod]
        public void TestEnumerable()
        {
            var expression = _builder.BuildExpression<TestEnumerableClass>(_fieldRing, _profile);

            var testObject = new TestEnumerableClass
            {
                ID = 1,
                Items = new List<TestChildObject>
                {
                    new TestChildObject
                    {
                        ID = 2,
                        Exclude = "should not display"
                    }
                }
            };

            var result = expression.Compile()(testObject);

            Assert.IsNotNull(result);
            Assert.AreEqual(testObject.ID, result.ID);
            Assert.IsNotNull(result.Items);
            Assert.IsTrue(result.Items.Count() == 1);

            var childResult = result.Items.First();
            Assert.AreEqual(2, childResult.ID);
            Assert.IsNull(childResult.Exclude);
        }


        [TestMethod]
        public void TestList()
        {
            var expression = _builder.BuildExpression<TestListClass>(_fieldRing, _profile);

            var testObject = new TestListClass
            {
                ID = 1,
                Items = new List<TestChildObject>
                {
                    new TestChildObject
                    {
                        ID = 2,
                        Exclude = "should not display"
                    }
            }
            };

            var result = expression.Compile()(testObject);

            Assert.IsNotNull(result);
            Assert.AreEqual(testObject.ID, result.ID);
            Assert.IsNotNull(result.Items);
            Assert.IsTrue(result.Items.Count() == 1);

            var childResult = result.Items.First();
            Assert.AreEqual(2, childResult.ID);
            Assert.IsNull(childResult.Exclude);
        }


        [TestMethod]
        public void TestArray()
        {
            var expression = _builder.BuildExpression<TestArrayClass>(_fieldRing, _profile);

            var testObject = new TestArrayClass
            {
                ID = 1,
                Items = new TestChildObject[]
                {
                    new TestChildObject
                    {
                        ID = 2,
                        Exclude = "should not display"
                    }
                }
            };

            var result = expression.Compile()(testObject);

            Assert.IsNotNull(result);
            Assert.AreEqual(testObject.ID, result.ID);
            Assert.IsNotNull(result.Items);
            Assert.IsTrue(result.Items.Count() == 1);

            var childResult = result.Items.First();
            Assert.AreEqual(2, childResult.ID);
            Assert.IsNull(childResult.Exclude);
        }


        [TestMethod]
        public void TestCollection()
        {
            var expression = _builder.BuildExpression<TestCollectionClass>(_fieldRing, _profile);

            var testObject = new TestCollectionClass
            {
                ID = 1,
                Items = new List<TestChildObject>
                {
                    new TestChildObject
                    {
                        ID = 2,
                        Exclude = "should not display"
                    }
                }
            };

            var result = expression.Compile()(testObject);

            Assert.IsNotNull(result);
            Assert.AreEqual(testObject.ID, result.ID);
            Assert.IsNotNull(result.Items);
            Assert.IsTrue(result.Items.Count() == 1);

            var childResult = result.Items.First();
            Assert.AreEqual(2, childResult.ID);
            Assert.IsNull(childResult.Exclude);
        }


        public class TestChildObject
        {
            public int ID { get; set; }

            public string Exclude { get; set; }
        }

        public class TestEnumerableClass
        {
            public int ID { get; set; }

            public IEnumerable<TestChildObject> Items { get; set; }
        }

        public class TestListClass
        {
            public int ID { get; set; }

            public List<TestChildObject> Items { get; set; }
        }

        public class TestArrayClass
        {
            public int ID { get; set; }

            public TestChildObject[] Items { get; set; }
        }

        public class TestCollectionClass
        {
            public int ID { get; set; }

            public ICollection<TestChildObject> Items { get; set; }
        }
    }
}
