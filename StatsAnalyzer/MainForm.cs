using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StatsAnalyzer
{
    public partial class MainForm : Form
    {
        private string connstring = String.Format("Server={0};Port={1};" + "User Id={2};Password={3};Database={4};", "localhost", 5432, "postgres", "15082000", "StatsGatherer");
        private NpgsqlConnection conn;
        private string sql;
        private NpgsqlCommand cmd;
        private DataTable dt;
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
            MainWindow main = new MainWindow(true);
            main.ShowDialog();
            Close();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            int max_score;
            int average_score;
            int average_jumps;
            int average_kills;
            int max_kills;
            int average_deaths;
            conn = new NpgsqlConnection(connstring);
            conn.Open();
            sql = @"SELECT SUM (score) AS total FROM player_stats";
            cmd = new NpgsqlCommand(sql, conn);
            average_score = Int32.Parse(cmd.ExecuteScalar().ToString());
            sql = @"SELECT count(*) AS exact_count FROM player_stats";
            cmd = new NpgsqlCommand(sql, conn);
            average_score = average_score / Int32.Parse(cmd.ExecuteScalar().ToString()); ;
            textBox6.Text = average_score.ToString();


            sql = @"SELECT SUM (amount_of_jumps) AS total FROM player_stats";
            cmd = new NpgsqlCommand(sql, conn);
            average_jumps = Int32.Parse(cmd.ExecuteScalar().ToString());
            sql = @"SELECT count(*) AS exact_count FROM player_stats";
            cmd = new NpgsqlCommand(sql, conn);
            average_jumps = average_jumps / Int32.Parse(cmd.ExecuteScalar().ToString()); ;
            textBox2.Text = average_jumps.ToString();

            sql = @"SELECT SUM (deaths) AS total FROM player_stats";
            cmd = new NpgsqlCommand(sql, conn);
            average_deaths = Int32.Parse(cmd.ExecuteScalar().ToString());
            sql = @"SELECT count(*) AS exact_count FROM player_stats";
            cmd = new NpgsqlCommand(sql, conn);
            average_deaths = average_deaths / Int32.Parse(cmd.ExecuteScalar().ToString()); ;
            textBox3.Text = average_deaths.ToString();

            sql = @"SELECT SUM (kills) AS total FROM player_stats";
            cmd = new NpgsqlCommand(sql, conn);
            average_kills = Int32.Parse(cmd.ExecuteScalar().ToString());
            sql = @"SELECT count(*) AS exact_count FROM player_stats";
            cmd = new NpgsqlCommand(sql, conn);
            average_kills = average_kills / Int32.Parse(cmd.ExecuteScalar().ToString()); ;
            textBox5.Text = average_kills.ToString();


            sql = @"SELECT MAX (kills) FROM player_stats";
            cmd = new NpgsqlCommand(sql, conn);
            max_kills = Int32.Parse(cmd.ExecuteScalar().ToString());
            textBox4.Text = max_kills.ToString();

            sql = @"SELECT MAX (score) FROM player_stats";
            cmd = new NpgsqlCommand(sql, conn);
            max_score = Int32.Parse(cmd.ExecuteScalar().ToString());
            textBox7.Text = max_score.ToString();
        }
    }
}
