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
        private string _connectionString = $"Data Source={System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cantece.db")}";
        public MainWindow()
        {
            InitializeComponent();
            IncarcaCantari();
            txtSearch.TextChanged += TxtSearch_TextChanged;
        }

        private void IncarcaCantari(string filtru = "")
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                string filtruCurat = NormalizeazaText(filtru);
                string filtruSql = "%" + filtruCurat.Replace(" ", "%") + "%";
                List<Cantare> lista = new List<Cantare>();
                SqliteCommand command = new SqliteCommand("SELECT id, titlu, versuri FROM Cantece WHERE REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(LOWER(titlu), '-', ' '), ',', ' '), 'ă', 'a'), 'ţ', 't'), 'ş', 's'), 'â', 'a'), 'î', 'i'), 'ș', 's'), 'ț', 't') LIKE @filtru LIMIT 100", connection);
                command.Parameters.AddWithValue("@filtru", filtruSql);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if(reader.GetString(1) != "Titlu necunoscut")
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
                                  .Where(s => !s.StartsWith("I:"))
                                  .ToList();

                itemsStrofe.ItemsSource = strofe;
            }
        }

        private void Strofa_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;

            if (button != null)
            {
                string textStrofa = button.DataContext.ToString();
                txtLivePreview.Text = textStrofa;

                if (_projectionWindow != null && _projectionWindow.IsLoaded)
                {
                    _projectionWindow.SetText(textStrofa);
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

        public static string NormalizeazaText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            text = text.ToLower();

            text = text.Replace("ă", "a").Replace("â", "a").Replace("î", "i");
            text = text.Replace("ș", "s").Replace("ş", "s"); 
            text = text.Replace("ț", "t").Replace("ţ", "t"); 

            text = text.Replace("-", " ");
            text = Regex.Replace(text, @"[^a-z0-9]", " ");
            text = Regex.Replace(text, @"\s+", " ");
            return text.Trim();
        }
    }
}