using Araci.Services;
using Araci.Properties;
using Araci.ViewModels;
using Araci.Applications.Abstractions;
using Araci.Applications.Projects.Tables;
using Araci.Core.Documents;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Araci
{
    public partial class MainWindow : Window
    {
        private readonly EditorContext _context;
        private readonly GridLength _projectBrowserColumnWidth = new(260);
        private readonly GridLength _propertiesColumnWidth = new(320);
        private ProjectTableDataViewModel? _projectTableDataViewModel;
        private ProjectSheetViewModel? _projectSheetViewModel;

        public MainWindow()
        {
            InitializeComponent();
            _context = new EditorContext();
            UnitValueConverter.CurrentUnits = _context.Settings.Units;
            ProjectBrowser.DataContext = new ProjectBrowserViewModel(_context.Document, _context.DefinirVistaAtiva, _context.RenomearItemProjeto, _context.ExcluirItemProjeto, _context.DuplicarItemProjeto, MostrarTabela, MostrarPrancha, MostrarPropriedadesVista, MostrarPropriedadesTabela, MostrarPropriedadesPrancha);
            _context.Editor.PropertyChanged += OnEditorStatePropertyChanged;
            _context.Document.PropriedadesTabelaAlteradas += OnPropriedadesTabelaAlteradas;
            _context.Document.ItemProjetoRenomeado += OnItemProjetoRenomeado;
            _context.Document.Tabelas.CollectionChanged += OnTabelasCollectionChanged;
            _context.Document.Pranchas.CollectionChanged += OnPranchasCollectionChanged;
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
            ProjectView? vista = _context.Document.Vistas.FirstOrDefault(v => v.Id == vistaId);

            if (vista == null)
                return;

            MostrarViewport();

            _context.Editor.ElementoSelecionado = new ProjectViewPropertiesViewModel(
                _context.Document,
                vista,
                _context.RenomearItemProjeto,
                _context.EditarPropriedadesVista);

            MostrarPropriedades();
        }

        public void MostrarPropriedadesTabela(System.Guid tabelaId)
        {
            ProjectTable? tabela = _context.Document.Tabelas.FirstOrDefault(t => t.Id == tabelaId);

            if (tabela == null)
                return;

            _context.Editor.ElementoSelecionado = new ProjectTablePropertiesViewModel(
                _context.Document,
                tabela,
                _context.RenomearItemProjeto,
                _context.EditarPropriedadesTabela,
                _context.ExportarTabela,
                _context.Dialogs);

            MostrarPropriedades();
        }

        public void MostrarPropriedadesPrancha(System.Guid pranchaId)
        {
            ProjectSheet? prancha = _context.Document.Pranchas.FirstOrDefault(p => p.Id == pranchaId);

            if (prancha == null)
                return;

            _context.Editor.ElementoSelecionado = new ProjectSheetPropertiesViewModel(
                _context.Document,
                prancha,
                _context.RenomearItemProjeto,
                _context.EditarPropriedadesPrancha);

            MostrarPropriedades();
        }

        public void MostrarTabela(System.Guid tabelaId)
        {
            ProjectTable? tabela = _context.Document.Tabelas.FirstOrDefault(t => t.Id == tabelaId);

            if (tabela == null)
                return;

            _projectTableDataViewModel = new ProjectTableDataViewModel(
                _context.Document,
                tabela,
                new ProjectTableDataBuilder());

            ProjectTableGrid.DataContext = _projectTableDataViewModel;
            ProjectTableGrid.Visibility = Visibility.Visible;
            ProjectSheetViewer.Visibility = Visibility.Collapsed;
            ProjectSheetViewer.DataContext = null;
            _projectSheetViewModel = null;
            Viewport.Visibility = Visibility.Collapsed;
        }

        public void MostrarPrancha(System.Guid pranchaId)
        {
            ProjectSheet? prancha = _context.Document.Pranchas.FirstOrDefault(p => p.Id == pranchaId);

            if (prancha == null)
                return;

            _projectSheetViewModel = new ProjectSheetViewModel(_context.Document, prancha, _context.MoverTabelaNaPrancha, _context.RedimensionarTabelaNaPrancha, _context.RemoverTabelaDaPrancha, _context.DividirTabelaNaPrancha);

            ProjectSheetViewer.DataContext = _projectSheetViewModel;
            ProjectSheetViewer.Visibility = Visibility.Visible;
            ProjectTableGrid.Visibility = Visibility.Collapsed;
            ProjectTableGrid.DataContext = null;
            _projectTableDataViewModel = null;
            Viewport.Visibility = Visibility.Collapsed;
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

        public void ExportarTabelaAtualCsv()
        {
            ProjectTable? tabela = _projectTableDataViewModel == null
                ? null
                : _context.Document.Tabelas.FirstOrDefault(t => t.Id == _projectTableDataViewModel.TableId);

            _context.ExportarTabela.Executar(tabela);
        }

        public void InserirTabelaNaPrancha()
        {
            if (_context.Document.Pranchas.Count == 0)
            {
                _context.Dialogs.ShowWarning("Inserir tabela na prancha", "Crie uma prancha antes de inserir uma tabela.");
                return;
            }

            if (_context.Document.Tabelas.Count == 0)
            {
                _context.Dialogs.ShowWarning("Inserir tabela na prancha", "Crie uma tabela antes de inseri-la em uma prancha.");
                return;
            }

            List<ProjectItemDialogOption> pranchas = _context.Document.Pranchas
                .Select(p => new ProjectItemDialogOption(p.Id, string.IsNullOrWhiteSpace(p.Numero) ? p.Nome : $"{p.Numero} - {p.Nome}"))
                .ToList();
            List<ProjectItemDialogOption> tabelas = _context.Document.Tabelas
                .Select(t => new ProjectItemDialogOption(t.Id, t.Nome))
                .ToList();

            InserirTabelaPranchaDialogResult? result = _context.Dialogs.ShowInserirTabelaPranchaDialog(pranchas, tabelas);

            if (result == null || result.TableIds.Count == 0)
                return;

            IReadOnlyList<ProjectSheetTableInstance> instances = _context.InserirTabelaNaPrancha.InserirMultiplas(result.SheetId, result.TableIds, AtualizarPranchaAtual);

            if (instances.Count == 0)
            {
                _context.Dialogs.ShowWarning("Inserir tabela na prancha", "Nao foi possivel inserir as tabelas na prancha selecionada.");
                return;
            }

            if (_projectSheetViewModel?.SheetId == result.SheetId)
                _projectSheetViewModel.Refresh();

            _context.Dialogs.ShowInfo("Inserir tabela na prancha", instances.Count == 1 ? "Tabela inserida na prancha." : "Tabelas inseridas na prancha.");
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


        private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled)
                return;

            if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                return;

            if (e.Key != Key.Z && e.Key != Key.Y)
                return;

            if (Keyboard.FocusedElement is TextBoxBase)
                return;

            if (_context.Input.KeyDown(e.Key))
                e.Handled = true;
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

        private void OnPropriedadesTabelaAlteradas(ProjectTable tabela)
        {
            if (_projectTableDataViewModel?.TableId == tabela.Id)
                _projectTableDataViewModel.Refresh();

            if (_projectSheetViewModel != null)
                _projectSheetViewModel.Refresh();
        }

        private void OnItemProjetoRenomeado()
        {
            _projectSheetViewModel?.Refresh();
        }

        private void OnTabelasCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_projectTableDataViewModel == null ||
                _context.Document.Tabelas.Any(t => t.Id == _projectTableDataViewModel.TableId))
            {
                _projectSheetViewModel?.Refresh();
                return;
            }

            MostrarViewport();
        }

        private void OnPranchasCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_projectSheetViewModel == null ||
                _context.Document.Pranchas.Any(p => p.Id == _projectSheetViewModel.SheetId))
                return;

            MostrarViewport();
        }

        private void OnEditorStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(EditorState.NavegadorProjetoVisivel))
                AtualizarVisibilidadeNavegadorProjeto();
        }

        private void AtualizarPranchaAtual()
        {
            _projectSheetViewModel?.Refresh();
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

        private void MostrarViewport()
        {
            ProjectTableGrid.Visibility = Visibility.Collapsed;
            ProjectTableGrid.DataContext = null;
            _projectTableDataViewModel = null;
            ProjectSheetViewer.Visibility = Visibility.Collapsed;
            ProjectSheetViewer.DataContext = null;
            _projectSheetViewModel = null;
            Viewport.Visibility = Visibility.Visible;
            FocarViewport();
        }
    }
}
