using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Menu : Form
    {
        private int currentUserId; // ID текущего пользователя
        private string currentRole; // роль текущего пользователя
        private Form activeForm = null; // ссылка на текущую открытую дочернюю форму
        Color activeColor = Color.FromArgb(91, 122, 196);   // активная
        Color defaultColor = Color.White; // обычная 

        // Конструктор формы Menu, принимает ФИО пользователя, его ID и роль
        public Menu(string FIO, int userId, string role)
        {
            InitializeComponent();
            label_fio.Text = FIO; // отображаем ФИО пользователя
            currentUserId = userId;
            currentRole = role;
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

        // Метод для открытия дочерней формы внутри панели panel2
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

        // Кнопка для открытия главного меню
        private void button7_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Main_menu(label_fio.Text, currentRole), button7);
        }

        // Кнопка для открытия формы пользователя
        private void button2_Click_1(object sender, EventArgs e)
        {
            OpenChildForm(new User(currentUserId), button2);
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

            // Если пользователь подтвердил, открываем форму логина
            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }

        // Кнопка открытия справочников
        private void button1_Click_1(object sender, EventArgs e)
        {
            var directoryForm = new Directory();

            // Подписка на событие выбора дочерней формы из справочников
            directoryForm.DictionarySelected += (childForm) =>
            {
                OpenChildForm(childForm, button1);
            };

            OpenChildForm(directoryForm, button1); // открываем форму справочников
        }

        private void Menu_Load(object sender, EventArgs e)
        {
            OpenChildForm(new Main_menu(label_fio.Text, currentRole), button7); // При старте открываем главное меню
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Doctor(), button3);
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Schedule(), button4);
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Import import = new Import();
            import.ShowDialog();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Backup backup = new Backup();
            backup.ShowDialog();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Export export = new Export();
            export.ShowDialog();
        }
    }
}
