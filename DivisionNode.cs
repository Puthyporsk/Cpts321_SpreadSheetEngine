// <copyright file="DivisionNode.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SpreadsheetEngine
{
    public class DivisionNode : OperatorNode
    {
        public DivisionNode(char op)
            : base(op)
        {
            this.Operator = op;
            this.Left = this.Right = null;
        }

        public override double Evaluate()
        {
            return this.Left.Evaluate() / this.Right.Evaluate();
        }
    }
}
