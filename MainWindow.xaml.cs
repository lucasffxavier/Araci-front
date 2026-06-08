using Araci.Services;
using Araci.Properties;
using Araci.ViewModels;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Araci
{
    public partial class MainWindow : Window
    {
        private readonly EditorContext _context;
        private readonly GridLength _projectBrowserColumnWidth = new(260);
        private readonly GridLength _propertiesColumnWidth = new(320);

        public MainWindow()
        {
            InitializeComponent();
            _context = new EditorContext();
            UnitValueConverter.CurrentUnits = _context.Settings.Units;
            ProjectBrowser.DataContext = new ProjectBrowserViewModel(_context.Document, _context.DefinirVistaAtiva, _context.RenomearItemProjeto, _context.ExcluirItemProjeto, _context.DuplicarItemProjeto, MostrarPropriedadesVista, MostrarPropriedadesTabela);
            _context.Editor.PropertyChanged += OnEditorStatePropertyChanged;
            Viewport.Inicializar(_context);
            InicializarRibbon();
            AtualizarVisibilidadeNavegadorProjeto();
        }

        private void InicializarRibbon()
        {
            DataContext = _context;
            ArquivoMenu.DataContext = _context;
        }

        public void ToggleArquivoMenu(UIElement referencia)
        {
            if (ArquivoPopup.IsOpen)
            {
                ArquivoPopup.IsOpen = false;
                return;
            }

            ArquivoPopup.PlacementTarget = referencia;
            ArquivoPopup.Placement = PlacementMode.Bottom;
            ArquivoPopup.IsOpen = true;
        }

        public void FocarViewport()
        {
            Viewport.Focus();
            System.Windows.Input.Keyboard.Focus(Viewport);
        }

        public void MostrarPropriedades()
        {
            PropertiesColumn.Width = _propertiesColumnWidth;
            PropertiesHost.Visibility = Visibility.Visible;
        }

        public void MostrarPropriedadesVista(System.Guid vistaId)
        {
            Core.Documents.ProjectView? vista = _context.Document.Vistas.FirstOrDefault(v => v.Id == vistaId);

            if (vista == null)
                return;

            _context.Editor.ElementoSelecionado = new ProjectViewPropertiesViewModel(
                _context.Document,
                vista,
                _context.RenomearItemProjeto,
                _context.EditarPropriedadesVista);

            MostrarPropriedades();
        }

        public void MostrarPropriedadesTabela(System.Guid tabelaId)
        {
            Core.Documents.ProjectTable? tabela = _context.Document.Tabelas.FirstOrDefault(t => t.Id == tabelaId);

            if (tabela == null)
                return;

            _context.Editor.ElementoSelecionado = new ProjectTablePropertiesViewModel(
                _context.Document,
                tabela,
                _context.RenomearItemProjeto,
                _context.EditarPropriedadesTabela,
                _context.Dialogs);

            MostrarPropriedades();
        }

        public void AlternarNavegadorProjeto()
        {
            _context.Editor.NavegadorProjetoVisivel = !_context.Editor.NavegadorProjetoVisivel;
        }

        public void CriarNovaVistaProjeto()
        {
            _context.CriarItemProjeto.CriarVista();
            MostrarNavegadorProjeto();
        }

        public void CriarNovaTabelaProjeto()
        {
            _context.CriarItemProjeto.CriarTabela();
            MostrarNavegadorProjeto();
        }

        public void CriarNovaPranchaProjeto()
        {
            _context.CriarItemProjeto.CriarPrancha();
            MostrarNavegadorProjeto();
        }

        public void MostrarConfiguracaoUnidades()
        {
            var viewModel = new UnitsSettingsViewModel(_context.Settings.Units);
            var window = new UnitsWindow(viewModel)
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
                _context.AlterarUnidadesProjeto.Executar(viewModel.ToUnitDisplaySettings());

            FocarViewport();
        }

        private void OnPropertiesCloseRequested(object sender, RoutedEventArgs e)
        {
            PropertiesColumn.Width = new GridLength(0);
            PropertiesHost.Visibility = Visibility.Collapsed;
            FocarViewport();
        }

        private void OnProjectBrowserCloseRequested(object sender, RoutedEventArgs e)
        {
            _context.Editor.NavegadorProjetoVisivel = false;
            FocarViewport();
        }

        private void OnEditorStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(EditorState.NavegadorProjetoVisivel))
                AtualizarVisibilidadeNavegadorProjeto();
        }

        private void AtualizarVisibilidadeNavegadorProjeto()
        {
            if (_context.Editor.NavegadorProjetoVisivel)
            {
                ProjectBrowserColumn.Width = _projectBrowserColumnWidth;
                ProjectBrowser.Visibility = Visibility.Visible;
                return;
            }

            ProjectBrowserColumn.Width = new GridLength(0);
            ProjectBrowser.Visibility = Visibility.Collapsed;
        }

        private void MostrarNavegadorProjeto()
        {
            _context.Editor.NavegadorProjetoVisivel = true;
        }
    }
}
