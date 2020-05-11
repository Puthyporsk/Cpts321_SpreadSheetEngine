// <copyright file="VariableNode.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SpreadsheetEngine
{
    using System.Collections.Generic;

    public class VariableNode : Node
    {
        public Dictionary<string, double> Var;

        public string Name { get; set; }

        public VariableNode(Dictionary<string, double> dict)
        {
            this.Var = dict;
        }

        public override double Evaluate()
        {
            return this.Var[this.Name];
        }
    }
}
