// <copyright file="ConstantNode.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SpreadsheetEngine
{
    public class ConstantNode : Node
    {
        public double Value { get; set; }

        public override double Evaluate()
        {
            return this.Value;
        }
    }
}
