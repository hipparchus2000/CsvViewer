using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvViewer
{
    public partial class CsvViewer : Form
    {
        public CsvViewer()
        {
            InitializeComponent();
        }

        private void openCsv_Click(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                var results = OpenCsvFile(openFileDialog1.FileName);
                foreach(var col in results.columns)
                {
                    var gridColumn = new DataGridViewColumn();
                    gridColumn.HeaderText = col;
                    gridColumn.CellTemplate = new DataGridViewTextBoxCell();
                    gridColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
                    dataGridView1.Columns.Add(gridColumn);
                }

                foreach(var line in results.data)
                {
                    var row = new DataGridViewRow();
                    var count = 0;
                    foreach(var value in line)
                    {
                        if(count<results.NumberOfFieldsFound)
                            row.Cells.Add(new DataGridViewTextBoxCell { Value = value });
                        count++;
                    }
                    dataGridView1.Rows.Add(row);

                }

            }
        }

        private void SaveCsv_Click(object sender, EventArgs e)
        {
            var result = saveFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                SaveCsvFile(saveFileDialog1.FileName);
            }
        }


        public class Results
        {
            public Results()
            {
                columns = new List<string>();
                data = new List<List<string>>();
            }

            public int NumberOfNewRecordsFound { get; internal set; }
            public int NumberOfFieldsFound { get; internal set; }
            public int NumberOfRecordsImported { get; internal set; }
            public List<string> columns { get; set; }
            public List<List<string>> data { get; set; }
        }

        public Results OpenCsvFile(string csvText)
        {
            var results = new Results();
            var nonNullLineCount = 0;

            var fileText = System.IO.File.ReadAllText(csvText);
            csvText = fileText.Replace("\n", "");
            var importLines = csvText.Split('\r');
            var data = new List<List<string>>();
            
            var lineNumber = 0;

            foreach (var importLine in importLines)
            {
                if (importLine != "")
                {
                    results.NumberOfNewRecordsFound++;
                    if (nonNullLineCount == 0)
                    {
                        results.columns = purgeCommasInTextFields(importLine).Split(',').ToList();
                        results.NumberOfFieldsFound = results.columns.Count;
                    }
                    else
                    {
                        results.NumberOfRecordsImported++;
                        
                        var columns = purgeCommasInTextFields(importLine).Split(',');
                        //var sha = sha256_hash(importLine);
                        if (lineNumber == 0)
                            results.columns = columns.ToList();
                        else
                            results.data.Add(columns.ToList());
                            results.NumberOfNewRecordsFound++;

                    }
                    nonNullLineCount++;
                } //if import line is not empty

                lineNumber++; 
            } // foreach import line

            return results;

        } //end of method


        private String purgeCommasInTextFields(String original)
        {
            String modified = "";
            bool insideQuotes = false;
            foreach (var character in original)
            {
                if (character == '"')
                    insideQuotes = !insideQuotes;
                if (character == ',' && insideQuotes)
                {
                    //do nothing
                }
                else
                {
                    modified = modified + character;
                }
            }
            return modified;
        }

        private void SaveCsvFile(String filename)
        {
            string data;
            var values = new List<string>();
            foreach(DataGridViewColumn col in dataGridView1.Columns)
            {
                values.Add(col.HeaderText);
            }
            data = String.Join(",",values)+"\r\n";
            
            foreach(DataGridViewRow row in dataGridView1.Rows)
            {
                values = new List<string>();
                foreach(DataGridViewCell cell in row.Cells)
                {
                    values.Add((string)cell.Value);
                }
                data += String.Join(",", values) + "\r\n";
            }

            System.IO.File.WriteAllText(filename, data);
        }

        private void dataGridView1_ColumnHeaderMouseClick(
                object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridViewColumn newColumn = dataGridView1.Columns[e.ColumnIndex];
            DataGridViewColumn oldColumn = dataGridView1.SortedColumn;
            ListSortDirection direction;

            // If oldColumn is null, then the DataGridView is not sorted.
            if (oldColumn != null)
            {
                // Sort the same column again, reversing the SortOrder.
                if (oldColumn == newColumn &&
                    dataGridView1.SortOrder == SortOrder.Ascending)
                {
                    direction = ListSortDirection.Descending;
                }
                else
                {
                    // Sort a new column and remove the old SortGlyph.
                    direction = ListSortDirection.Ascending;
                    oldColumn.HeaderCell.SortGlyphDirection = SortOrder.None;
                }
            }
            else
            {
                direction = ListSortDirection.Ascending;
            }

            // Sort the selected column.
            dataGridView1.Sort(newColumn, direction);
            newColumn.HeaderCell.SortGlyphDirection =
                direction == ListSortDirection.Ascending ?
                SortOrder.Ascending : SortOrder.Descending;
        }

        private void dataGridView1_DataBindingComplete(object sender,
            DataGridViewBindingCompleteEventArgs e)
        {
            // Put each of the columns into programmatic sort mode.
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.Programmatic;
            }
        }

    }
}
