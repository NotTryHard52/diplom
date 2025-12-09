using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Menu_registrator : Form
    {
        private int currentUserId; // ID текущего пользователя
        private string currentRole; // роль текущего пользователя
        private Form activeForm = null; // текущая открытая дочерняя форма

        // Конструктор формы Menu_registrator, принимает ФИО, ID пользователя и роль
        public Menu_registrator(string FIO, int userId, string role)
        {
            InitializeComponent();
            label_fio.Text = FIO; // отображаем ФИО пользователя
            currentUserId = userId;
            currentRole = role;
        }

        // Метод для открытия дочерней формы внутри панели panel2
        private void OpenChildForm(Form childForm)
        {
            // Если та же форма уже открыта, выходим
            if (activeForm != null && activeForm.GetType() == childForm.GetType())
            {
                return;
            }

            panel2.Controls.Clear(); // очищаем панель перед открытием новой формы

            // Настройка дочерней формы
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            panel2.Controls.Add(childForm); // добавляем форму в панель
            childForm.Show(); // показываем форму

            activeForm = childForm; // сохраняем текущую открытую форму
        }

        // Загрузка формы Menu_registrator
        private void Menu_registrator_Load(object sender, EventArgs e)
        {
            // При загрузке открываем главное меню
            OpenChildForm(new Main_menu(label_fio.Text, currentRole));
        }

        // Кнопки для открытия различных дочерних форм
        private void button1_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Services(false));
            label_fio.Visible = false; // скрываем ФИО
            label_role.Visible = false; // скрываем роль
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Doctor());
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Patient());
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Schedule());
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Talon(label_fio.Text, currentUserId));
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenChildForm(new UchetTalona());
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Main_menu(label_fio.Text, currentRole));
        }

        // Кнопка выхода с подтверждением
        private void button8_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Вы действительно хотите выйти?",
                "Подтверждение выхода",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            // Если пользователь подтвердил, показываем форму логина
            if (result == DialogResult.Yes)
            {
                Login login = new Login();
                this.Hide(); // скрываем текущую форму
                login.ShowDialog(); // показываем форму логина
                this.Show(); // возвращаем текущую форму после закрытия логина
            }
        }
    }
}
