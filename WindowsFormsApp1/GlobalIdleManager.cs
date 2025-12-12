using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class GlobalIdleManager
    {
        private Timer idleTimer;
        private int idleTimeSeconds = 0;
        private int idleTimeoutSeconds;
        private List<Form> trackedForms = new List<Form>();

        public event EventHandler IdleTimeoutReached;

        public GlobalIdleManager(int timeoutSeconds = 0)
        {
            // Если таймаут не передан, читаем из App.config
            if (timeoutSeconds > 0)
            {
                idleTimeoutSeconds = timeoutSeconds;
            }
            else
            {
                string timeoutValue = ConfigurationManager.AppSettings["IdleTimeoutSeconds"];
                if (!int.TryParse(timeoutValue, out idleTimeoutSeconds))
                {
                    idleTimeoutSeconds = 30; // значение по умолчанию
                }
            }

            idleTimer = new Timer();
            idleTimer.Interval = 1000; // проверка каждую секунду
            idleTimer.Tick += IdleTimer_Tick;
            idleTimer.Start();
        }

        // Добавление формы для отслеживания активности
        public void TrackForm(Form form)
        {
            if (form == null || trackedForms.Contains(form)) return;

            trackedForms.Add(form);

            form.MouseMove += ResetIdleTime;
            form.MouseClick += ResetIdleTime;
            form.KeyPress += ResetIdleTime;
            form.FormClosed += (s, e) => trackedForms.Remove(form);

            // Рекурсивно подписываемся на все дочерние контролы
            TrackControls(form.Controls);
        }

        private void TrackControls(Control.ControlCollection controls)
        {
            foreach (Control ctrl in controls)
            {
                ctrl.MouseMove += ResetIdleTime;
                ctrl.MouseClick += ResetIdleTime;
                ctrl.KeyPress += ResetIdleTime;

                if (ctrl.HasChildren)
                {
                    TrackControls(ctrl.Controls);
                }
            }
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
