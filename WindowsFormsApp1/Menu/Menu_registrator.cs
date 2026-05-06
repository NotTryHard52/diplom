using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Menu_registrator : Form
    {
        private int currentUserId; // ID текущего пользователя
        private string currentRole; // роль текущего пользователя
        private Form activeForm = null; // текущая открытая дочерняя форма
        Color activeColor = Color.FromArgb(91, 122, 196);   // активная
        Color defaultColor = Color.White; // обычная 

        // Конструктор формы Menu_registrator, принимает ФИО, ID пользователя и роль
        public Menu_registrator(string FIO, int userId, string role)
        {
            InitializeComponent();
            label_fio.Text = FIO; // отображаем ФИО пользователя
            currentUserId = userId;
            currentRole = role;
            this.FormClosing += Menu_registrator_FormClosing;
        }

        private void Menu_registrator_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Проверяем, что пользователь сам закрывает форму
            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult result = MessageBox.Show(
                    "Вы действительно хотите выйти?",
                    "Подтверждение выхода",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.No)
                {
                    e.Cancel = true; // отменяем закрытие
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

        // Метод для открытия дочерней формы внутри панели panel2
        private void OpenChildForm(Form childForm, Button clickedButton)
        {
            if (activeForm != null && activeForm.GetType() == childForm.GetType())
            {
                SetActiveButton(clickedButton);
                return;
            }

            // закрываем предыдущую
            if (activeForm != null)
            {
                activeForm.Close();
            }

            panel2.Controls.Clear();

            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            panel2.Controls.Add(childForm);
            childForm.Show();

            activeForm = childForm;

            SetActiveButton(clickedButton);
        }

        // Загрузка формы Menu_registrator
        private void Menu_registrator_Load(object sender, EventArgs e)
        {
            // При загрузке открываем главное меню
            OpenChildForm(new Main_menu(label_fio.Text, currentRole), button7);
        }

        // Кнопки для открытия различных дочерних форм
        private void button1_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Services(false), button1);
            label_fio.Visible = false; // скрываем ФИО
            label_role.Visible = false; // скрываем роль
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var patientForm = new Patient();
            patientForm.OnSessionExpired += ShowLoginForm;

            OpenChildForm(patientForm, button3);
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Talon(label_fio.Text, currentUserId), button5);
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var uchetForm = new UchetTalona();
            uchetForm.OnSessionExpired += ShowLoginForm;

            OpenChildForm(uchetForm, button6);
            label_fio.Visible = false;
            label_role.Visible = false;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Main_menu(label_fio.Text, currentRole), button7);
        }

        // Кнопка выхода с подтверждением
        private void button8_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
