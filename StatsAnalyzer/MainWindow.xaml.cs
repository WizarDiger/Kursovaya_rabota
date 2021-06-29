using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Npgsql;



namespace StatsAnalyzer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string connstring = String.Format("Server={0};Port={1};" + "User Id={2};Password={3};Database={4};", "localhost", 5432, "postgres", "15082000", "StatsGatherer");
        private NpgsqlConnection conn;
        private string sql;
        private NpgsqlCommand cmd;
        private DataTable dt;
        static float leftb;
        static float rightb;
        static float upb;
        static float downb;
        private float xres;
        private float yres;
        private float maxy;
        private float minx;
        private float miny;
        int[,] matrix;
        private int blocksize = 20;
        public MainWindow()
        {
            
            this.Visibility = Visibility.Hidden;
            InitializeComponent();
            Hide();
            MainForm form = new MainForm();
            form.ShowDialog();
            Close();
            cbColors.SelectedIndex = 0;
           
            
        }
        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }
        public MainWindow(bool doNotMakeInvisible)
        {
            InitializeComponent();
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            MainForm form = new MainForm();
            form.ShowDialog();
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RenderContent();
        }
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RenderContent();
        }
        private void RenderContent()
        {
            cHeatMap.Clear();

            Random rRand = new Random();

          
            float x;
            float y;
            byte intense;
            conn = new NpgsqlConnection(connstring);
            conn.Open();

      
            sql = @"SELECT img FROM level_picture WHERE id = 1";
            cmd = new NpgsqlCommand(sql, conn);
            byte[] byteArray = (byte[])cmd.ExecuteScalar();
            Image1.Source = LoadImage(byteArray);
   

            sql = @"SELECT leftb FROM level_border WHERE id = 1";
            cmd = new NpgsqlCommand(sql, conn);
            leftb = float.Parse(cmd.ExecuteScalar().ToString());
            sql = @"SELECT rightb FROM level_border WHERE id = 1";
            cmd = new NpgsqlCommand(sql, conn);
            rightb = float.Parse(cmd.ExecuteScalar().ToString());
            sql = @"SELECT upb FROM level_border WHERE id = 1";
            cmd = new NpgsqlCommand(sql, conn);
            upb = float.Parse(cmd.ExecuteScalar().ToString());
            sql = @"SELECT downb FROM level_border WHERE id = 1";
            cmd = new NpgsqlCommand(sql, conn);
            downb = float.Parse(cmd.ExecuteScalar().ToString());
            sql = @"SELECT count(*) AS exact_count FROM player_movement_tracker";
            cmd = new NpgsqlCommand(sql, conn);
            long amount_of_rows = (long)cmd.ExecuteScalar();
            sql = @"SELECT MAX(y) FROM player_movement_tracker";
            cmd = new NpgsqlCommand(sql, conn);
            maxy = float.Parse(cmd.ExecuteScalar().ToString())*16;
            sql = @"SELECT MIN(x) FROM player_movement_tracker";
            cmd = new NpgsqlCommand(sql, conn);
            minx = float.Parse(cmd.ExecuteScalar().ToString()) * 16;
            sql = @"SELECT MIN(y) FROM player_movement_tracker";
            cmd = new NpgsqlCommand(sql, conn);
            miny = float.Parse(cmd.ExecuteScalar().ToString()) * 16;
            xres = leftb < 0 ? (rightb - leftb)*16 : (rightb + leftb)*16;
            yres = downb < 0 ? (upb - downb)*16 : (upb + downb)*16;
            if (minx == 0) minx = minx + 1;
            if (miny == 0) miny = miny + 1;
            if (minx < 0 && miny < 0)
                matrix = new int[(int)((xres - minx) / blocksize), (int)((yres - miny) / blocksize)];
            if (minx > 0 && miny > 0)
                matrix = new int[(int)(xres / blocksize), (int)(yres / blocksize)];
            if (minx > 0 && miny < 0)
                matrix = new int[(int)(xres / blocksize), (int)((yres - miny) / blocksize)];
            if (minx < 0 && miny > 0)
                matrix = new int[(int)((xres - minx) / blocksize), (int)(yres / blocksize)];
            Console.WriteLine($"xscaler {xres} yscaler{yres}  matrix = {(int)(xres-minx) / blocksize} {(int)(yres-miny) / blocksize}");
            for (int i = 1; i < amount_of_rows; i++)
            {
                
                sql = $@"SELECT x FROM player_movement_tracker WHERE id = {i}";
                cmd = new NpgsqlCommand(sql, conn);
                //Console.WriteLine(minx);
                if (minx > 0)
                x = (float.Parse(cmd.ExecuteScalar().ToString())*16)-minx;
                else
                    x = (float.Parse(cmd.ExecuteScalar().ToString()) * 16)+minx;
                sql = $@"SELECT y FROM player_movement_tracker WHERE id = {i}";
                cmd = new NpgsqlCommand(sql, conn);
                y = (float.Parse(cmd.ExecuteScalar().ToString()) > 0) ? (maxy - (float.Parse(cmd.ExecuteScalar().ToString()) * 16)) :
                (-float.Parse(cmd.ExecuteScalar().ToString()) * 16);
       
                if (minx < 0 && miny < 0)
                    matrix[(int)((x - minx) / blocksize), (int)((y - miny) / blocksize)]++;
                if (minx > 0 && miny > 0)
                    matrix[(int)(x / blocksize), (int)(y / blocksize)]++;
                if (minx > 0 && miny < 0)
                    matrix[(int)(x / blocksize), (int)((y - miny) / blocksize)]++;
                if (minx < 0 && miny > 0)
                    matrix[(int)((x - minx) / blocksize), (int)(y / blocksize)]++;
                //matrix[(int)((x - minx) / blocksize), (int)(y/blocksize) ]++;
                //cHeatMap.AddHeatPoint(new HeatPoint(x, y, 100));

                //Console.WriteLine("Y = " + y);
                intense = (byte)rRand.Next(0, 255);

                
                //cHeatMap.AddHeatPoint(new HeatPoint(x, y, intense));
            }
            int arrayMin = matrix.Cast<int>().Min();
            int arrayMax = matrix.Cast<int>().Max();
           
            Console.WriteLine("ArrayMin" + arrayMin);
            Console.WriteLine("ArrayMax" + arrayMax);
           
            for (int i = 0; i < (int)(xres / blocksize); i++)
            {
                for (int j = 0; j < (int)(yres / blocksize); j++)
                {
                    matrix[i, j] = (matrix[i, j]*255)/arrayMax;

                    //Console.Write($"{matrix[i, j]}\t");
                    cHeatMap.AddHeatPoint(new HeatPoint(i*blocksize+blocksize , j*blocksize+blocksize , (byte)(matrix[i, j])));
                }
                Console.WriteLine();
            }


            conn.Close();
            cHeatMap.Render();
        }
       
    }
}
