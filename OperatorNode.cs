// <copyright file="OperatorNode.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SpreadsheetEngine
{
    public class OperatorNode : Node
    {
        public Node Left { get; set; }

        public Node Right { get; set; }

        public char Operator { get; set; }

        public int Precedence { get; set; }

        public string Associativity { get; set; }

        public OperatorNode(char op)
        {
            this.Operator = op;
            this.Left = this.Right = null;
            if (op == '+' || op == '-')
            {
                Precedence = 1;
                Associativity = "Left";
            }
            else if (op == '*' || op == '/')
            {
                Precedence = 2;
                Associativity = "Left";
            }
            else if (op == '^')
            {
                Precedence = 3;
                Associativity = "Right";
            }
            else if (op == '(' || op == ')')
            {
                Precedence = 3;
                Associativity = "Non";
            }
        }

        // Make calls to the classes that does the final calculation
        public override double Evaluate()
        {
            switch (this.Operator)
            {
                case '+':
                    AdditionNode add = new AdditionNode('+');
                    add.Left = this.Left;
                    add.Right = this.Right;
                    return add.Evaluate();
                case '-':
                    SubtractionNode sub = new SubtractionNode('-');
                    sub.Left = this.Left;
                    sub.Right = this.Right;
                    return sub.Evaluate();
                case '*':
                    MultiplicationNode mul = new MultiplicationNode('*');
                    mul.Left = this.Left;
                    mul.Right = this.Right;
                    return mul.Evaluate();
                case '/':
                    DivisionNode div = new DivisionNode('/');
                    div.Left = this.Left;
                    div.Right = this.Right;
                    return div.Evaluate();
                default:
                    break;
            }

            return 0.0;
        }
    }
}
