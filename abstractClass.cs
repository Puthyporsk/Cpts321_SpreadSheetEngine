// <copyright file="abstractClass.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SpreadsheetEngine
{
    using System.ComponentModel;
    using System.Collections.Generic;

    // This is a base class for all Cells, and what happens to it
    public abstract class Cell : INotifyPropertyChanged
    {
        public readonly int RowIndex;
        public readonly int ColumnIndex;
        protected string text;
        protected string value;
        public uint BGColor = 0xFFFFFFFF;

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler ReferencedCellPropertyChanged;
        public event PropertyChangedEventHandler ColorPropertyChanged;

        // Cell constructor to set the dimensions of the spreadsheet, and its values
        public Cell(int rowIndexValue, int columnIndexValue)
        {
            this.RowIndex = rowIndexValue;
            this.ColumnIndex = columnIndexValue;
            this.text = string.Empty;
            this.value = string.Empty;
            this.Color = 0xFFFFFFFF;
        }

        public int GetRowIndex()
        {
            return this.RowIndex;
        }

        public int GetColumnIndex()
        {
            return this.ColumnIndex;
        }

        public string Text
        {
            get
            {
                return this.text;
            }

            set
            {
                if (value != this.text)
                {
                    this.text = value;

                    // Invoke this event when text is changed
                    this.OnPropertyChanged(this, new PropertyChangedEventArgs("Text"));
                }
            }
        }

        public string Value
        {
            get 
            { 
                return this.value; 
            }

            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;

                this.ReferencedCellPropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value Referenced"));
            }
        }

        public uint Color
        {
            get { return this.BGColor; }

            set
            {
                if (value != this.BGColor)
                {
                    this.BGColor = value;

                    this.ColorPropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Color"));
                }
            }
        }

        // Function that handles a change that happens to Cell
        public virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        // Function that handles a change that happens to ReferencedCell
        protected virtual void OnReferencedCellPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(sender, e);
        }

        // Function that handles a change that happens to cell's color change
        public virtual void OnColorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(sender, e);
        }
    }
}
