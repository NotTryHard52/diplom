using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private int currentUserId;       // ID текущего пользователя
        private string currentRole;      // Роль текущего пользователя
        private Form activeForm = null;  // Ссылка на текущую открытую дочернюю форму
        Color activeColor = Color.FromArgb(91, 122, 196);   // активная
        Color defaultColor = Color.White; // обычная 

        // Конструктор формы, принимает ФИО, ID и роль пользователя
        public Form1(string FIO, int userId, string role)
        {
            InitializeComponent();        // Инициализация компонентов формы
            label_fio.Text = FIO;         // Отображаем ФИО пользователя на форме
            currentUserId = userId;       // Сохраняем ID пользователя
            currentRole = role;           // Сохраняем роль пользователя
        }

        private void IdleManager_IdleTimeoutReached(object sender, EventArgs e)
        {
            ShowLoginForm();
        }

        private void ShowLoginForm()
        {
            this.Hide();

            using (Login loginForm = new Login())
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    this.Show();
                }
                else
                {
                    Application.Exit();
                }
            }
        }

        private void SetActiveButton(Button activeBtn)
        {
            // сбрасываем все кнопки
            foreach (Control ctrl in panel1.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.BackColor = defaultColor;
                    btn.ForeColor = Color.Black;
                }
            }

            // подсвечиваем активную
            activeBtn.BackColor = activeColor;
            activeBtn.ForeColor = Color.White;
        }

        // Метод для открытия дочерней формы внутри главной формы
        private void OpenChildForm(Form childForm, Button clickedButton)
        {
            if (activeForm != null && activeForm.GetType() == childForm.GetType())
                return;

            panel2.Controls.Clear();

            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            panel2.Controls.Add(childForm);
            childForm.Show();

            activeForm = childForm;

            // подсветка кнопки
            SetActiveButton(clickedButton);
        }

        // Кнопка 6 открывает форму учета талона
        private void button6_Click(object sender, EventArgs e)
        {
            var uchetForm = new UchetTalona();
            uchetForm.OnSessionExpired += ShowLoginForm;

            OpenChildForm(uchetForm, button6);
        }

        // Кнопка 7 открывает главное меню
        private void button7_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Main_menu(label_fio.Text, currentRole), button7);
        }

        // Событие загрузки формы
        private void Form1_Load(object sender, EventArgs e)
        {
            OpenChildForm(new Main_menu(label_fio.Text, currentRole), button7); // При старте открываем главное меню
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
