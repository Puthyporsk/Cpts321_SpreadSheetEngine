// <copyright file="ExpressionTree.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SpreadsheetEngine
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text.RegularExpressions;

    public class ExpressionTree
    {
        public Node Root = null;
        public Dictionary<string, double> Variables = new Dictionary<string, double>();

        public List<string> GetVariableNames()
        {
            List<string> dict = new List<string>();
            foreach (string key in this.Variables.Keys)
            {
                dict.Add(key);
            }

            return dict;
        }

        // ExpressionTree will add the incoming string into a tree accordingly
        public ExpressionTree(string expression)
        {
            this.Root = this.ConstructTree(expression);
        }

        public void SetVariable(string variableName, double variableValue)
        {
            this.Variables[variableName] = variableValue;
        }

        public double Evaluate()
        {
            // If the node is a constantNode(a number), then simply return its value
            ConstantNode constantNode = this.Root as ConstantNode;
            if (constantNode != null)
            {
                return constantNode.Value;
            }

            // If the node is a variableNode then return the value in the dictionary correspondingly
            VariableNode variableNode = this.Root as VariableNode;
            if (variableNode != null)
            {
                return this.Variables[variableNode.Name];
            }

            // it is an operator node if we came here
            OperatorNode operatorNode = this.Root as OperatorNode;
            if (operatorNode != null)
            {
                OperatorNode op = this.Root as OperatorNode;
                if (op != null)
                {
                    return op.Evaluate();
                }
            }

            throw new NotSupportedException();
        }

        public string ShuntingYard(string expression)
        {
            Stack<string> stack = new Stack<string>();
            string result = string.Empty;
            char c;
            for (int i = 0; i < expression.Length; i++)
            {
                c = expression[i];
                if (c == '+' || c == '-' || c == '*' || c == '/' || c == '^') // Incoming symbols is an operator 
                {
                    OperatorNode op = new OperatorNode(c);
                    if (stack.Count == 0 || stack.Peek() == "(") // 4. Stack is empty or stack top = Left parenthesis
                    {
                        stack.Push(c.ToString());
                    }
                    else if (stack.Count != 0)
                    {
                        OperatorNode stackOp = new OperatorNode(stack.Peek().ToCharArray()[0]);
                        if ((op.Precedence > stackOp.Precedence) || (op.Precedence == stackOp.Precedence && stackOp.Associativity == "Right")) // 5.
                        {
                            stack.Push(c.ToString());
                        }
                        else if ((op.Precedence < stackOp.Precedence) || (op.Precedence == stackOp.Precedence && stackOp.Associativity == "Left")) // 6.
                        {
                            while (stack.Count != 0 && ((op.Precedence < stackOp.Precedence) || (op.Precedence == stackOp.Precedence && stackOp.Associativity == "Left")))
                            {
                                result += stack.Pop();
                            }

                            stack.Push(c.ToString());
                        }
                    }
                }
                else if (c == '(') // 2. Incoming symbols is a left parenthesis 
                {
                    stack.Push(c.ToString());
                }
                else if (c == ')') // 3. Incoming symbols is a right parenthesis 
                {
                    while (stack.Peek() != "(")
                    {
                        result += stack.Pop();
                    }

                    stack.Pop();
                }
                else // 1. Incoming symbols is an operand
                {
                    result += c;
                }
            }

            while (stack.Count != 0)
            {
                result += stack.Pop();
            }

            return result;
        }

        public Node ConstructTree(string pre_expression)
        {
            // This block is responsible for finding all the variables in the expression
            List<string> temp = new List<string>();
            string m = string.Empty;
            char p;
            for (int k = 0; k < pre_expression.Length; k++)
            {
                p = pre_expression[k];
                if (p == '(' || p == ')')
                { }
                else if (p != '+' && p != '-' && p != '*' && p != '/')
                {
                    m += p;
                }
                else
                {
                    temp.Add(m);
                    m = "";
                }
            }
            temp.Add(m);

            // Take the original expression
            string expression = ShuntingYard(pre_expression); // Get its postfix notation

            // This block is responsible for building the tree
            Stack<Node> stack = new Stack<Node>();   
            Node n = this.Root;
            char c;
            string x = "";
            int y = 0;

            // Read until there is nothing left to read
            for (int i = 0; i < expression.Length; i++)
            {
                c = expression[i]; // Read char by char

                // If char read is an operator
                if (c == '+' || c == '-' || c == '*' || c == '/')
                {
                    Node tree1 = null;
                    if (stack.Count != 0)
                        tree1 = stack.Pop();
                    Node tree2 = null;
                    if (stack.Count != 0)
                        tree2 = stack.Pop();

                    OperatorNode op = new OperatorNode(c);
                    op.Right = tree1;
                    op.Left = tree2;
                    n = op;
                    stack.Push(op);
                }
                else
                {
                    // If its any other things (variables)
                    x += c; // Keep making the string until it is one of the variables we need
                    if (x == temp[y])
                    {
                        y++;
                        // This part is where we deal with single expressions without Operators
                        // So automatically, we know that it will either be a ConstantNode(a number) or a VariableNode(a name)
                        double number;

                        // If the expression is a ConstantNode
                        if (double.TryParse(x, out number))
                        {
                            // Insert the expression into the dictionary
                            //if (!this.Variables.ContainsKey(x))
                            //{
                            //    this.Variables.Add(x, number);
                            //}
                            ConstantNode cn = new ConstantNode();
                            cn.Value = number;
                            n = cn;
                            stack.Push(n);
                        }

                        // If the expression is VariableNode
                        else
                        {
                            // Insert the expression into the dictionary
                            if (!this.Variables.ContainsKey(x))
                            {
                                this.Variables.Add(x, 0.0);
                            }
                            VariableNode vn = new VariableNode(Variables);
                            vn.Name = x;
                            n = vn;
                            stack.Push(n);
                        }
                        x = "";
                    }
                }
            }
            return n;
        }

    }
}
