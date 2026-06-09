using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    // Статический класс для ограничения ввода в TextBox и DateTimePicker
    public static class InputLimit
    {
        // Ограничение ввода только английских символов (запрещаем русские)
        public static void English_Symbol(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            if (char.IsControl(e.KeyChar)) return; // Разрешаем управляющие символы (Backspace и т.д.)

            // Проверка на длину текста
            if (textBox.Text.Length >= textBox.MaxLength)
            {
                e.Handled = true;
                return;
            }

            // Запрещаем русские буквы
            if ((e.KeyChar >= 'А' && e.KeyChar <= 'я') || e.KeyChar == 'Ё' || e.KeyChar == 'ё')
            {
                e.Handled = true;
                return;
            }
        }

        public static void Captcha_Symbol(object sender, KeyPressEventArgs e, string allowedChars)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            if (char.IsControl(e.KeyChar)) return; // Backspace и т.д.

            // Ограничение длины
            if (textBox.Text.Length >= textBox.MaxLength)
            {
                e.Handled = true;
                return;
            }

            // Разрешаем только символы из капчи
            if (!allowedChars.Contains(e.KeyChar.ToString()))
            {
                e.Handled = true;
            }
        }

        // Ограничение ввода только русских букв и пробела
        public static void Russian(object sender, KeyPressEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            if (textBox.Text.Length >= textBox.MaxLength) { e.Handled = true; return; }
            if (char.IsControl(e.KeyChar)) return;

            // Если введен русский символ или пробел
            if ((e.KeyChar >= 'А' && e.KeyChar <= 'я') || e.KeyChar == 'Ё' || e.KeyChar == 'ё' || e.KeyChar == ' ')
            {
                e.Handled = true;

                int selectionStart = textBox.SelectionStart;
                string newChar = e.KeyChar.ToString();

                // Автокапитализация первой буквы
                if (textBox.TextLength == 0 && char.IsLetter(e.KeyChar))
                    newChar = newChar.ToUpper();

                textBox.Text = textBox.Text.Insert(selectionStart, newChar);
                textBox.SelectionStart = selectionStart + 1;
                return;
            }

            e.Handled = true; // Запрещаем все остальные символы
        }

        // Ограничение ввода только цифр
        public static void Numbers(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return; // Разрешаем управляющие символы
            if (!char.IsDigit(e.KeyChar)) e.Handled = true; // Запрещаем все, кроме цифр
        }

        // Ввод только русских букв и один дефис, с автокапитализацией
        public static void Russian_Hyphen(object sender, KeyPressEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;
            if (char.IsControl(e.KeyChar)) return;

            bool isRussianLetter = (e.KeyChar >= 'А' && e.KeyChar <= 'я') || e.KeyChar == 'Ё' || e.KeyChar == 'ё';

            // Разрешены только русские буквы и дефис
            if (!isRussianLetter && e.KeyChar != '-') { e.Handled = true; return; }

            int pos = textBox.SelectionStart;
            int sel = textBox.SelectionLength;

            // Проверка на MaxLength
            if ((sel == 0 && textBox.TextLength >= textBox.MaxLength) ||
                (sel > 0 && textBox.TextLength - sel + 1 > textBox.MaxLength))
            {
                e.Handled = true;
                return;
            }

            // Обработка дефиса
            if (e.KeyChar == '-')
            {
                if (pos == 0) { e.Handled = true; return; } // нельзя в начале
                if (textBox.Text.Contains("-") && sel == 0) { e.Handled = true; return; } // только один дефис
                if (pos > 0 && textBox.Text[pos - 1] == '-') { e.Handled = true; return; } // нельзя подряд
            }

            e.Handled = true;

            if (sel > 0) textBox.Text = textBox.Text.Remove(pos, sel); // удаляем выделение
            string newChar = e.KeyChar.ToString();

            if (pos == 0 && isRussianLetter) newChar = newChar.ToUpper(); // автокапитализация
            textBox.Text = textBox.Text.Insert(pos, newChar);
            textBox.SelectionStart = pos + newChar.Length;
        }

        // Ввод только русских букв без пробелов и дефисов
        public static void OnlyRussian(object sender, KeyPressEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;
            if (char.IsControl(e.KeyChar)) return;
            if (textBox.Text.Length >= textBox.MaxLength) { e.Handled = true; return; }

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

        // Настройка DateTimePicker для даты рождения (от 100 лет назад до сегодня)
        public static void Date(DateTimePicker picker)
        {
            if (picker == null) return;
            picker.MinDate = DateTime.Today.AddYears(-100);
            picker.MaxDate = DateTime.Today;
            picker.Format = DateTimePickerFormat.Custom;
            picker.CustomFormat = "dd.MM.yyyy";
            picker.Value = DateTime.Today.AddYears(-20); // значение по умолчанию
        }

        // Настройка DateTimePicker для даты записи/приёма (от сегодня до +14 дней)
        public static void DateOrder(DateTimePicker picker)
        {
            if (picker == null) return;
            picker.MinDate = DateTime.Today;
            picker.MaxDate = DateTime.Today.AddDays(13);
            picker.Format = DateTimePickerFormat.Custom;
            picker.CustomFormat = "dd.MM.yyyy";
            picker.Value = DateTime.Today; // значение по умолчанию
        }

        // Только русские буквы и пробел для ComboBox
        public static void RussianComboBox(object sender, KeyPressEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox == null) return;

            if (char.IsControl(e.KeyChar))
                return;

            bool isRussian =
                (e.KeyChar >= 'А' && e.KeyChar <= 'я') ||
                e.KeyChar == 'Ё' ||
                e.KeyChar == 'ё';

            // Разрешаем только русские буквы и пробел
            if (!isRussian && e.KeyChar != ' ')
            {
                e.Handled = true;
            }
        }
    }
}
