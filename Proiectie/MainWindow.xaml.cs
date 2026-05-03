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
            if (_projectionWindow == null || !_projectionWindow.IsLoaded)
            {
                _projectionWindow = new ProjectionWindow();

                var screens = System.Windows.Forms.Screen.AllScreens;
                var secondary = screens.FirstOrDefault(s => !s.Primary) ?? screens[0];

                _projectionWindow.Left = secondary.Bounds.Left;
                _projectionWindow.Top = secondary.Bounds.Top;
                _projectionWindow.Width = secondary.Bounds.Width;
                _projectionWindow.Height = secondary.Bounds.Height;

                _projectionWindow.Show();
                _projectionWindow.WindowState = WindowState.Maximized;

                btnToggleProjection.Content = "OPREȘTE PROIECȚIA";
                btnToggleProjection.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#C0392B"));
                txtLivePreview.Text = "PROIECȚIE ACTIVĂ";
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

                if (_projectionWindow != null && _projectionWindow.IsLoaded)
                {
                    _projectionWindow.SetText(textStrofa);
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