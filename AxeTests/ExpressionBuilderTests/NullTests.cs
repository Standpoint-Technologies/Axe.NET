using System.Collections.Generic;
using Axe.ExpressionBuilders;
using Axe.FieldParsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AxeTests.ExpressionBuilderTests
{
    [TestClass]
    public class NullTests
    {
        private DefaultExpressionBuilder _builder;
        private Axe.AxeProfile _profile;


        [TestInitialize]
        public void InitializeExpressionBuilder()
        {
            _builder = new DefaultExpressionBuilder();
            _profile = new Axe.AxeProfile()
            {
                ExpressionBuilder = _builder,
                FieldParser = new GoogleParser(),
                IgnoreCase = true,
            };
        }

        [TestMethod]
        public void TestSimpleNullChildObject()
        {
            var fieldRing = new Axe.FieldRing()
            {
                Fields = new List<string>() { "id" },
                NestedRings = new Dictionary<string, Axe.FieldRing>()
                {
                    {"child", new Axe.FieldRing()
                        {
                            Fields = new List<string>() { "id" }
                        }
                    }
                }
            };

            var expression = _builder.BuildExpression<NullTest_TestParentObject>(fieldRing, _profile);

            var testObject = new NullTest_TestParentObject
            {
                ID = 1
            };

            var result = expression.Compile()(testObject);

            Assert.IsNotNull(result);
            Assert.AreEqual(testObject.ID, result.ID);
            Assert.IsNull(result.Child);
        }

        [TestMethod]
        public void TestSimpleNullValue()
        {
            var fieldRing = new Axe.FieldRing()
            {
                Fields = new List<string>() { "id" }
            };

            var expression = _builder.BuildExpression<NullTest_TestChildObject>(fieldRing, _profile);

            var testObject = new NullTest_TestChildObject
            {
                ID = 1,
                Exclude = "exclude"
            };

            var result = expression.Compile()(testObject);

            Assert.IsNotNull(result);
            Assert.AreEqual(testObject.ID, result.ID);
            Assert.IsNull(result.Exclude);
        }

        [TestMethod]
        public void TestDeepNestedObjectWithNull()
        {
            var fieldRing = new Axe.FieldRing()
            {
                Fields = new List<string>() { "id" },
                NestedRings = new Dictionary<string, Axe.FieldRing>()
                {
                    {"child", new Axe.FieldRing()
                        {
                            Fields = new List<string>() { "id" },
                            NestedRings = new Dictionary<string, Axe.FieldRing>()
                        }
                    }
                }
            };

            var expression = _builder.BuildExpression<NullTest_TestParentObject>(fieldRing, _profile);

            var testObject = new NullTest_TestParentObject
            {
                ID = 1,
                Child = new NullTest_TestChildObject()
                {
                    ID = 2,
                    Child = new NullTest_TestChildChildObject()
                    {
                        ID = 3
                    }
                }
            };

            var result = expression.Compile()(testObject);

            Assert.IsNotNull(result);
            Assert.AreEqual(testObject.ID, result.ID);
            Assert.IsNotNull(result.Child);
            Assert.AreEqual(testObject.Child.ID, result.Child.ID);
            Assert.IsNull(result.Child.Child);
        }

        [TestMethod]
        public void TestNullCollection()
        {
            var fieldRing = new Axe.FieldRing()
            {
                Fields = new List<string>() { "id" },
                NestedRings = new Dictionary<string, Axe.FieldRing>()
            };

            var expression = _builder.BuildExpression<NullTest_TestObjectWithCollection>(fieldRing, _profile);

            var testObject = new NullTest_TestObjectWithCollection
            {
                ID = 1,
                Children = new List<NullTest_TestChildChildObject>()
                {
                    new NullTest_TestChildChildObject()
                    {
                        ID = 2
                    },
                    new NullTest_TestChildChildObject()
                    {
                        ID = 3
                    }
                }
            };

            var result = expression.Compile()(testObject);

            Assert.IsNotNull(result);
            Assert.AreEqual(testObject.ID, result.ID);
            Assert.IsNull(result.Children);
        }


        public class NullTest_TestParentObject
        {
            public int ID { get; set; }

            public NullTest_TestChildObject Child { get; set; }
        }

        public class NullTest_TestChildObject
        {
            public int ID { get; set; }

            public string Exclude { get; set; }

            public NullTest_TestChildChildObject Child { get; set; }
        }

        public class NullTest_TestChildChildObject
        {
            public int ID { get; set; }
        }

        public class NullTest_TestObjectWithCollection
        {
            public int ID { get; set; }

            public List<NullTest_TestChildChildObject> Children { get; set; }
        }
    }
}
