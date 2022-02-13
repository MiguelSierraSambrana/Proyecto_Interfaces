using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace ProyectoInterfaces
{
    /// <summary>
    /// Lógica de interacción para Splash.xaml
    /// </summary>
    public partial class Splash : Window
    {
        public Splash()
        {
            InitializeComponent();

        }

        //solución para la ProgressBar sacado de https://wpf-tutorial.com/es/65/controles-adicionales/the-progressbar-control/

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;

            worker.RunWorkerAsync();
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < 101; i += 4)
            {
                (sender as BackgroundWorker).ReportProgress(i);
                Thread.Sleep(100);

                //añado esto de abajo para que cuando el contador llegue a 100, abra la ventana "principal" y cierre esta.
                //lo de abrir y cerrar ventanas lo ví en los apuntes y en un post de StackOverflow https://stackoverflow.com/questions/52589293/switching-between-windows-wpf-mvc
                if (i == 100)
                {
                    //aún asi me daba un error "El subproceso de llamada debe ser STA" y econtré esto: https://qastack.mx/programming/2329978/the-calling-thread-must-be-sta-because-many-ui-components-require-this
                    //tenía que poner "Application.Current.Dispatcher.." para que no diera el error en el subproceso
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        Login login = new Login();
                        login.Show();
                        this.Close();
                    });

                }
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbStatus.Value = e.ProgressPercentage;
        }
    }
}



