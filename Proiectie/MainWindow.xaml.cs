using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Proiectie
{
    

    public partial class MainWindow : Window
    {
        private ProjectionWindow _projectionWindow;
        private List<Song> _listaFavorite = new List<Song>();
        private List<ProjectionWindow> _projectionWindows = new List<ProjectionWindow>();

        public string _connectionString = $"Data Source={System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cantece.db")}";
        public MainWindow()
        {
            InitializeComponent();
            Functions.IncarcaCantari(this);
            txtSearch.TextChanged += TxtSearch_TextChanged;
            txtSearch.KeyDown += TxtSearch_KeyDown;
        }

        private void SearchType_Changed(object sender, RoutedEventArgs e)
        {
            if (txtSearch != null)
                Functions.IncarcaCantari(this, txtSearch.Text);
        }
        private void btnToggleProjection_Click(object sender, RoutedEventArgs e)
        {
            if (_projectionWindows.Count == 0)
            {
                var screens = System.Windows.Forms.Screen.AllScreens;

                foreach (var screen in screens)
                {
                    if (screen.Primary)
                        continue;

                    ProjectionWindow window = new ProjectionWindow();

                    window.Left = screen.WorkingArea.Left;
                    window.Top = screen.WorkingArea.Top;
                    window.Width = screen.WorkingArea.Width;
                    window.Height = screen.WorkingArea.Height;

                    window.Show();
                    window.WindowState = WindowState.Maximized;

                    _projectionWindows.Add(window);
                }

                if (_projectionWindows.Count > 0)
                {
                    btnToggleProjection.Content = "OPREȘTE PROIECȚIA";
                    btnToggleProjection.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#C0392B"));
                    txtLivePreview.Text = "PROIECȚIE ACTIVĂ";
                }
            }
            else
            {
                _projectionWindow.Close();
                _projectionWindow = null;

                btnToggleProjection.Content = "DESCHIDE PROIECȚIA";
                btnToggleProjection.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#28A745"));
                txtLivePreview.Text = "PROIECȚIE OPRITĂ";
            }
        }
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            Functions.IncarcaCantari(this, txtSearch.Text);
        }
        private void BtnStergeFavorite_Click(object sender, RoutedEventArgs e)
        {
            if (lstFavorite.SelectedItem is Song selectata)
            {
                _listaFavorite.Remove(selectata);
                lstFavorite.ItemsSource = null;
                lstFavorite.ItemsSource = _listaFavorite;
            }
        }

        private void lstCantari_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCantari.SelectedItem is Song selectata)
                Functions.IncarcaDetaliiCantare(this, selectata);
        }

        private void Strofa_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;

            if (button != null)
            {
                string textStrofa = button.DataContext.ToString();
                txtLivePreview.Text = textStrofa;

                foreach (var window in _projectionWindows)
                {
                    if (window.IsLoaded)
                    {
                        window.SetText(textStrofa);
                    }
                }
            }
        }

        private void Blackout_Click(object sender, RoutedEventArgs e)
        {
            _projectionWindow?.SetText("");
            txtLivePreview.Text = "- MUTE -";
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (lstCantari.SelectedItem is Song selectata)
            {
                if (!_listaFavorite.Any(c => c.Id == selectata.Id))
                {
                    _listaFavorite.Add(selectata);
                    lstFavorite.ItemsSource = null;
                    lstFavorite.ItemsSource = _listaFavorite;
                }
            }
        }
        private void lstFavorite_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstFavorite.SelectedItem is Song selectata)
                Functions.IncarcaDetaliiCantare(this, selectata);
        }
        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.Owner = this; 
            settings.ShowDialog(); 
        }
        private void TxtSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (lstCantari.Items.Count > 0)
                {
                    lstCantari.Focus();
                    lstCantari.SelectedIndex = 0;
                    if (lstCantari.SelectedItem is Song selectata)
                    {
                        Functions.IncarcaDetaliiCantare(this, selectata);
                    }
                    e.Handled = true;
                }
            }
        }
    }
}