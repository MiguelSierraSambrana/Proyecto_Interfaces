using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ProyectoInterfaces
{
    public partial class Principal : Window
    {
        //creo la tabla que meteré en el DataGrid
        DataTable dt = new DataTable();


        //login con Firebase
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "ogmc5dN7iMkrFG4w3H4mr5C5LcdkroyOzMhP6v0d",
            BasePath = "https://proyectointerfaces-3f5db-default-rtdb.europe-west1.firebasedatabase.app/"
        };

        IFirebaseClient client;

        public Principal()
        {

            InitializeComponent();
            client = new FireSharp.FirebaseClient(config);
            //creo las columnas que se verán en el DataGrid
            //info tablas en el DataGrid: https://stackoverflow.com/questions/5398441/how-to-set-the-datasource-of-a-datagrid-in-wpf
            //para hacer las tablas https://stackoverflow.com/questions/704724/programmatically-add-column-rows-to-wpf-datagrid
            dt.Columns.Add("id");
            dt.Columns.Add("titulo");
            dt.Columns.Add("artista");
            dt.Columns.Add("año");
            dt.Columns.Add("discografica");
            dt.Columns.Add("imagen", typeof(BitmapImage));
            DataGrid1.ItemsSource = dt.DefaultView;


        }

        //METODOS BOTONES
        private async void InsertBtn1_Click(object sender, RoutedEventArgs e)
        {

            //si los campos están vacíos aviso al usuario
            if (string.IsNullOrEmpty(Artista.Text) || string.IsNullOrEmpty(Titulo.Text) || string.IsNullOrEmpty(Año.Text) || string.IsNullOrEmpty(Discografica.Text) || CajaImagen.Source == null)
            {
                DialogHostCampos.IsOpen = true;
            }
            else
            {
                try

                {

                    FirebaseResponse resp = await client.GetTaskAsync("Contador/cntDiscos");
                    Contador get = resp.ResultAs<Contador>();

                    //para poder enviar la imagen la idea es: desde el control [Image] "CajaImagen" > MemoryStream > byte Array > tobase64String 
                    //metodo sacado parcialmente de https://stackoverflow.com/questions/24338592/how-to-convert-a-system-windows-control-image-to-system-drawing-image-in-c-s
                    //metodo sacado parcialmente de https://stackoverflow.com/questions/19134062/encode-a-filestream-to-base64-with-c-sharp
                    MemoryStream ms = new MemoryStream();
                    PngBitmapEncoder pbe = new PngBitmapEncoder();
                    pbe.Frames.Add(BitmapFrame.Create(new Uri(CajaImagen.Source.ToString(), UriKind.RelativeOrAbsolute)));
                    pbe.Save(ms);
                    byte[] a = ms.GetBuffer();
                    string output = Convert.ToBase64String(a);

                    //envio datos a FB
                    var data = new Data
                    {
                        Id = Cuenta().ToString(), //para llevar la cuenta paso el metodo "cuenta" que genera una lista de objetos
                        Titulo = Titulo.Text,
                        Artista = Artista.Text,
                        Año = Año.Text,
                        Discografica = Discografica.Text,
                        Img = output
                    };

                    SetResponse response = await client.SetTaskAsync("Discos/" + data.Id, data);
                    Data result = response.ResultAs<Data>();


                    //para el autoincremento
                    var obj = new Contador
                    {
                        cnt = data.Id  //devuelvo al contador de FB el valor actual
                    };

                    SetResponse response1 = await client.SetTaskAsync("Contador/cntDiscos", obj);

                    //dialogo personalizado
                    exportaTodo();
                    DialogHost1.IsOpen = true;

                }
                //MARAVILLOSO PARA MI CODIGO PATATA https://www.iteramos.com/pregunta/1196/capturar-varias-excepciones-a-la-vez
                catch (Exception ex)
                {
                    if (ex is NullReferenceException)
                    {
                        DialogHostError.IsOpen = true;


                    }
                    else if (ex is FileNotFoundException)
                    {
                        DialogHostErrorImage.IsOpen = true;

                    }

                }

            }
        }

        private void ActualizarBtn2_Click_1(object sender, RoutedEventArgs e)
        {

            //antes de lanzar el dialogo compruebo que todos los datos para actualizar el disco estén introducidos
            if (string.IsNullOrEmpty(Artista.Text) || string.IsNullOrEmpty(Titulo.Text) || string.IsNullOrEmpty(Año.Text) || string.IsNullOrEmpty(Discografica.Text) || CajaImagen.Source == null)
            {
                DialogHostCampos.IsOpen = true;
            }
            else
            {
                DialogHostActualizar.IsOpen = true;
            }
        }
        //boton ligado al de arriba ^
        private async void btnAceptar_actualizar_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(Id3.Text))
            {

                DialogHostError.IsOpen = true;
            }
            else
            {
                try
                {
                    FirebaseResponse resp2 = new FirebaseResponse();
                    resp2 = await client.GetTaskAsync("Discos/" + Id3.Text);
                    Data obj2 = resp2.ResultAs<Data>();


                    if (obj2.Id == null)
                    {
                        //
                    }
                    else
                    {

                        MemoryStream ms = new MemoryStream();
                        PngBitmapEncoder pbe = new PngBitmapEncoder();
                        pbe.Frames.Add(BitmapFrame.Create(new Uri(CajaImagen.Source.ToString(), UriKind.RelativeOrAbsolute)));
                        pbe.Save(ms);
                        byte[] a = ms.GetBuffer();
                        string output = Convert.ToBase64String(a);

                        var data = new Data
                        {
                            Id = Id3.Text,
                            Titulo = Titulo.Text,
                            Artista = Artista.Text,
                            Año = Año.Text,
                            Discografica = Discografica.Text,
                            Img = output
                        };

                        FirebaseResponse response = await client.UpdateTaskAsync("Discos/" + Id3.Text, data);
                        Data result = response.ResultAs<Data>();

                        //dialogo personalizado
                        DialogHost1.IsOpen = true;



                    }
                }
                //MARAVILLOSO PARA MI CODIGO PATATA https://www.iteramos.com/pregunta/1196/capturar-varias-excepciones-a-la-vez
                catch (Exception ex)
                {
                    if (ex is NullReferenceException)
                    {
                        DialogHostError.IsOpen = true;

                    }
                    else if (ex is FileNotFoundException)
                    {
                        DialogHostErrorImage.IsOpen = true;

                    }

                    /*(FileNotFoundException fne)
                    ERROR que me trajo de cabeza: cuando uso la opcion "recuperar Disco", si seguidamente utilizo la opcion "actualizar disco"
                    y no inserto una imagen nueva, el programa no encuentra la imagen y suelta un FileNotfoundException,
                    porque está en formato bitmapimage y no bitmap que es lo que necesita
                    así que la unica manera que se me ha encontrado de solucionarlo es pedir al usuario insertar una nueva imagen mediante una excepción
                    */

                }

            }

        }

        private void BorrarBtn3_Click(object sender, RoutedEventArgs e)
        {
            DialogHostBorrar.IsOpen = true;
        }
        //boton ligado al de arriba ^
        private async void btnAceptar_borrar_Click(object sender, RoutedEventArgs e)
        {
            //compruebo el numero del contador del disco para comprobar el ID
            if (string.IsNullOrEmpty(Id.Text))
            {

                DialogHostError.IsOpen = true;
            }
            else
            {

                try
                {
                    //primero hago una consulta con el ID así si no me devuelve el disco porque el ID no existe, saltará la excepción y el contador no decrementará
                    FirebaseResponse response = await client.GetTaskAsync("Discos/" + Id.Text);
                    Data data = response.ResultAs<Data>();

                    FirebaseResponse resp2 = await client.DeleteTaskAsync("Discos/" + Id.Text);
                    //DECREMENTAR EL CONTADOR
                    FirebaseResponse resp3 = await client.GetTaskAsync("Contador/cntDiscos");
                    Contador get = resp3.ResultAs<Contador>();

                    var obj3 = new Contador
                    {
                        cnt = (Convert.ToInt32(get.cnt) - 1).ToString()
                    };

                    SetResponse response1 = await client.SetTaskAsync("Contador/cntDiscos", obj3);
                    limpiar();
                    exportaTodo();



                }
                catch (NullReferenceException)
                {
                    DialogHostError.IsOpen = true;
                }

            }

        }


        private void RecuperarBtn4_Click(object sender, RoutedEventArgs e)
        {

            DialogHostRecuperar.IsOpen = true;

        }
        //boton ligado al de arriba ^
        private async void btnAceptar_recuperar_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(Id2.Text))
            {

                DialogHostError.IsOpen = true;
            }
            else
            {
                try
                {
                    //para que no se dupliquen los datos en el GridRow
                    dt.Rows.Clear();

                    //como el contador para autoincremento es manual, puede pasar que borre un disco con un ID que ya no exista, por ello controlo la excepcion de esta manera
                    FirebaseResponse response = await client.GetTaskAsync("Discos/" + Id2.Text);
                    Data obj2 = response.ResultAs<Data>();

                    DataRow row = dt.NewRow();
                    row["id"] = obj2.Id;
                    row["titulo"] = obj2.Titulo;
                    row["artista"] = obj2.Artista;
                    row["año"] = obj2.Año;
                    row["discografica"] = obj2.Discografica;

                    Id2.Text = obj2.Id;
                    Titulo.Text = obj2.Titulo;
                    Artista.Text = obj2.Artista;
                    Año.Text = obj2.Año;
                    Discografica.Text = obj2.Discografica;
                    //para la imagen
                    byte[] b = Convert.FromBase64String(obj2.Img);
                    MemoryStream ms = new MemoryStream();
                    ms.Write(b, 0, Convert.ToInt32(b.Length));
                    Bitmap bm = new Bitmap(ms, false);

                    dt.Rows.Add(row);
                    row["imagen"] = ConvertBitmap(bm);

                    //metodo ConverBitmap sacado de https://stackoverflow.com/questions/37890121/fast-conversion-of-bitmap-to-imagesource
                    CajaImagen.Source = ConvertBitmap(bm);


                }
                catch (NullReferenceException ex)
                {
                    DialogHostError.IsOpen = true;

                }
            }
        }

        //metodo para buscar imagen para añadir a la ficha
        private void BuscarBtn5_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog op = new Microsoft.Win32.OpenFileDialog();
            op.Title = "Seleccione Imagen PNG";
            op.Filter = "Portable Network Graphic (*.png)|*.png";

            if (op.ShowDialog() == true)
            {
                CajaImagen.Source = new BitmapImage(new Uri(op.FileName));

            }
        }

        private void ExportarBtn6_Click(object sender, RoutedEventArgs e)
        {
            exportaTodo();
        }


        //icono de la basura para limpiar las casillas y la imagen
        private void LimpiarBtn7_Click(object sender, RoutedEventArgs e)
        {
            limpiar();
        }


        private async void exportaTodo()
        {
            //para que no se dupliquen los datos en el GridRow
            dt.Rows.Clear();


            int i = 0;
            FirebaseResponse resp1 = new FirebaseResponse();
            resp1 = await client.GetTaskAsync("Contador/cntDiscos");
            Contador obj1 = new Contador();
            obj1 = resp1.ResultAs<Contador>();
            int cnt = Convert.ToInt32(obj1.cnt);

            while (true)
            {
                //loop para recorrer todos los elementos de firebase, el limite es el contador

                i++;

                try
                {
                    FirebaseResponse resp2 = new FirebaseResponse();
                    resp2 = await client.GetTaskAsync("Discos/" + i);
                    Data obj2 = resp2.ResultAs<Data>();

                    DataRow row = dt.NewRow();
                    row["id"] = obj2.Id;
                    row["titulo"] = obj2.Titulo;
                    row["artista"] = obj2.Artista;
                    row["año"] = obj2.Año;
                    row["discografica"] = obj2.Discografica;


                    //para la imagen metodo sacado del boton RecuperarBtn7
                    byte[] b = Convert.FromBase64String(obj2.Img);

                    MemoryStream ms = new MemoryStream();
                    ms.Write(b, 0, Convert.ToInt32(b.Length));

                    Bitmap bm = new Bitmap(ms, false);
                    dt.Rows.Add(row);

                    //row["imagen"] = ConvertBitmap(bm);
                    row["imagen"] = ConvertBitmap(bm);

                }
                catch (NullReferenceException nr)

                {
                    //
                }

            }


        }



        //OTROS METODOS

        //para poder mover la ventana
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        //boton X para cerrar la App
        private void CerrarBtn1_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        //limpiar las casillas y la imagen
        private void limpiar()
        {
            CajaImagen.Source = new BitmapImage(new Uri("pack://application:,,,/InserteImagen.png"));
            Titulo.Text = null;
            Artista.Text = null;
            Año.Text = null;
            Id.Text = null;
            Discografica.Text = null;
            dt.Rows.Clear();
        }




        //metodo de conversion entre imagenes para poder visualizar correctamente la imagen en la casilla de imagen al recuperar el disco
        //https://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa
        public BitmapImage ConvertBitmap(System.Drawing.Bitmap bitmap)
        {
            MemoryStream ms2 = new MemoryStream();
            bitmap.Save(ms2, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms2.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms2;
            image.EndInit();

            return image;
        }

        //para contar los discos de FireBase y obtener el ID
        private int Cuenta()
        {
            try
            {
                int bandera = 0;
                FirebaseResponse response = client.Get("Discos/");
                List<Data> lista = response.ResultAs<List<Data>>();
                foreach (object obj in lista)
                {
                    bandera++;
                }

                return bandera;

            }

            //si el valor que devuelve es nulo porque no hay discos, salta la excepcion y devuelve 1 

            catch (NullReferenceException nre)
            {
                return 1;

            }


        }


    }
}
