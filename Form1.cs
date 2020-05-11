// <copyright file="Form1.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Spreadsheet_Puthypor_Sengkeo
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;
    using SpreadsheetEngine;

    public partial class Form1 : Form
    {
        private SpreadSheet sheet;
        private char[] alphabet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        public Form1()
        {
            this.InitializeComponent();
            this.dataGridView1.Columns.Clear();

            // Create the columns and set their values
            for (int i = 0; i < this.alphabet.Length; i++)
            {
                this.dataGridView1.Columns.Add(this.alphabet[i].ToString(), this.alphabet[i].ToString());
            }

            // The initial width of the cell could not fit the values, so I had to manually change it
            this.dataGridView1.RowHeadersWidth = 50;

            // Create the rows and set their values
            for (int i = 0; i < 50; i++)
            {
                this.dataGridView1.Rows.Add();
                this.dataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();
            }

            // Set the dimensions of the spreadsheet
            this.sheet = new SpreadSheet(50, 26);

            // Subscribe the spreadsheet to the event
            this.sheet.CellPropertyChanged += this.OnCellPropertyChanged;
            this.sheet.ColorPropertyChanged += this.OnCellPropertyChanged;

            this.dataGridView1.CellBeginEdit += this.DataGridView1_CellBeginEdit;
            this.dataGridView1.CellEndEdit += this.DataGridView1_CellEndEdit;
            this.dataGridView1.BackgroundColorChanged += this.ChangeBackToolStripMenuItem_Click;
            this.undoToolStripMenuItem.Enabled = false;
            this.redoTextToolStripMenuItem.Enabled = false;
        }

        // Will be notified when a cell has been changed
        public void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is Cell)
            {
                Cell cell = (Cell)sender;
                this.dataGridView1.Rows[cell.RowIndex].Cells[cell.ColumnIndex].Value = cell.Value;
                this.dataGridView1.Rows[cell.RowIndex].Cells[cell.ColumnIndex].Style.BackColor = Color.FromArgb((int)cell.Color);
            }
            else if (sender is UndoRedoCollection)
            {
                // If the function gets in here then something must have happened to the Undos Redos stacks.
                if (e.PropertyName == "Undo Available")
                {
                    this.undoToolStripMenuItem.Enabled = true; // Enable Undo button
                }
                else if (e.PropertyName == "Undo UnAvailable")
                {
                    this.undoToolStripMenuItem.Enabled = false; // Disable Undo button
                }
                else if (e.PropertyName == "Redo Available")
                {
                    this.redoTextToolStripMenuItem.Enabled = true; // Enable Redo button
                }
                else if (e.PropertyName == "Redo UnAvailable")
                {
                    this.redoTextToolStripMenuItem.Enabled = false; // Disable Redo button
                }

                // Appropriately set the Text of the buttons
                this.undoToolStripMenuItem.Text = this.sheet.SetUndoButtonText();
                this.redoTextToolStripMenuItem.Text = this.sheet.SetRedoButtonText();
            }
        }

        /// <summary>
        /// Insert "I love C#" in 50 cells randomly
        /// Set every cell in Column B to "This is cell B#". where # number is the row number for the cell
        /// Set every cell in Column A to "=B#", A will have the same value as B.
        /// </summary>
        private void Button1_Click(object sender, EventArgs e)
        {
            Random rand = new Random();
            for (int i = 0; i < 50; i++)
            {
                int t_row = rand.Next(0, 50);
                int t_col = rand.Next(2, 26);
                string t_txt = this.sheet.GetCell(t_row, t_col).Text;
                this.sheet.GetCell(t_row, t_col).Text = "I love C#";
                UndoRedoText command = new UndoRedoText(this.sheet.GetCell(t_row, t_col), t_txt, this.sheet.GetCell(t_row, t_col).Text);
                this.sheet.AddUndo(command);

                string t_txt2 = this.sheet.GetCell(i, 1).Text;
                this.sheet.GetCell(i, 1).Text = "This is cell B" + (i + 1);
                UndoRedoText command1 = new UndoRedoText(this.sheet.GetCell(i, 1), t_txt2, this.sheet.GetCell(i, 1).Text);
                this.sheet.AddUndo(command1);

                string t_txt3 = this.sheet.GetCell(i, 0).Text;
                this.sheet.GetCell(i, 0).Text = "=B" + (i + 1);
                UndoRedoText command2 = new UndoRedoText(this.sheet.GetCell(i, 0), t_txt3, this.sheet.GetCell(i, 0).Text);
                this.sheet.AddUndo(command2);
            }
        }

        private void DataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            DataGridViewCell formCell = this.dataGridView1.CurrentCell;
            Cell cell = this.sheet.GetCell(e.RowIndex, e.ColumnIndex);
            formCell.Value = cell.Text;
        }

        private void DataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell formCell = this.dataGridView1.CurrentCell;
            Cell cell = this.sheet.GetCell(e.RowIndex, e.ColumnIndex);

            if (formCell.Value != null)
            {
                string temp = cell.Text;
                cell.Text = formCell.Value.ToString();

                UndoRedoText command = new UndoRedoText(cell, temp, cell.Text);
                this.sheet.AddUndo(command);
            }
            else
            {
                formCell.Value = string.Empty;
            }
        }

        private void ChangeBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Store the Cell as a list because user might select many cells at once
            List<Cell> temp_Cell = new List<Cell>();

            // Same goes for color
            List<uint> prev_Color = new List<uint>();

            ColorDialog myDialog = new ColorDialog();

            // Keeps the user from selecting a custom color.
            myDialog.AllowFullOpen = false;

            // Allows the user to get help. (The default is false.)
            myDialog.ShowHelp = true;

            if (myDialog.ShowDialog() == DialogResult.OK)
            {
                // The loop is used to handle when user selects more than 1 cell
                foreach (DataGridViewTextBoxCell cell in this.dataGridView1.SelectedCells)
                {
                    // Add each cell to the temp_Cell to use a parameter for the UndoRedoCollection
                    temp_Cell.Add(this.sheet.GetCell(cell.RowIndex, cell.ColumnIndex));

                    // Add each cell's previous color to the temp_Cell to use a parameter for the UndoRedoCollection
                    prev_Color.Add(this.sheet.GetCell(cell.RowIndex, cell.ColumnIndex).Color);

                    // Set the Cell's color to the color user selected
                    this.sheet.GetCell(cell.RowIndex, cell.ColumnIndex).Color = (uint)myDialog.Color.ToArgb();
                }

                // Add the changes we made to the cell(s) to the command stack
                UndoRedoColor command = new UndoRedoColor(temp_Cell, prev_Color, (uint)myDialog.Color.ToArgb());
                this.sheet.AddUndo(command);
            }
        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.sheet.Undo();
        }

        private void RedoTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.sheet.Redo();
        }

        private void LoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML File|*.xml";
            ofd.Title = "Open";
            ofd.ShowDialog();

            if (ofd.FileName != string.Empty)
            {
                FileStream file = (FileStream)ofd.OpenFile();

                this.sheet.Clear_Sheet();
                this.dataGridView1.Refresh();
                this.undoToolStripMenuItem.Text = "Undo";
                this.undoToolStripMenuItem.Enabled = false;
                this.redoTextToolStripMenuItem.Text = "Redo";
                this.redoTextToolStripMenuItem.Enabled = false;

                this.sheet.Load(file);

                file.Close();
            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "XML File|*.xml";
            sfd.Title = "Save";
            sfd.ShowDialog();

            if (sfd.FileName != string.Empty)
            {
                FileStream file = (FileStream)sfd.OpenFile();

                this.sheet.Save(file);

                file.Close();
            }
        }
    }
}
