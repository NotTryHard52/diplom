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
                _hoveredRow = e.RowIndex; // Сохраняем индекс наведенной строки
                _dataGridView.Rows[e.RowIndex].DefaultCellStyle.BackColor = _hoverColor; // Меняем цвет строки
                _dataGridView.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.White; // Меняем цвет текста для лучшей видимости
            }
        }

        // Событие при уходе мыши с ячейки
        private void DataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (_hoveredRow >= 0 && _hoveredRow < _dataGridView.Rows.Count) // Проверяем валидность индекса
            {
                _dataGridView.Rows[_hoveredRow].DefaultCellStyle.BackColor = _defaultColor; // Возвращаем стандартный цвет
                _dataGridView.Rows[_hoveredRow].DefaultCellStyle.ForeColor = Color.Black; // Возвращаем стандартный цвет текста
                _hoveredRow = -1; // Сбрасываем индекс
            }
        }
    }
}
