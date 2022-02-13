using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace ProyectoInterfaces
{
    /// <summary>
    /// Lógica de interacción para Registro.xaml
    /// </summary>
    public partial class Registro : Window
    {
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "ogmc5dN7iMkrFG4w3H4mr5C5LcdkroyOzMhP6v0d",
            BasePath = "https://proyectointerfaces-3f5db-default-rtdb.europe-west1.firebasedatabase.app/"
        };

        IFirebaseClient client;
        public Registro()
        {
            InitializeComponent();
            client = new FireSharp.FirebaseClient(config);
        }


        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();

        }

        private void CerrarBtn1_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void RegistrarseBtn1_Click(object sender, RoutedEventArgs e)
        {
            registraUsuario();
        }

        private async void registraUsuario()
        {
            //comprobar que no haya campos vacios
            if (string.IsNullOrWhiteSpace(txt_usuario.Text) || string.IsNullOrWhiteSpace(txt_contraseña.Password) || string.IsNullOrWhiteSpace(txt_contraseña2.Password))

            {
                DialogHostCampos.IsOpen = true;

            }
            else
            {

                if (txt_contraseña.Password != txt_contraseña2.Password)
                {
                    DialogHostPassword.IsOpen = true;
                }
                else
                {
                    try
                    {
                        FirebaseResponse resp = await client.GetTaskAsync("Contador/cntUsuarios");
                        Contador get = resp.ResultAs<Contador>();

                        var logreg = new LogReg
                        {
                            Id = (Convert.ToInt32(get.cnt) + 1).ToString(), //añado +1 para seguir añadiendo usuarios
                            Usuario = txt_usuario.Text,
                            Contraseña = Protect(txt_contraseña.Password.ToString())

                        };

                        SetResponse response = await client.SetTaskAsync("usuarios/" + logreg.Id, logreg);
                        LogReg result = response.ResultAs<LogReg>();

                        //contador para autoincremento en Firebase
                        var obj = new Contador
                        {
                            cnt = logreg.Id
                        };

                        SetResponse response1 = await client.SetTaskAsync("Contador/cntUsuarios", obj);

                        //abro el dialogo personalizado creaddo con materialDesign
                        //el dialogo contendrá un botón de "aceptar", que al ser pulsado te llevará a la pantalla principal
                        DialogHost1.IsOpen = true;


                    }
                    catch (Exception ex)
                    {

                    }
                }

            }

        }

        private void btnAceptar_Click(object sender, RoutedEventArgs e)
        {
            //vuelvo a la pantalla del Login
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                Login login = new Login();
                login.Show();
                this.Close();
            });
        }


        //metodos para encriptar y desencriptar el password
        //https://stackoverflow.com/questions/22435561/encrypting-credentials-in-a-wpf-application
        public static string Protect(string str)
        {
            byte[] entropy = Encoding.ASCII.GetBytes(Assembly.GetExecutingAssembly().FullName);
            byte[] data = Encoding.ASCII.GetBytes(str);
            string protectedData = Convert.ToBase64String(ProtectedData.Protect(data, entropy, DataProtectionScope.CurrentUser));
            return protectedData;
        }

        public static string Unprotect(string str)
        {
            byte[] protectedData = Convert.FromBase64String(str);
            byte[] entropy = Encoding.ASCII.GetBytes(Assembly.GetExecutingAssembly().FullName);
            string data = Encoding.ASCII.GetString(ProtectedData.Unprotect(protectedData, entropy, DataProtectionScope.CurrentUser));
            return data;
        }



    }
}
