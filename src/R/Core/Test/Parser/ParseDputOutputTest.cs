using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.DataTypes.Definitions;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.AST.Expressions.Definitions;
using Microsoft.R.Core.AST.Functions.Definitions;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Operators.Definitions;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Core.AST.Values;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Core.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser
{
    public class RDataFactory
    {
        public static RObject Parse(string dputOutput, string[] rClassNames, string rTypeName)
        {
            AstRoot astRoot = RParser.Parse(dputOutput);
            Debug.Assert(astRoot.Children.Count == 1);

            IExpression expression = astRoot.FindFirstElement(ExpressionPredicate) as IExpression;

            return Create(expression, rClassNames, rTypeName);
        }

        private static RObject Create(IAstNode node, string[] rClassNames, string rTypeName)
        {
            // if-else if order is tricky. It must be in order from specific to generic, well... obviously
            if (node is FunctionCall)
            {
                var func = node as FunctionCall;
                var functionName = func.Children[0] as Variable;
                if (functionName != null)
                {
                    if (functionName.Name == "c")
                    {
                        if (func.Arguments.Count <= 0)
                        {
                            return RNull.Null;
                        }

                        var robj = Create(func.Arguments[0], rClassNames, rTypeName);
                        if (robj is RNumber)
                        {
                            return CreateVector<RNumber>(func, RMode.Numeric);
                        }
                        else if (robj is RLogical)
                        {
                            return CreateVector<RLogical>(func, RMode.Logical);
                        }
                        else if (robj is RString)
                        {
                            return CreateVector<RString>(func, RMode.Character);
                        }
                        else if (robj is RFunction)
                        {
                            return CreateVector<RFunction>(func, RMode.Function);
                        }

                        throw new InvalidOperationException("Can't understand combine function type");
                    }
                    else if (functionName.Name == "list")
                    {
                        var list = new RList();

                        for (int i = 0; i < func.Arguments.Count; i++)
                        {
                            list[i] = (Create(func.Arguments[i], rClassNames, rTypeName));
                        }

                        return list;
                    }
                    else if (functionName.Name == "structure")
                    {
                        RObject rObj = FindClass(func.Arguments);
                        if ((rObj != null)
                            && rObj.IsString
                            && ((RString)rObj).Value == "\"factor\"")
                        {
                            return CreateFactor(func);
                        }
                        
                    }
                }
            }
            else if (node is ExpressionArgument)
            {
                return Create(((ExpressionArgument)node).ArgumentValue, rClassNames, rTypeName);
            }
            else if (node is IExpression)
            {
                var expression = (IExpression)node;
                return Create(expression.Content, rClassNames, rTypeName);
            }
            else if (node is IOperator)
            {
                var op = (IOperator)node;
                if (op.OperatorType == OperatorType.Sequence)
                {
                    var from = (RNumber)Create(op.LeftOperand, rClassNames, rTypeName);
                    var to = (RNumber)Create(op.RightOperand, rClassNames, rTypeName);

                    int count = (int)Math.Abs(to.Value - from.Value) + 1;
                    int increment = to.Value > from.Value ? 1 : -1;
                    var vector = new RVector<RNumber>(RMode.Numeric, count);

                    double value = from.Value;
                    for (int i = 0; i < count; i++)
                    {
                        vector[i] = new RNumber(value);
                        value += increment;
                    }

                    return vector;
                }
            }
            else if (node is IRValueNode)
            {
                return ((IRValueNode)node).GetValue();
            }
            throw new NotImplementedException();
        }

        public static RObject CreateVector<T>(IFunction func, RMode mode) where T : RObject
        {
            RVector<T> vector = new RVector<T>(mode, func.Arguments.Count);
            List<RObject> items = new List<RObject>();
            for (int i = 0; i < func.Arguments.Count; i++)
            {
                vector[i] = (T) Create(func.Arguments[i], null, null);
            }

            return vector;
        }

        private static RObject CreateFactor(IFunction functionNode)
        {
            var arguments = functionNode.Arguments;

            var labelObj = FindLabel(arguments);
            if (labelObj == null)
            {
                Debug.Fail("CreateFactor needs a node with .Label argument");   // TODO: throw?
                return null;
            }

            var label = (IRVector<RString>)labelObj;

            RObject valuesObj = null;
            for (int i = 0; i < arguments.Count; i++)
            {
                var expression = arguments[i] as ExpressionArgument;
                if (expression != null)
                {
                    valuesObj = Create(expression, null, null);
                    break;
                }
            }
            var values = valuesObj as IRVector<RNumber>;
            if (values == null)
            {
                Debug.Fail("CreateFactor needs numerical vector");   // TODO: throw?
                return null;
            }

            var factor = new RFactor(values.Length, label);
            for (int i = 0; i < values.Length; i++)
            {
                factor[i] = values[i];
            }
            return factor;
        }

        private static bool LeftAssignPredicate(IAstNode node)
        {
            var op = node as IOperator;
            if (op != null)
            {
                if (op.OperatorType == OperatorType.LeftAssign)
                {
                    return true;
                }
            }
            return false;
        }

        private static RObject FindClass(ArgumentList arguments)
        {
            return FindNamedArgument(arguments, "class");
        }

        private static RObject FindLabel(ArgumentList arguments)
        {
            return FindNamedArgument(arguments, ".Label");
        }

        private static RObject FindNamedArgument(ArgumentList arguments, string name)
        {
            for (int i = 0; i < arguments.Count; i++)
            {
                var named = arguments[i] as NamedArgument;
                if (named != null)
                {
                    if (named.Name == name)
                    {
                        return Create(named.DefaultValue.Content, null, null);
                    }
                }
            }
            return null;
        }

        private static bool ExpressionPredicate(IAstNode node)
        {
            return node is IExpression;
        }
    }



    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ParseDputOutputTest
    {
        [TestMethod]
        public void ParseDataFrameTest()
        {
            string dputOutput =
@".dput.output.value<-structure(
  list(
    a = structure(
      c(1L, 3L, 2L),
      .Label = c(\""one\"", \""three\"", \""two\""),
      class = \""factor\""),
    x = c(1, 1, 1),
	y = c(1, 2, 3)
  ),
  .Names = c(\""a\"", \""x\"", \""y\""),
  row.names = c(NA, -3L),
  class = \""data.frame\"")";

            RObject rData = RDataFactory.Parse(dputOutput, null, null);

            var rIntegeer = rData as RInteger;
            Assert.IsNotNull(rIntegeer);
        }

        [TestMethod]
        public void ParseIntegerTest()
        {
            RObject rData = RDataFactory.Parse("100", new string[] { "integer" }, "integer");

            var rNumber = rData as RNumber;
            Assert.IsNotNull(rNumber);
            Assert.AreEqual(100, rNumber.Value);

            rData = RDataFactory.Parse("1L", new string[] { "integer" }, "integer");

            rNumber = rData as RNumber;
            Assert.IsNotNull(rNumber);
            Assert.AreEqual(100, rNumber.Value);
        }

        [TestMethod]
        public void ParseSequenceTest()
        {
            RObject rData = RDataFactory.Parse("1:3", new string[] { "integer" }, "integer");

            var rVector = rData as RVector<RNumber>;
            Assert.IsNotNull(rVector);
            Assert.AreEqual(3, rVector.Length);
            Assert.AreEqual(1, rVector[0]);
            Assert.AreEqual(2, rVector[1]);
            Assert.AreEqual(3, rVector[2]);

            rData = RDataFactory.Parse("-101:-103", new string[] { "integer" }, "integer");

            rVector = rData as RVector<RNumber>;
            Assert.IsNotNull(rVector);
            Assert.AreEqual(3, rVector.Length);
            Assert.AreEqual(-101, rVector[0]);
            Assert.AreEqual(-102, rVector[1]);
            Assert.AreEqual(-103, rVector[2]);
        }

        [TestMethod]
        public void ParseArrayTest()
        {
            RObject rData = RDataFactory.Parse("c(1,2,3)", new string[] { "integer" }, "integer");

            var rNumbers = rData as RVector<RNumber>;
            Assert.IsNotNull(rNumbers);
            Assert.AreEqual(3, rNumbers.Length);
        }

        [TestMethod]
        public void ParseCharacterTest()
        {
            RObject rData = RDataFactory.Parse(@"""ab\""c""", new string[] { "integer" }, "integer");

            var rString = rData as RString;
            Assert.IsNotNull(rString);
            Assert.AreEqual("\"ab\\\"c\"", rString.Value);

            rData = RDataFactory.Parse(@"'ab\""c'", new string[] { "integer" }, "integer");

            rString = rData as RString;
            Assert.IsNotNull(rString);
            Assert.AreEqual("'ab\\\"c'", rString.Value);
        }

        [TestMethod]
        public void ParseListTest()
        {
            RObject rData = RDataFactory.Parse(@"list(1)", new string[] { "integer" }, "integer");

            var rList = rData as RList;
            Assert.IsNotNull(rList);
            Assert.AreEqual(1, rList.Length);
            Assert.AreEqual(1, ((RNumber)rList[0]).Value);

            rData = RDataFactory.Parse(@"list(1,2L,'abc',c(1,2,3))", new string[] { "integer" }, "integer");

            rList = rData as RList;
            Assert.IsNotNull(rList);
            Assert.AreEqual(4, rList.Length);
            Assert.AreEqual(1, ((RNumber)rList[0]).Value);
            Assert.AreEqual(2, ((RNumber)rList[1]).Value);
            Assert.AreEqual("'abc'", ((RString)rList[2]).Value);
            var rListItem3 = (RVector<RNumber>)rList[3];
            Assert.AreEqual(3, rListItem3.Length);
            Assert.AreEqual(1, rListItem3[0]);
            Assert.AreEqual(2, rListItem3[1]);
            Assert.AreEqual(3, rListItem3[2]);
        }

        [TestMethod]
        public void TempTest()  // Not a real test. For experiment code
        {
            RObject rData = RDataFactory.Parse(@"structure(c(2L, 1L, 1L, 1L, 2L), .Label = c(""female"", ""male""), class = ""factor"")", new string[] { "integer" }, "integer");

            RFactor rFactor = rData as RFactor;
            Assert.IsNotNull(rFactor);
            Assert.AreEqual(5, rFactor.Length);
            Assert.AreEqual(2, rFactor[0].Value);
            Assert.AreEqual(1, rFactor[1].Value);
            Assert.AreEqual(1, rFactor[2].Value);
            Assert.AreEqual(1, rFactor[3].Value);
            Assert.AreEqual(2, rFactor[4].Value);

            Assert.AreEqual("\"male\"", rFactor.LabelOf(0).Value);
            Assert.AreEqual("\"female\"", rFactor.LabelOf(1).Value);
            Assert.AreEqual("\"female\"", rFactor.LabelOf(2).Value);
            Assert.AreEqual("\"female\"", rFactor.LabelOf(3).Value);
            Assert.AreEqual("\"male\"", rFactor.LabelOf(4).Value);
        }
    }
}
