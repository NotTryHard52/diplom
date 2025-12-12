using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class IdleManager
    {
        private Timer idleTimer;
        private int idleTimeSeconds = 0;
        private int idleTimeoutSeconds;

        public event EventHandler IdleTimeoutReached;

        public IdleManager(Control control)
        {
            // Читаем значение таймаута из App.config
            string timeoutValue = ConfigurationManager.AppSettings["IdleTimeoutSeconds"];
            if (!int.TryParse(timeoutValue, out idleTimeoutSeconds))
            {
                idleTimeoutSeconds = 30; // значение по умолчанию
            }

            // Создаем таймер
            idleTimer = new Timer();
            idleTimer.Interval = 1000; // проверка каждую секунду
            idleTimer.Tick += IdleTimer_Tick;

            // Подписываемся на события активности пользователя на переданном контроле
            control.MouseMove += ResetIdleTime;
            control.KeyPress += ResetIdleTime;
            control.MouseClick += ResetIdleTime;

            idleTimer.Start();
        }

        private void ResetIdleTime(object sender, EventArgs e)
        {
            idleTimeSeconds = 0;
        }

        private void IdleTimer_Tick(object sender, EventArgs e)
        {
            idleTimeSeconds++;
            if (idleTimeSeconds >= idleTimeoutSeconds)
            {
                idleTimer.Stop();
                IdleTimeoutReached?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Restart()
        {
            idleTimeSeconds = 0;
            idleTimer.Start();
        }
    }
}
