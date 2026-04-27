using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite; 

namespace Proiectie
{
    public class Cantare
    {
        public int Id { get; set; }
        public string Titlu { get; set; }
        public string Versuri { get; set; } 
    }

    public partial class MainWindow : Window
    {
        private ProjectionWindow _projectionWindow;
        private string _connectionString = $"Data Source={System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cantari.db")}";
        public MainWindow()
        {
            InitializeComponent();
            IncarcaCantari();
            txtSearch.TextChanged += TxtSearch_TextChanged;
        }

        private void IncarcaCantari(string filtru = "")
        {
            string dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cantari.db");

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                string numeTabelReal = "";
                var cmdGetTable = connection.CreateCommand();
                cmdGetTable.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";

                using (var reader = cmdGetTable.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        numeTabelReal = reader.GetString(0);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Baza de date pare să fie goală (nu are niciun tabel)!");
                        return;
                    }
                }

                List<Cantare> lista = new List<Cantare>();
                var command = connection.CreateCommand();

                command.CommandText = $"SELECT id, titlu, versuri FROM {numeTabelReal} WHERE titlu LIKE @filtru LIMIT 100";
                command.Parameters.AddWithValue("@filtru", $"%{filtru}%");

                try
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Cantare
                            {
                                Id = reader.GetInt32(0),
                                Titlu = reader.GetString(1),
                                Versuri = reader.GetString(2)
                            });
                        }
                    }
                    lstCantari.ItemsSource = lista;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Eroare la citire: " + ex.Message);
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            IncarcaCantari(txtSearch.Text);
        }

        private void lstCantari_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCantari.SelectedItem is Cantare selectata)
            {
                lblSelectedTitle.Text = selectata.Titlu;
                var strofe = Regex.Split(selectata.Versuri, @"(?=\d+\.)|(?:\r?\n){2,}")
                                  .Select(s => s.Trim())
                                  .Where(s => !string.IsNullOrWhiteSpace(s))
                                  .ToList();

                itemsStrofe.ItemsSource = strofe;
            }
        }

        private void Strofa_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;

            if (button?.DataContext != null)
            {
                string textStrofa = button.DataContext.ToString();

                string textCurat = Regex.Replace(textStrofa, @"^\d+\.\s*", "");
                textCurat = textCurat.Replace("/:", "").Replace(":/", "").Trim();

                txtLivePreview.Text = textCurat;

                if (_projectionWindow != null && _projectionWindow.IsLoaded)
                {
                    _projectionWindow.SetText(textCurat);
                }
            }
        }

        private void OpenProjection_Click(object sender, RoutedEventArgs e)
        {
            if (_projectionWindow == null || !_projectionWindow.IsLoaded)
            {
                _projectionWindow = new ProjectionWindow();
            }

            var screens = System.Windows.Forms.Screen.AllScreens;
            var secondary = screens.FirstOrDefault(s => !s.Primary) ?? screens[0];

            _projectionWindow.Left = secondary.Bounds.Left;
            _projectionWindow.Top = secondary.Bounds.Top;
            _projectionWindow.Width = secondary.Bounds.Width;
            _projectionWindow.Height = secondary.Bounds.Height;

            _projectionWindow.Show();
            _projectionWindow.WindowState = WindowState.Maximized;
        }

        private void Blackout_Click(object sender, RoutedEventArgs e)
        {
            _projectionWindow?.SetText("");
            txtLivePreview.Text = "- MUTE -";
        }

        public void BtnCloseProjection_Click(object sender, RoutedEventArgs e)
        {
            if (_projectionWindow != null)
            {
                _projectionWindow.Close();
                _projectionWindow = null;
                txtLivePreview.Text = "PROIECȚIE OPRITĂ";
            }
        }
    }
}