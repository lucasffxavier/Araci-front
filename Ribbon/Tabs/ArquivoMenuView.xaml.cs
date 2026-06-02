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
            Context?.NovoProjeto.Executar();
        }

        private void Abrir_Click(object sender, RoutedEventArgs e)
        {
            Context?.AbrirProjeto.ExecutarComDialogo();
        }

        private void Salvar_Click(object sender, RoutedEventArgs e)
        {
            Context?.SalvarProjeto.ExecutarComDialogo();
        }
    }
}
