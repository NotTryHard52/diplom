using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    // Класс для подсветки строки DataGridView при наведении мыши
    public class HoverDataGridView
    {
        private readonly DataGridView _dataGridView; // Ссылка на DataGridView
        private Color _hoverColor = Color.FromArgb(39, 70, 144); // Цвет подсветки при наведении
        private Color _defaultColor = Color.White; // Стандартный цвет строки
        private int _hoveredRow = -1; // Индекс строки, на которой сейчас наведение

        // Конструктор класса. Можно задать свои цвета hover и default
        public HoverDataGridView(DataGridView dgv, Color? hoverColor = null, Color? defaultColor = null)
        {
            _dataGridView = dgv;

            EnableDoubleBuffering(_dataGridView); // Включаем двойную буферизацию для плавной отрисовки

            if (hoverColor.HasValue) _hoverColor = hoverColor.Value; // Если задан цвет подсветки, используем его
            if (defaultColor.HasValue) _defaultColor = defaultColor.Value; // Если задан цвет по умолчанию, используем его

            // Подписываемся на события наведения мыши
            _dataGridView.CellMouseEnter += DataGridView_CellMouseEnter;
            _dataGridView.CellMouseLeave += DataGridView_CellMouseLeave;
            _dataGridView.MouseLeave += DataGridView_MouseLeave;
            _dataGridView.Scroll += DataGridView_Scroll;
            _dataGridView.SizeChanged += DataGridView_SizeChanged;
            _dataGridView.DataBindingComplete += DataGridView_DataBindingComplete;
        }

        // Метод включения двойной буферизации DataGridView через Reflection
        private void EnableDoubleBuffering(DataGridView dgv)
        {
            typeof(DataGridView)
                .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(dgv, true, null); // Устанавливаем приватное свойство DoubleBuffered в true
        }

        // Событие при наведении мыши на ячейку
        private void DataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Проверяем, что это не заголовок
            {
                // Сбрасываем предыдущую подсветку, если она есть и отличается от текущей
                if (_hoveredRow >= 0 && _hoveredRow < _dataGridView.Rows.Count && _hoveredRow != e.RowIndex)
                {
                    try
                    {
                        _dataGridView.Rows[_hoveredRow].DefaultCellStyle.BackColor = _defaultColor;
                        _dataGridView.Rows[_hoveredRow].DefaultCellStyle.ForeColor = Color.Black;
                    }
                    catch { }
                }

                _hoveredRow = e.RowIndex; // Сохраняем индекс наведенной строки
                _dataGridView.Rows[e.RowIndex].DefaultCellStyle.BackColor = _hoverColor; // Меняем цвет строки
                _dataGridView.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.White; // Меняем цвет текста для лучшей видимости
            }
        }

        // Событие при уходе мыши с ячейки
        private void DataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            // При уходе мыши с ячейки - сбрасываем подсветку текущей строки
            if (e.RowIndex >= 0 && e.RowIndex < _dataGridView.Rows.Count)
            {
                try
                {
                    _dataGridView.Rows[e.RowIndex].DefaultCellStyle.BackColor = _defaultColor;
                    _dataGridView.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Black;
                }
                catch { }
            }

            // Если ушли с ячейки и это была подсвеченная строка - сбрасываем индекс
            if (_hoveredRow == e.RowIndex)
                _hoveredRow = -1;
        }

        private void DataGridView_MouseLeave(object sender, System.EventArgs e)
        {
            ResetHoverColors();
        }

        private void DataGridView_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
        {
            ResetHoverColors();
        }

        private void DataGridView_SizeChanged(object sender, System.EventArgs e)
        {
            ResetHoverColors();
        }

        private void DataGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // После биндинга очищаем возможные цвета и выбор
            ResetHoverColors();
            try { _dataGridView.ClearSelection(); } catch { }
        }

        private void ResetHoverColors()
        {
            try
            {
                for (int i = 0; i < _dataGridView.Rows.Count; i++)
                {
                    var row = _dataGridView.Rows[i];
                    // Сбрасываем только если явно стоит hover цвет или белым/black не соответствует
                    if (row.DefaultCellStyle.BackColor == _hoverColor || row.DefaultCellStyle.ForeColor == Color.White)
                    {
                        row.DefaultCellStyle.BackColor = _defaultColor;
                        row.DefaultCellStyle.ForeColor = Color.Black;
                    }
                }
            }
            catch { }
            _hoveredRow = -1;
        }
    }
}
