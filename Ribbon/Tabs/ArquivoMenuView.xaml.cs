using System.Windows;
using System.Windows.Controls;
using Araci.Services;

namespace Araci.Ribbon.Tabs
{
    public partial class ArquivoMenuView : UserControl
    {
        public ArquivoMenuView()
        {
            InitializeComponent();
        }

        private EditorContext? Context =>
            DataContext as EditorContext ??
            Window.GetWindow(this)?.DataContext as EditorContext;

        private void Novo_Click(object sender, RoutedEventArgs e)
        {
            Context?.Projects.Novo();
        }

        private void Abrir_Click(object sender, RoutedEventArgs e)
        {
            Context?.Projects.AbrirComDialogo();
        }

        private void Salvar_Click(object sender, RoutedEventArgs e)
        {
            Context?.Projects.SalvarComDialogo();
        }
    }
}
