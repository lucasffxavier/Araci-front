using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using Araci.ViewModels;

namespace Araci.Views
{
    public partial class ProjectTableGridView : UserControl
    {
        private ProjectTableDataViewModel? _viewModel;

        public ProjectTableGridView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (_viewModel != null)
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

            _viewModel = e.NewValue as ProjectTableDataViewModel;

            if (_viewModel != null)
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            AtualizarColunas();
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(ProjectTableDataViewModel.Columns))
                AtualizarColunas();
        }

        private void AtualizarColunas()
        {
            TableDataGrid.Columns.Clear();

            if (_viewModel == null)
                return;

            for (int i = 0; i < _viewModel.Columns.Count; i++)
            {
                TableDataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = _viewModel.Columns[i].NomeExibicao,
                    Binding = new Binding($"[{i}]"),
                    IsReadOnly = true
                });
            }
        }
    }
}
