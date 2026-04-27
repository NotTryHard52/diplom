using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private int currentUserId;       // ID текущего пользователя
        private string currentRole;      // Роль текущего пользователя
        private Form activeForm = null;  // Ссылка на текущую открытую дочернюю форму

        // Конструктор формы, принимает ФИО, ID и роль пользователя
        public Form1(string FIO, int userId, string role)
        {
            InitializeComponent();        // Инициализация компонентов формы
            label_fio.Text = FIO;         // Отображаем ФИО пользователя на форме
            currentUserId = userId;       // Сохраняем ID пользователя
            currentRole = role;           // Сохраняем роль пользователя
        }


        // Метод для открытия дочерней формы внутри главной формы
        private void OpenChildForm(Form childForm)
        {
            // Проверяем, открыта ли уже такая форма, чтобы не открывать дубликат
            if (activeForm != null && activeForm.GetType() == childForm.GetType())
            {
                return;
            }

            panel2.Controls.Clear(); // Очищаем панель, где будет отображаться дочерняя форма

            childForm.TopLevel = false;                     // Форма не является топ-уровнем
            childForm.FormBorderStyle = FormBorderStyle.None; // Убираем границы формы
            childForm.Dock = DockStyle.Fill;               // Растягиваем форму на всю панель

            panel2.Controls.Add(childForm); // Добавляем форму на панель
            childForm.Show();               // Показываем форму

            activeForm = childForm;         // Сохраняем ссылку на активную форму
        }

        // Кнопка 6 открывает форму учета талона
        private void button6_Click(object sender, EventArgs e)
        {
            OpenChildForm(new UchetTalona(true));
        }

        // Кнопка 7 открывает главное меню
        private void button7_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Main_menu(label_fio.Text, currentRole));
        }

        // Событие загрузки формы
        private void Form1_Load(object sender, EventArgs e)
        {
            OpenChildForm(new Main_menu(label_fio.Text, currentRole)); // При старте открываем главное меню
        }

        // Кнопка 8 для выхода из системы
        private void button8_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Вы действительно хотите выйти?",             // Текст сообщения
                "Подтверждение выхода",                      // Заголовок окна
                MessageBoxButtons.YesNo,                     // Кнопки Да/Нет
                MessageBoxIcon.Question                      // Значок вопроса
            );

            if (result == DialogResult.Yes)                 // Если пользователь подтвердил выход
            {
                Login login = new Login();                 // Создаем форму входа
                this.Hide();                               // Скрываем текущую форму
                login.ShowDialog();                        // Показываем форму входа
                this.Show();                               // После закрытия формы входа показываем текущую форму
            }
        }
    }
}
