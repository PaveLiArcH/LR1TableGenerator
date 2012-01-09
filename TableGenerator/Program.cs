using System;
using System.Collections.Generic;
using System.Windows;

namespace TableGenerator
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
			cApplication _application = new cApplication();
            _application.Run();
			//Application.EnableVisualStyles();
			//Application.SetCompatibleTextRenderingDefault(false);
        }
    }

    public class cApplication : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            fwMain _form = new fwMain();
			this.MainWindow = _form;
			_form.Show();
        }
    }
}
