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
                FieldParser = new GoogleParser(),
                IgnoreCase = true
            };
        }


        [TestMethod]
        public void TestEnumerable()
        {
            var expression = _builder.BuildExpression<CollectionTests_TestEnumerableClass>(_fieldRing, _profile);

            var testObject = new CollectionTests_TestEnumerableClass
            {
                ID = 1,
                Items = new List<CollectionTests_TestChildObject>
                {
                    new CollectionTests_TestChildObject
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
            var expression = _builder.BuildExpression<CollectionTests_TestListClass>(_fieldRing, _profile);

            var testObject = new CollectionTests_TestListClass
            {
                ID = 1,
                Items = new List<CollectionTests_TestChildObject>
                {
                    new CollectionTests_TestChildObject
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
            var expression = _builder.BuildExpression<CollectionTests_TestArrayClass>(_fieldRing, _profile);

            var testObject = new CollectionTests_TestArrayClass
            {
                ID = 1,
                Items = new CollectionTests_TestChildObject[]
                {
                    new CollectionTests_TestChildObject
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
            var expression = _builder.BuildExpression<CollectionTests_TestCollectionClass>(_fieldRing, _profile);

            var testObject = new CollectionTests_TestCollectionClass
            {
                ID = 1,
                Items = new List<CollectionTests_TestChildObject>
                {
                    new CollectionTests_TestChildObject
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


        public class CollectionTests_TestChildObject
        {
            public int ID { get; set; }

            public string Exclude { get; set; }
        }

        public class CollectionTests_TestEnumerableClass
        {
            public int ID { get; set; }

            public IEnumerable<CollectionTests_TestChildObject> Items { get; set; }
        }

        public class CollectionTests_TestListClass
        {
            public int ID { get; set; }

            public List<CollectionTests_TestChildObject> Items { get; set; }
        }

        public class CollectionTests_TestArrayClass
        {
            public int ID { get; set; }

            public CollectionTests_TestChildObject[] Items { get; set; }
        }

        public class CollectionTests_TestCollectionClass
        {
            public int ID { get; set; }

            public ICollection<CollectionTests_TestChildObject> Items { get; set; }
        }
    }
}
