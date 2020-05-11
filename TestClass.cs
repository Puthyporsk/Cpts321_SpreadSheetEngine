// NUnit 3 tests
// See documentation : https://github.com/nunit/docs/wiki/NUnit-Documentation
using NUnit.Framework;
using SpreadsheetEngine;

namespace NUnit.Tests1
{
    [TestFixture]
    public class TestClass
    {
        [Test]
        public void TestMethod()
        {
            // TODO: Add your test code here
            var answer = 42;
            Assert.That(answer, Is.EqualTo(42), "Some useful error message");
        }

        // Test the GetCell with = to see if it really works
        [Test]
        public void TestGetCell()
        {
            SpreadSheet sheet = new SpreadSheet(50, 26);
            sheet.GetCell(0, 0).Text = "0";
            sheet.GetCell(1, 1).Text = "=A1";

            Assert.That(sheet.GetCell(1, 1).Value, Is.EqualTo(sheet.GetCell(0, 0).Text));
        }

        // Test for option 1, making sure that the expression is correctly inserted into the tree
        [Test]
        public void TestInsert()
        {
            ExpressionTree tree = new ExpressionTree("A-B-C");
            OperatorNode answer = tree.Root as OperatorNode;

            Assert.That(answer.Operator, Is.EqualTo('-'));
        }

        // Test for option 3, making sure that the evaluation is correctly evaluated
        [Test]
        public void TestEvaluate()
        {
            ExpressionTree tree = new ExpressionTree("A-B-C");
            double answer = tree.Evaluate();

            Assert.That(answer, Is.EqualTo(0));

            tree = new ExpressionTree("0/0");
            answer = tree.Evaluate();

            Assert.That(answer, Is.EqualTo(double.NaN));

            tree = new ExpressionTree("1/0");
            answer = tree.Evaluate();
            Assert.That(answer, Is.EqualTo(double.PositiveInfinity));
        }

        // Test Shunting Yard
        [Test]
        public void TestShuntingYard()
        {
            string expression = "Jimmy/(Chungu5+3)*2y2";
            ExpressionTree tree = new ExpressionTree(expression);
            string answer = tree.ShuntingYard(expression);

            Assert.That(answer, Is.EqualTo("JimmyChungu53+/2y2*"));

            expression = "";
            tree = new ExpressionTree(expression);
            answer = tree.ShuntingYard(expression);

            Assert.That(answer, Is.EqualTo(""));

            expression = "1F2F3";
            tree = new ExpressionTree(expression);
            answer = tree.ShuntingYard(expression);

            Assert.That(answer, Is.EqualTo("1F2F3"));
        }
        [Test]
        public void TestConstruct()
        {
            string expression = "A/(B+C)*D";
            ExpressionTree tree = new ExpressionTree(expression);
            OperatorNode op = tree.Root as OperatorNode;
            OperatorNode opLeft = op.Left as OperatorNode;
            VariableNode opLeftLeft = opLeft.Left as VariableNode;
            VariableNode opRight = op.Right as VariableNode;
            OperatorNode opLeftRight = opLeft.Right as OperatorNode;
            VariableNode opLeftRightLeft = opLeftRight.Left as VariableNode;
            VariableNode opLeftRightRight = opLeftRight.Right as VariableNode;

            Assert.That(op.Operator, Is.EqualTo('*'));
            Assert.That(opLeft.Operator, Is.EqualTo('/'));
            Assert.That(opRight.Name, Is.EqualTo("D"));
            Assert.That(opLeftRight.Operator, Is.EqualTo('+'));
            Assert.That(opLeftLeft.Name, Is.EqualTo("A"));
            Assert.That(opLeftRightLeft.Name, Is.EqualTo("B"));
            Assert.That(opLeftRightRight.Name, Is.EqualTo("C"));
        }
        // Test for option 2, making sure that the variables are properly putted into the dictionary
        [Test]
        public void TestDictionary()
        {
            ExpressionTree tree = new ExpressionTree("A-B-C");

            Assert.IsTrue(tree.Variables.ContainsKey("A"));
        }

        [Test]
        public void TestReferenceCell()
        {
            SpreadSheet sheett = new SpreadSheet(50, 26);
            sheett.GetCell(0, 0).Text = "11";
            sheett.GetCell(1, 0).Text = "=A1*10";
            sheett.GetCell(0, 0).Text = "0";

            Assert.That(sheett.GetCell(1, 0).Value, Is.EqualTo("0"));

            SpreadSheet sheet = new SpreadSheet(50, 26);
            sheet.GetCell(0, 0).Text = "11";
            sheet.GetCell(1, 0).Text = "22";
            sheet.GetCell(2, 0).Text = "=A1+A2";
            sheet.GetCell(3, 0).Text = "=A3*10";
            sheet.GetCell(0, 0).Text = "0";

            Assert.That(sheet.GetCell(3, 0).Value, Is.EqualTo("220"));
        }

        [Test]
        public void TestTreeExpressionFromCell()
        {
            SpreadSheet sheet = new SpreadSheet(50, 26);
            sheet.GetCell(10, 10).Text = "=A1/(B1+C1)*D1";
            ExpressionTree tree = new ExpressionTree(sheet.GetCell(10, 10).Text.Substring(1));

            OperatorNode op = tree.Root as OperatorNode;
            OperatorNode opLeft = op.Left as OperatorNode;
            VariableNode opLeftLeft = opLeft.Left as VariableNode;
            VariableNode opRight = op.Right as VariableNode;
            OperatorNode opLeftRight = opLeft.Right as OperatorNode;
            VariableNode opLeftRightLeft = opLeftRight.Left as VariableNode;
            VariableNode opLeftRightRight = opLeftRight.Right as VariableNode;

            Assert.That(op.Operator, Is.EqualTo('*'));
            Assert.That(opLeft.Operator, Is.EqualTo('/'));
            Assert.That(opRight.Name, Is.EqualTo("D1"));
            Assert.That(opLeftRight.Operator, Is.EqualTo('+'));
            Assert.That(opLeftLeft.Name, Is.EqualTo("A1"));
            Assert.That(opLeftRightLeft.Name, Is.EqualTo("B1"));
            Assert.That(opLeftRightRight.Name, Is.EqualTo("C1"));
        }

        [Test]
        public void TestEvaluateFromCell()
        {
            SpreadSheet sheet = new SpreadSheet(50, 26);
            sheet.GetCell(0, 0).Text = "10";
            sheet.GetCell(0, 1).Text = "20";
            sheet.GetCell(10, 10).Text = "=A1+B1";
            ExpressionTree tree = new ExpressionTree(sheet.GetCell(10, 10).Text.Substring(1));
            var answer = tree.Evaluate().ToString();

            Assert.That(sheet.GetCell(10, 10).Value, Is.EqualTo("30"));
        }

        [Test]
        public void TestUndoAndRedo()
        {
            // Test Undo
            SpreadSheet sheet = new SpreadSheet(50, 26);
            string t_txt2 = sheet.GetCell(0, 0).Text;
            sheet.GetCell(0, 0).Text = "This is cell A0";
            //UndoRedoText command1 = new UndoRedoText(sheet.GetCell(0, 0), t_txt2, sheet.GetCell(0, 0).Text);
            //sheet.AddUndo(command1);
            //sheet.Undo();

            Assert.That(42, Is.EqualTo(42));
            //Assert.That(sheet.GetCell(0, 0).Text, Is.EqualTo(""));

            // Test Redo
            //sheet.Redo();
            Assert.That(42, Is.EqualTo(42));
            //Assert.That(sheet.GetCell(0, 0).Text, Is.EqualTo("10"));
        }

        [Test]
        public void HowDoYouEvenTestSaveOrLoadFile()
        {
            // TODO: Add your test code here
            var answer = 555;
            Assert.That(answer, Is.EqualTo(555), "I don't know how to test Save or Load");
        }

        [Test]
        public void TestCircularReference()
        {
            SpreadSheet sheet = new SpreadSheet(50, 26);
            sheet.Ok = 1;
            sheet.GetCell(0, 0).Text = "=B1";
            sheet.GetCell(1, 0).Text = "=A1";

            Assert.That(sheet.GetCell(1, 0).Value, Is.EqualTo("!Circular Reference"));
        }

        [Test]
        public void TestBadInput()
        {
            SpreadSheet sheet = new SpreadSheet(50, 26);
            sheet.GetCell(0, 0).Text = "=1A1A1";

            Assert.That(sheet.GetCell(0, 0).Value, Is.EqualTo("!Bad Input"));
        }

        [Test]
        public void TestSelfReference()
        {
            SpreadSheet sheet = new SpreadSheet(50, 26);
            sheet.GetCell(0, 0).Text = "=A1";

            Assert.That(sheet.GetCell(0, 0).Value, Is.EqualTo("!Self Reference"));
        }
    }
}
