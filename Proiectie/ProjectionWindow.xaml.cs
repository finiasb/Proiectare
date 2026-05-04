using System.Windows;

namespace Proiectie
{
    public partial class ProjectionWindow : Window
    {
        public ProjectionWindow()
        {
            InitializeComponent();
        }
        public void SetText(string textCurent)
        {
            Dispatcher.Invoke(() => {
                txtMain.Text = textCurent;
            });
        }
    }
}
