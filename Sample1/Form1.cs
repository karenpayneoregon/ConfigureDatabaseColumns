using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseConfiguration;

namespace Sample1
{
    public partial class Form1 : Form
    {
        BindingSource _bsTables = new BindingSource();
        BindingSource _bsColumns = new BindingSource();
        ConfigureOperations _columnOperations = new ConfigureOperations();

        public Form1()
        {
            InitializeComponent();
            Shown += Form1_Shown;
        }

        private void ColumnsDataTable_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {

            var currentRowData = new ColumnDetails()
            {
                id = e.Row.Field<int>("Id"),
                TableId = e.Row.Field<int>("TableId"),
                Name = e.Row.Field<string>("ColumnName"),
                Title = e.Row.Field<string>("ColumnHeaderText"),
                Visible = e.Row.Field<bool>("Visible"),
                OrdinalPosition = e.Row.Field<int>("OrdinalPosition")
            };

            if (!_columnOperations.UpdateColumn(currentRowData))
            {
                MessageBox.Show($"Update failed{Environment.NewLine}{_columnOperations.LastExceptionMessage}");
            }

        }

        private void Form1_Shown(object sender, EventArgs e)
        {

            _bsTables.DataSource = _columnOperations.Tables();
            TableNameslistBox.DataSource = _bsTables;

            _bsColumns.DataSource = _columnOperations.Columns();
            dataGridView1.DataSource = _bsColumns;

            // ReSharper disable once PossibleNullReferenceException
            dataGridView1.Columns["ColumnName"].ReadOnly = true;

            ((DataTable)_bsColumns.DataSource).ColumnChanged += ColumnsDataTable_ColumnChanged;

            dataGridView1.CellContentClick += DataGridView1_CellContentClick;

        }
        /// <summary>
        /// Changing the Visible DataGridViewCheckBoxColumn will not trigger ColumnChanged
        /// event for the Columns DataTable, by invoking EndEdit this triggers the
        /// ColumnChanged event to fire.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            // ReSharper disable once PossibleNullReferenceException
            if (e.ColumnIndex == dataGridView1.Columns["Visible"].Index)
            {
                dataGridView1.EndEdit();
            }

        }

        private void TableNameslistBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _bsColumns.Filter = $"TableId = {((TableDetails)_bsTables.Current).TableId}";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var test = new UpdateConfiguration();
        }
    }
}
