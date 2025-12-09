using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Directory : Form
    {
        // Событие, через которое передаётся выбранная форма-справочник
        public event Action<Form> DictionarySelected;

        public Directory()
        {
            InitializeComponent(); // Инициализация формы
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Открытие справочника ролей
            DictionarySelected?.Invoke(new Role());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Открытие справочника статусов
            DictionarySelected?.Invoke(new Status());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Открытие справочника категорий
            DictionarySelected?.Invoke(new Category());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Открытие справочника специальностей
            DictionarySelected?.Invoke(new Speciality());
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Открытие справочника статусов приёма
            DictionarySelected?.Invoke(new StatusPriem());
        }
    }
}
