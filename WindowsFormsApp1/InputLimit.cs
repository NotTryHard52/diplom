using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public static class InputLimit
    {
        public static void English_Symbol(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null)
                return;

            if (char.IsControl(e.KeyChar))
                return;

            if (textBox.Text.Length >= textBox.MaxLength)
            {
                e.Handled = true;
                return;
            }

            if ((e.KeyChar >= 'А' && e.KeyChar <= 'я') || e.KeyChar == 'Ё' || e.KeyChar == 'ё')
            {
                e.Handled = true;
                return;
            }
        }
        public static void Russian(object sender, KeyPressEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
                return;
            if (textBox.Text.Length >= textBox.MaxLength)
            {
                e.Handled = true;
                return;
            }
            if (char.IsControl(e.KeyChar))
                return;

            if ((e.KeyChar >= 'А' && e.KeyChar <= 'я') || e.KeyChar == 'Ё' || e.KeyChar == 'ё' || e.KeyChar == ' ')
            {
                e.Handled = true;

                int selectionStart = textBox.SelectionStart;
                string newChar = e.KeyChar.ToString();

                if (textBox.TextLength == 0 && char.IsLetter(e.KeyChar))
                {
                    newChar = newChar.ToUpper();
                }

                textBox.Text = textBox.Text.Insert(selectionStart, newChar);
                textBox.SelectionStart = selectionStart + 1;

                return;
            }
            e.Handled = true;
        }
        public static void Numbers(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
                return;

            if (!char.IsDigit(e.KeyChar))
                e.Handled = true;
        }
        public static void Russian_Hyphen(object sender, KeyPressEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
                return;

            if (char.IsControl(e.KeyChar))
                return;

            // только русские буквы + Ёё
            bool isRussianLetter =
                (e.KeyChar >= 'А' && e.KeyChar <= 'я') ||
                e.KeyChar == 'Ё' || e.KeyChar == 'ё';

            // Если не русская буква и не дефис → запрет
            if (!isRussianLetter && e.KeyChar != '-')
            {
                e.Handled = true;
                return;
            }

            int pos = textBox.SelectionStart;
            int sel = textBox.SelectionLength;

            // ---- ПРОВЕРКА НА MaxLength ----
            // Если нет выделения — просто нельзя выйти за предел
            if (sel == 0 && textBox.TextLength >= textBox.MaxLength)
            {
                e.Handled = true;
                return;
            }

            // Если есть выделение — можно заменять, но нельзя превысить MaxLength
            if (sel > 0 && textBox.TextLength - sel + 1 > textBox.MaxLength)
            {
                e.Handled = true;
                return;
            }

            // ---- ОБРАБОТКА ДЕФИСА ----
            if (e.KeyChar == '-')
            {
                // Запрет в начале
                if (pos == 0)
                {
                    e.Handled = true;
                    return;
                }

                // Запрет второго дефиса
                if (textBox.Text.Contains("-") && sel == 0)
                {
                    e.Handled = true;
                    return;
                }

                // Запрет двух подряд
                if (pos > 0 && textBox.Text[pos - 1] == '-')
                {
                    e.Handled = true;
                    return;
                }
            }

            e.Handled = true;

            // Удаляем выделенный текст (если есть)
            if (sel > 0)
                textBox.Text = textBox.Text.Remove(pos, sel);

            // символ который вставляем
            string newChar = e.KeyChar.ToString();

            // автокапитализация
            if (pos == 0 && isRussianLetter)
                newChar = newChar.ToUpper();

            // вставка
            textBox.Text = textBox.Text.Insert(pos, newChar);
            textBox.SelectionStart = pos + newChar.Length;
        }
        public static void OnlyRussian(object sender, KeyPressEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
                return;

            if (char.IsControl(e.KeyChar))
                return;
            if (textBox.Text.Length >= textBox.MaxLength)
            {
                e.Handled = true;
                return;
            }

            if ((e.KeyChar >= 'А' && e.KeyChar <= 'я') || e.KeyChar == 'Ё' || e.KeyChar == 'ё')
            {
                e.Handled = true;

                int selectionStart = textBox.SelectionStart;
                string newChar = e.KeyChar.ToString();

                if (textBox.TextLength == 0 && char.IsLetter(e.KeyChar))
                    newChar = newChar.ToUpper();

                textBox.Text = textBox.Text.Insert(selectionStart, newChar);
                textBox.SelectionStart = selectionStart + 1;
                return;
            }

            e.Handled = true;
        }
        public static void Date(DateTimePicker picker)
        {
            if (picker == null)
                return;

            picker.MinDate = DateTime.Today.AddYears(-100);

            picker.MaxDate = DateTime.Today;

            picker.Format = DateTimePickerFormat.Custom;
            picker.CustomFormat = "dd.MM.yyyy";

            picker.Value = DateTime.Today.AddYears(-20);
        }
        public static void DateOrder(DateTimePicker picker)
        {
            if (picker == null)
                return;

            picker.MinDate = DateTime.Today;

            picker.MaxDate = DateTime.Today.AddDays(14);

            picker.Format = DateTimePickerFormat.Custom;
            picker.CustomFormat = "dd.MM.yyyy";

            picker.Value = DateTime.Today;
        }
    }
}
