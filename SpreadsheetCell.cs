// <copyright file="Class1.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SpreadsheetEngine
{
    public class SpreadsheetCell : Cell
    {
        public SpreadsheetCell(int rowIndexValue, int columnIndexValue)
            : base(rowIndexValue, columnIndexValue)
        {
        }

        // The only(?) place to all a cell.value
        public void SetCell(SpreadsheetCell cell, string inputString)
        {
            cell.value = inputString;
        }

    }
}
