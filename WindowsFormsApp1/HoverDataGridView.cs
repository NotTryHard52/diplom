using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class HoverDataGridView
    {
        private readonly DataGridView _dataGridView;
        private Color _hoverColor = Color.FromArgb(192, 255, 255);
        private Color _defaultColor = Color.White;
        private int _hoveredRow = -1;

        public HoverDataGridView(DataGridView dgv, Color? hoverColor = null, Color? defaultColor = null)
        {
            _dataGridView = dgv;

            EnableDoubleBuffering(_dataGridView);

            if (hoverColor.HasValue) _hoverColor = hoverColor.Value;
            if (defaultColor.HasValue) _defaultColor = defaultColor.Value;

            _dataGridView.CellMouseEnter += DataGridView_CellMouseEnter;
            _dataGridView.CellMouseLeave += DataGridView_CellMouseLeave;
        }

        private void EnableDoubleBuffering(DataGridView dgv)
        {
            typeof(DataGridView)
                .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(dgv, true, null);
        }

        private void DataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                _hoveredRow = e.RowIndex;
                _dataGridView.Rows[e.RowIndex].DefaultCellStyle.BackColor = _hoverColor;
            }
        }

        private void DataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (_hoveredRow >= 0 && _hoveredRow < _dataGridView.Rows.Count)
            {
                _dataGridView.Rows[_hoveredRow].DefaultCellStyle.BackColor = _defaultColor;
                _hoveredRow = -1;
            }
        }
    }
}
