// <copyright file="SpreadSheet.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace SpreadsheetEngine
{
    public interface UndoRedoCollection
    {
        void UndoCollection();
        void RedoCollection();
    }

    public class UndoRedoText : UndoRedoCollection
    {
        private string cur_Text;
        private string prev_Text;
        private Cell temp_Cell;

        public UndoRedoText(Cell temp_Cell, string prev_Text, string cur_Text)
        {
            this.cur_Text = cur_Text;
            this.prev_Text = prev_Text;
            this.temp_Cell = temp_Cell;
        }

        public void RedoCollection()
        {
            this.temp_Cell.Text = this.cur_Text;
        }

        public void UndoCollection()
        {
            this.temp_Cell.Text = this.prev_Text;
        }
    }

    public class UndoRedoColor : UndoRedoCollection
    {
        private uint cur_Color;
        private List<uint> prev_Color;
        private List<Cell> temp_Cell;

        public UndoRedoColor(List<Cell> temp_Cell, List<uint> prev_Color, uint cur_Color)
        {
            this.cur_Color = cur_Color;
            this.prev_Color = prev_Color;
            this.temp_Cell = temp_Cell;
        }

        public void RedoCollection()
        {
            foreach (Cell cell in this.temp_Cell)
            {
                cell.Color = this.cur_Color;
            }
        }

        public void UndoCollection()
        {
            for (int i = 0; i < this.temp_Cell.Count; i++)
            {
                this.temp_Cell[i].Color = this.prev_Color[i];
            }
        }
    }
}