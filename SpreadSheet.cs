// <copyright file="SpreadSheet.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SpreadsheetEngine
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;

    public partial class SpreadSheet
    {
        // 2D array for the spreadsheet
        private SpreadsheetCell[,] arrayCells;
        // cell will contain the alphabet as the key, and the column number of that key as the value
        public Dictionary<string, int> cell = new Dictionary<string, int>();
        public int Ok = 0;
        public Dictionary<Cell, int> circularReferenceKeys = new Dictionary<Cell, int>();
        public event PropertyChangedEventHandler CellPropertyChanged;
        public event PropertyChangedEventHandler ColorPropertyChanged;
        public int ColumnCount { get; }
        public int RowCount { get; }

        char[] alphabet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        private Stack<UndoRedoCollection> Undos = new Stack<UndoRedoCollection>();
        private Stack<UndoRedoCollection> Redos = new Stack<UndoRedoCollection>();

        // Function will reset the spreadsheet to its base settings
        public void Clear_Sheet()
        {
            foreach (SpreadsheetCell cell in arrayCells)
            {
                cell.Text = string.Empty;
                cell.Color = 0xFFFFFFFF;
                Undos.Clear();
                Redos.Clear();
            }
        }

        // Initialize the 2D arrays with the given dimentsions
        public SpreadSheet(int rowIndexN, int columnIndexN)
        {
            this.arrayCells = new SpreadsheetCell[rowIndexN, columnIndexN];
            this.ColumnCount = columnIndexN;
            this.RowCount = rowIndexN;
            for (int i = 0; i < 26; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    this.arrayCells[j, i] = new SpreadsheetCell(j, i);
                    this.arrayCells[j, i].PropertyChanged += this.OnPropertyChanged;
                    this.arrayCells[j, i].ColorPropertyChanged += this.OnColorPropertyChanged;
                }
            }

            // Easy to get a Cell
            // Since Column 'A' will always be at index 0 or Column 0 
            // And Column 'B' will always be at index 1 or Column 1
            // and so on, we set those index as the value in the dictionary with the alphabet char as the key
            
            for (int i = 0; i < alphabet.Length; i++)
            {
                this.cell.Add(alphabet[i].ToString(), i);
            }
        }

        /// <summary>
        /// Title of function tells a thousand stories
        /// </summary>
        /// <returns></returns>
        public bool PleaseDontGoIntoInfiniteLoop()
        {
            if (Ok == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Put all cells that has been changed into a list
        /// look thru that list and compare the positioning of the cell and the Text
        /// if the text is trying to reference any cell positioning already in the list then it is a circular reference
        /// say list have (0, 0) if any other item have Text =A1 then it is a circular reference
        /// </summary>
        /// <returns></returns>
        public bool CheckCircularReference()
        {
            List<Cell> lCell = new List<Cell>();
            foreach (Cell cell in circularReferenceKeys.Keys)
            {
                lCell.Add(cell);
            }
            for (int i = 0; i < lCell.Count; i++)
            {
                string name = this.GetNameFromCell(lCell[i]);
                for (int j = i+1; j < lCell.Count; j++)
                {
                    string temp_name = lCell[j].Text;
                    if ('='+name == temp_name)
                    {
                        Ok = 1;
                        return false;
                    }
                }
            }
            
            return true;
        }

        public string GetNameFromCell(Cell cell)
        {
            return alphabet[cell.ColumnIndex]+(cell.RowIndex+1).ToString();
        }
        // From the comment right above this
        // Allows easy access when trying to get a Cell
        public Cell GetCellFromName(string name)
        {
            // Since we know the stated statements above
            // We can just pull out the column index like this
            int col = this.cell[name[0].ToString()];
            int row = Convert.ToInt32(name.Substring(1)) - 1;
            return this.GetCell(row, col);
        }

        // Returns a Cell from the 2D array
        public Cell GetCell(int rowIndex, int columnIndex)
        {
            return this.arrayCells[rowIndex, columnIndex];
        }

        protected virtual void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.CellPropertyChanged?.Invoke(sender, e);
        }

        protected virtual void OnColorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.CellPropertyChanged?.Invoke(sender, e);
        }

        protected void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SpreadsheetCell sheetCell = (SpreadsheetCell)sender;
            string temp_string = sheetCell.Text;

            // If the text is a formula
            if (temp_string == string.Empty || temp_string[0] != '=') // If the text is NOT a formula
            {
                sheetCell.Text = temp_string;
                sheetCell.Value = sheetCell.Text;
            }
            else //(temp_string != string.Empty || temp_string[0] == '=')
            {
                if (temp_string.Length > 2)
                {
                    string newExpression = temp_string.Substring(1); // Take out the '=' in the text, the rest will be an expression
                                                                     // Create a tree from that expression
                    ExpressionTree tree = new ExpressionTree(newExpression);

                    // Go through every variables in the tree dictionary
                    foreach (string key in tree.GetVariableNames())
                    {
                        // We are using a dictionary because dictionary cannot add the same key twice
                        // So it is easier to catch acception and check whether the function went into a circular loop
                        try
                        {
                            circularReferenceKeys.Add(sheetCell, 1);
                        }
                        catch (ArgumentException)
                        {
                            if (!this.CheckCircularReference())
                            {
                                Ok = 1;
                                return;
                            }
                        };

                        // MAKE THE KEY TRUE
                        // These booleans are here to evaluate the formula
                        // Covers when formula is less than 4
                        // When its 3 or 2, its in the correct format (Char Int (Int))
                        // Covers when the formula is larger than 1
                        // Anything that evaluate false from this will not be considered a proper formula
                        bool four = key.Length < 4;
                        bool three = true;
                        if (key.Length == 3)
                        {
                            // three = Int32.TryParse(key[1].ToString(), out int temp1) && Int32.TryParse(key[2].ToString(), out int temp2);
                            if (Int32.TryParse(key[1].ToString(), out int temp1))
                            {
                                if (Int32.TryParse(key[2].ToString(), out int temp2))
                                {
                                    Int32.TryParse(key.Substring(1), out int temp3);
                                    if (temp3 > 50)
                                    {
                                        three = false;
                                    }
                                    else
                                    {
                                        three = true;
                                    }
                                }
                                else
                                {
                                    three = false;
                                }
                            }
                        }
                        bool two = true;
                        if (key.Length == 2)
                        {
                            two = Int32.TryParse(key[1].ToString(), out int tempp1);
                            if (tempp1 == 0)
                            {
                                two = false;
                            }
                        }
                        bool one = key.Length > 1;
                        if (four && three && two && one)
                        {
                            // Here we are doing the Try-Catch key 
                            // Because if the program fails from creating a cell from that key
                            // that means the formula is bad
                            try
                            {
                                // Make a Cell out of that tree dictionary key
                                Cell cell = this.GetCellFromName(key);

                                double val = 0.0;
                                if (double.TryParse(cell.Value, out val))
                                {
                                    tree.SetVariable(key, val);
                                    // Evaluate the tree expression
                                    temp_string = tree.Evaluate().ToString();
                                    // Set the value of the cell
                                    sheetCell.Value = temp_string;
                                    // Only allow the most recent cell that caused the issue to change its value
                                    if (Ok == 1)
                                    {
                                        sheetCell.Value = "!Circular Reference";
                                        Ok = 0;
                                        circularReferenceKeys.Clear();
                                    }
                                }
                                else
                                {
                                    if (cell.Value == "")
                                    {
                                        sheetCell.Value = 0.ToString();
                                    }
                                    else
                                    {
                                        // Only allow the most recent cell that caused the issue to change its value
                                        if (cell.Value != "!Self Reference")
                                        {
                                            sheetCell.Value = cell.Value;
                                        }
                                    }
                                }

                                if (GetNameFromCell(sheetCell) == GetNameFromCell(cell))
                                {
                                    sheetCell.Value = "!Self Reference";
                                }

                                // Subsribe the cell to the PropertyChanged event
                                cell.ReferencedCellPropertyChanged += sheetCell.OnPropertyChanged;
                            }
                            catch (KeyNotFoundException)
                            {
                                sheetCell.Value = "!Bad Input";
                            };
                        }
                        else
                        {
                            sheetCell.Value = "!Bad Input";
                        }
                    }

                    if (tree.GetVariableNames().Count == 0)
                    {
                        // Evaluate the tree expression
                        temp_string = tree.Evaluate().ToString();
                        // Set the value of the cell
                        sheetCell.Value = temp_string;
                    }
                }
                else
                {
                    if (temp_string.Length == 2 && Int32.TryParse(temp_string[1].ToString(), out int temp))
                    {
                        sheetCell.Value = temp_string[1].ToString();
                    }
                    else
                    {
                        sheetCell.Value = "!Bad Input";
                    }
                }
            }

            // Invoke the CellPropertyChanged event
            this.OnCellPropertyChanged(sheetCell, e);
        }

        PropertyChangedEventArgs handler;
        public void AddUndo(UndoRedoCollection item)
        {
            this.Undos.Push(item);
            // Let the event in Form knows when to enable or disable the button
            handler = new PropertyChangedEventArgs("Undo Available");
            this.CellPropertyChanged(item, handler);
        }

        // Function will be called in Form when trying to set the Text of the button 
        // This function will set the button's text according to the top of the Undo stack
        public string SetUndoButtonText()
        {
            if (Undos.Count != 0)
            {
                // Set the button's name to this if the top of the stack is a TextChanged Command
                if (Undos.Peek() is UndoRedoText)
                {
                    return "Undo Text Changed";
                }
                else if (Undos.Peek() is UndoRedoColor) // Set the button's name to this if the top of the stack is a ColorChanged Command
                {
                    return "Undo Color Changed";
                }
            }
            return "Undo";
        }

        // Function will be called in Form when trying to set the Text of the button 
        // This function will set the button's text according to the top of the Redo stack
        public string SetRedoButtonText()
        {
            if (Redos.Count != 0)
            {
                // Set the button's name to this if the top of the stack is a TextChanged Command
                if (Redos.Peek() is UndoRedoText)
                {
                    return "Redo Text Changed";
                }
                else if (Redos.Peek() is UndoRedoColor) // Set the button's name to this if the top of the stack is a TextChanged Command
                {
                    return "Redo Color Changed";
                }
            }

            return "Redo";
        }

        public void Undo()
        {
            if (this.Undos.Count > 0)
            {
                var undoCommand = this.Undos.Pop();
                undoCommand.UndoCollection();
                this.Redos.Push(undoCommand);
                // Let the event in Form knows when to enable or disable the button
                handler = new PropertyChangedEventArgs("Redo Available");
                this.CellPropertyChanged(this.Redos.Peek(), handler);
            }

            if (this.Undos.Count > 0)
            {
                // Let the event in Form knows when to enable or disable the button
                handler = new PropertyChangedEventArgs("Undo Available");
                this.CellPropertyChanged(this.Redos.Peek(), handler);
            }
            else
            {
                // Let the event in Form knows when to enable or disable the button
                handler = new PropertyChangedEventArgs("Undo UnAvailable");
                this.CellPropertyChanged(this.Redos.Peek(), handler);
            }
        }

        public void Redo()
        {
            if (this.Redos.Count > 0)
            {
                var redoCommand = this.Redos.Pop();
                redoCommand.RedoCollection();
                this.Undos.Push(redoCommand);
                // Let the event in Form knows when to enable or disable the button
                handler = new PropertyChangedEventArgs("Undo Available");
                this.CellPropertyChanged(this.Undos.Peek(), handler);
            }

            if (this.Redos.Count > 0)
            {
                // Let the event in Form knows when to enable or disable the button
                handler = new PropertyChangedEventArgs("Redo Available");
                this.CellPropertyChanged(this.Undos.Peek(), handler);
            }
            else
            {
                // Let the event in Form knows when to enable or disable the button
                handler = new PropertyChangedEventArgs("Redo UnAvailable");
                this.CellPropertyChanged(this.Undos.Peek(), handler);
            }
        }

        public void Load(Stream stream)
        {
            // Set the reader settings
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;

            // Create a reader for the document
            XmlReader r = XmlReader.Create(stream, settings);

            Cell cell;

            // Read the top most item
            r.ReadStartElement("Puthypor_Sengkeo_Spreadsheet");

            while (r.Name == "Cell")
            {
                // Read the next cell
                r.ReadStartElement("Cell");

                // Read the name
                r.ReadStartElement("Name");
                // This content will be anything inside of <Name>....</Name>
                string name = r.ReadContentAsString();

                // 'name' will have an alphabet char as the column and an int as the row
                // but the row will be 1 off the actual row we want
                // i.e. name will get 'A0' from the top left cell, but the cell name is 'A1'
                int row;
                Int32.TryParse(name[1].ToString(), out row);
                string roww = (row + 1).ToString();
                string newName = (name[0] + roww).ToString();

                cell = this.GetCellFromName(newName);
                r.ReadEndElement(); // </Name>

                if (r.Name == "Color")
                {
                    // Read the color
                    r.ReadStartElement("Color");
                    // This content will be anything inside of <Color>....</Color>
                    string color = r.ReadContentAsString();
                    // convert the color content into uint
                    uint result;
                    uint.TryParse(color, out result);
                    this.GetCellFromName(newName).Color = result;
                    r.ReadEndElement(); // </Color>
                }

                if (r.Name == "Text")
                {
                    // Read the text
                    r.ReadStartElement("Text");
                    // This content will be anything inside of <Text>....</Text>
                    string text = r.ReadContentAsString();
                    cell.Text = text;
                    r.ReadEndElement(); // </Text>
                }

                r.ReadEndElement(); // </Cell>
            }

            r.ReadEndElement(); // </Puthypor_Sengkeo_Spreadsheet>
        }

        public void Save(Stream stream)
        {
            // Set the writer settings
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";

            // Create a writer for the document
            XmlWriter w = XmlWriter.Create(stream, settings);

            // Start writing the document
            w.WriteStartDocument();

            // Write the start element of document "<Puthypor_Sengkeo_Spreadsheet>"
            w.WriteStartElement("Puthypor_Sengkeo_Spreadsheet");

            // Loop to go through the spreadsheet
            for (int i = 0; i < this.RowCount; i++)
            {
                for (int j = 0; j < this.ColumnCount; j++)
                {
                    // Check each cell if it has been changed or not
                    Cell cell = this.GetCell(i, j);
                    if (cell.Text != string.Empty || cell.Color != 0xFFFFFFFF)
                    {
                        // Create a Cell block to write
                        w.WriteStartElement("Cell");

                        // Write the Name of that Cell
                        w.WriteStartElement("Name");
                        w.WriteString(alphabet[cell.ColumnIndex]+cell.RowIndex.ToString());
                        w.WriteEndElement(); // </Name>

                        // Don't write the Color element if the Color is White
                        if (cell.Color != 0xFFFFFFFF)
                        {
                            // Write the Color of that Cell
                            w.WriteStartElement("Color");
                            w.WriteString(cell.Color.ToString());
                            w.WriteEndElement(); // </Color>
                        }

                        // Don't write the Text element if the Text is empty
                        if (cell.Text != string.Empty)
                        {
                            // Write the Text of that Cell
                            w.WriteStartElement("Text");
                            w.WriteString(cell.Text);
                            w.WriteEndElement(); // </Text>
                        }

                        w.WriteEndElement(); // </Cell>
                    }
                }
            }

            // Write the end element of Cell "</Puthypor_Sengkeo_Spreadsheet>"
            w.WriteEndElement();
            // Write the end of the document
            w.WriteEndDocument();
            // Close the document
            w.Close();
        }
    }
}
