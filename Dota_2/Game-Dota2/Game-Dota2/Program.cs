using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Game_Dota2
{
    static class Program
    {
        // Рестарт формы.
        public static bool Restart;
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
                if (Restart)
                    Application.Restart();
            }
            catch(Exception)
            {
                MessageBox.Show("????");
            }
        }
    }
}
