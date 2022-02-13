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
    /// Lógica de interacción para Login.xaml
    /// </summary>
    public partial class Login : Window
    {

        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "ogmc5dN7iMkrFG4w3H4mr5C5LcdkroyOzMhP6v0d",
            BasePath = "https://proyectointerfaces-3f5db-default-rtdb.europe-west1.firebasedatabase.app/"
        };

        IFirebaseClient client;


        public Login()
        {
            InitializeComponent();
            client = new FireSharp.FirebaseClient(config);
        }


        //permite mover la ventana
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();

        }

        private void CerrarBtn1_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LoginBtn1_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_usuario.Text) || string.IsNullOrWhiteSpace(txt_contraseña.Password))

            {
                DialogHostCampos.IsOpen = true;

            }
            else
            {
                compruebaUsuario();
            }
        }



        private async void compruebaUsuario()
        {

            int i = 0;
            FirebaseResponse resp = await client.GetTaskAsync("Contador/cntUsuarios");
            Contador obj = resp.ResultAs<Contador>();
            int cnt = Convert.ToInt32(obj.cnt);

            while (true)
            {
                //loop para recorrer todos los elementos de firebase, el limite es el contador
                if (i == cnt)
                {
                    break;
                }
                i++;

                try
                {

                    FirebaseResponse resultado = await client.GetTaskAsync("usuarios/" + i);

                    LogReg obj2 = resultado.ResultAs<LogReg>();

                    string usuario = obj2.Usuario;
                    string contraseña = obj2.Contraseña;
                    string id = obj2.Id;

                    if (txt_usuario.Text == usuario && txt_contraseña.Password.ToString() == Unprotect(contraseña))
                    {
                        //abro el dialogo personalizado creaddo con materialDesign
                        //el dialogo contendrá un botón de "aceptar", que al ser pulsado te llevará a la pantalla principal
                        DialogHost1.IsOpen = true;
                        return;  //hago esto para para el metodo, si no saldria la ventana de exito y la de error a la vez
                    }


                }
                catch (NullReferenceException nre)
                {
                    //
                }



            }
            //si el loop no encuentra el usuario, sale el dialogo de error
            DialogHostUsuarios2.IsOpen = true;


        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {

            //voy a la pantalla de registro
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                Registro registro = new Registro();
                registro.Show();
                this.Close();
            });
        }

        private void btnAceptar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                Principal principal = new Principal();
                principal.Show();
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