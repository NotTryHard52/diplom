
namespace WindowsFormsApp1
{
    partial class EditDoctor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditDoctor));
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.button6 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.maskedTextBox1 = new System.Windows.Forms.MaskedTextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label6 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBox2
            // 
            this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(16, 243);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(198, 28);
            this.comboBox2.TabIndex = 42;
            // 
            // button6
            // 
            this.button6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.button6.Location = new System.Drawing.Point(16, 277);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(200, 37);
            this.button6.TabIndex = 41;
            this.button6.Text = "Редактировать";
            this.button6.UseVisualStyleBackColor = false;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.button1.Location = new System.Drawing.Point(243, 226);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(192, 34);
            this.button1.TabIndex = 40;
            this.button1.Text = "Сменить изображение";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 220);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(132, 20);
            this.label5.TabIndex = 38;
            this.label5.Text = "Специальность:";
            // 
            // maskedTextBox1
            // 
            this.maskedTextBox1.Location = new System.Drawing.Point(16, 191);
            this.maskedTextBox1.Mask = "+7 999 999 99 99";
            this.maskedTextBox1.Name = "maskedTextBox1";
            this.maskedTextBox1.Size = new System.Drawing.Size(198, 26);
            this.maskedTextBox1.TabIndex = 37;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 168);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(146, 20);
            this.label4.TabIndex = 36;
            this.label4.Text = "Номер телефона:";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(16, 139);
            this.textBox3.MaxLength = 40;
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(198, 26);
            this.textBox3.TabIndex = 35;
            this.textBox3.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox3_KeyPress);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 20);
            this.label3.TabIndex = 34;
            this.label3.Text = "Отчество:";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(16, 86);
            this.textBox2.MaxLength = 40;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(198, 26);
            this.textBox2.TabIndex = 33;
            this.textBox2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox2_KeyPress);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 20);
            this.label2.TabIndex = 32;
            this.label2.Text = "Имя:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(16, 33);
            this.textBox1.MaxLength = 40;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(198, 26);
            this.textBox1.TabIndex = 31;
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 20);
            this.label1.TabIndex = 30;
            this.label1.Text = "Фамилия:";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox1.Location = new System.Drawing.Point(243, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(192, 188);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 39;
            this.pictureBox1.TabStop = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(239, 203);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(51, 20);
            this.label6.TabIndex = 43;
            this.label6.Text = "Фото";
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.button2.Location = new System.Drawing.Point(243, 266);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(192, 34);
            this.button2.TabIndex = 44;
            this.button2.Text = "Удалить изображение";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // EditDoctor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(452, 333);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.comboBox2);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.maskedTextBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditDoctor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Редактирование врача";
            this.Load += new System.EventHandler(this.EditDoctor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.MaskedTextBox maskedTextBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button2;
    }
}