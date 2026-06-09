using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Araci.Applications.Projects.Tables;
using Araci.Applications.UseCases.Projeto;
using Araci.Core.Documents;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetViewModel : INotifyPropertyChanged
    {
        private const double WorkspaceMargin = 1600.0;
        private const double MinZoomScale = 0.25;
        private const double MaxZoomScale = 4.0;
        private const double ZoomStep = 0.10;

        private readonly AraciDocument _document;
        private readonly ProjectSheet _sheet;
        private readonly MoverTabelaNaPranchaUseCase? _moverTabelaNaPrancha;
        private readonly RedimensionarTabelaNaPranchaUseCase? _redimensionarTabelaNaPrancha;
        private readonly RemoverTabelaDaPranchaUseCase? _removerTabelaDaPrancha;
        private readonly ProjectTableDataBuilder _tableDataBuilder = new();
        private string _titulo = string.Empty;
        private string _emptyMessage = string.Empty;
        private Guid? _selectedInstanceId;
        private double _workspaceWidth = MoverTabelaNaPranchaUseCase.LarguraPadraoPrancha + WorkspaceMargin * 2;
        private double _workspaceHeight = MoverTabelaNaPranchaUseCase.AlturaPadraoPrancha + WorkspaceMargin * 2;
        private double _zoomScale = 1.0;

        public ProjectSheetViewModel(
            AraciDocument document,
            ProjectSheet sheet,
            MoverTabelaNaPranchaUseCase? moverTabelaNaPrancha = null,
            RedimensionarTabelaNaPranchaUseCase? redimensionarTabelaNaPrancha = null,
            RemoverTabelaDaPranchaUseCase? removerTabelaDaPrancha = null)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            _moverTabelaNaPrancha = moverTabelaNaPrancha;
            _redimensionarTabelaNaPrancha = redimensionarTabelaNaPrancha;
            _removerTabelaDaPrancha = removerTabelaDaPrancha;
            TableInstances = new ObservableCollection<ProjectSheetTableInstanceViewModel>();
            Refresh();
        }

        public Guid SheetId => _sheet.Id;
        public ObservableCollection<ProjectSheetTableInstanceViewModel> TableInstances { get; }
        public bool HasInstances => TableInstances.Count > 0;
        public bool HasEmptyMessage => !string.IsNullOrWhiteSpace(EmptyMessage);
        public Guid? SelectedInstanceId => _selectedInstanceId;
        public bool HasSelectedInstance => _selectedInstanceId.HasValue;
        public double SheetWidth => MoverTabelaNaPranchaUseCase.LarguraPadraoPrancha;
        public double SheetHeight => MoverTabelaNaPranchaUseCase.AlturaPadraoPrancha;
        public double SheetOriginOffsetX => WorkspaceMargin;
        public double SheetOriginOffsetY => WorkspaceMargin;
        public double MinimumWorkspaceWidth => SheetWidth + WorkspaceMargin * 2;
        public double MinimumWorkspaceHeight => SheetHeight + WorkspaceMargin * 2;

        public double WorkspaceWidth
        {
            get => _workspaceWidth;
            private set
            {
                double normalized = Math.Max(MinimumWorkspaceWidth, NormalizeWorkspaceDimension(value, MinimumWorkspaceWidth));

                if (Math.Abs(_workspaceWidth - normalized) < 0.000001)
                    return;

                _workspaceWidth = normalized;
                OnPropertyChanged();
            }
        }

        public double WorkspaceHeight
        {
            get => _workspaceHeight;
            private set
            {
                double normalized = Math.Max(MinimumWorkspaceHeight, NormalizeWorkspaceDimension(value, MinimumWorkspaceHeight));

                if (Math.Abs(_workspaceHeight - normalized) < 0.000001)
                    return;

                _workspaceHeight = normalized;
                OnPropertyChanged();
            }
        }

        public double ZoomScale
        {
            get => _zoomScale;
            private set
            {
                double normalized = NormalizeZoom(value);

                if (Math.Abs(_zoomScale - normalized) < 0.000001)
                    return;

                _zoomScale = normalized;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ZoomPercentText));
            }
        }

        public string ZoomPercentText => $"{Math.Round(ZoomScale * 100):0}%";

        public string Titulo
        {
            get => _titulo;
            private set
            {
                if (_titulo == value)
                    return;

                _titulo = value;
                OnPropertyChanged();
            }
        }

        public string EmptyMessage
        {
            get => _emptyMessage;
            private set
            {
                if (_emptyMessage == value)
                    return;

                _emptyMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasEmptyMessage));
            }
        }

        public void Refresh()
        {
            Guid? selectedBeforeRefresh = _selectedInstanceId;
            Titulo = string.IsNullOrWhiteSpace(_sheet.Numero)
                ? _sheet.Nome
                : $"{_sheet.Numero} - {_sheet.Nome}";

            TableInstances.Clear();
            NormalizeInstances();
            RecalculateWorkspaceFromDocument();

            foreach (ProjectSheetTableInstance instance in _sheet.Tabelas.Where(i => i != null))
            {
                ProjectTable? table = _document.Tabelas.FirstOrDefault(t => t.Id == instance.TableId);
                ProjectTableDataResult? tableData = table == null
                    ? null
                    : _tableDataBuilder.Build(_document, table);

                var instanceViewModel = new ProjectSheetTableInstanceViewModel(
                    instance,
                    table?.Nome ?? "Tabela nao encontrada",
                    tableData)
                {
                    IsSelected = selectedBeforeRefresh == instance.Id
                };
                instanceViewModel.SetSheetOriginOffset(SheetOriginOffsetX, SheetOriginOffsetY);
                TableInstances.Add(instanceViewModel);
            }

            if (selectedBeforeRefresh.HasValue && !TableInstances.Any(i => i.Id == selectedBeforeRefresh.Value))
                _selectedInstanceId = null;

            EmptyMessage = HasInstances ? string.Empty : "Nenhuma tabela inserida na prancha";
            OnPropertyChanged(nameof(HasInstances));
            OnSelectionChanged();
        }

        public void SelecionarInstancia(Guid instanceId)
        {
            bool encontrou = false;

            foreach (ProjectSheetTableInstanceViewModel instance in TableInstances)
            {
                bool selecionada = instance.Id == instanceId;
                instance.IsSelected = selecionada;
                encontrou |= selecionada;
            }

            _selectedInstanceId = encontrou ? instanceId : null;
            OnSelectionChanged();
        }

        public void LimparSelecao()
        {
            foreach (ProjectSheetTableInstanceViewModel instance in TableInstances)
                instance.IsSelected = false;

            _selectedInstanceId = null;
            OnSelectionChanged();
        }

        public void SetPreviewPosition(ProjectSheetTableInstanceViewModel instance, double x, double y)
        {
            ArgumentNullException.ThrowIfNull(instance);

            instance.SetPreviewPosition(
                MoverTabelaNaPranchaUseCase.NormalizePosition(x),
                MoverTabelaNaPranchaUseCase.NormalizePosition(y));
        }

        public void SetPreviewSize(ProjectSheetTableInstanceViewModel instance, double width, double height)
        {
            ArgumentNullException.ThrowIfNull(instance);

            instance.SetPreviewSize(
                RedimensionarTabelaNaPranchaUseCase.NormalizeDimension(width, ProjectSheetTableInstance.MinWidth),
                RedimensionarTabelaNaPranchaUseCase.NormalizeDimension(height, ProjectSheetTableInstance.MinHeight));
        }

        public bool MoverInstancia(Guid instanceId, double novoX, double novoY)
        {
            ProjectSheetTableInstanceViewModel? instanceViewModel = TableInstances.FirstOrDefault(i => i.Id == instanceId);

            if (instanceViewModel == null)
                return false;

            bool moved = _moverTabelaNaPrancha?.Mover(
                SheetId,
                instanceId,
                novoX,
                novoY,
                SheetWidth,
                SheetHeight,
                Refresh) == true;

            if (moved)
                SelecionarInstancia(instanceId);
            else
                RestoreWorkspaceFromDocument();

            return moved;
        }

        public bool RedimensionarInstancia(Guid instanceId, double novaLargura, double novaAltura)
        {
            ProjectSheetTableInstanceViewModel? instanceViewModel = TableInstances.FirstOrDefault(i => i.Id == instanceId);

            if (instanceViewModel == null)
                return false;

            bool resized = _redimensionarTabelaNaPrancha?.Redimensionar(
                SheetId,
                instanceId,
                novaLargura,
                novaAltura,
                SheetWidth,
                SheetHeight,
                Refresh) == true;

            if (resized)
                SelecionarInstancia(instanceId);
            else
                RestoreWorkspaceFromDocument();

            return resized;
        }

        public bool RemoverInstanciaSelecionada()
        {
            if (!_selectedInstanceId.HasValue)
                return false;

            Guid instanceId = _selectedInstanceId.Value;
            bool removed = _removerTabelaDaPrancha?.Remover(SheetId, instanceId, Refresh) == true;

            if (removed)
            {
                _selectedInstanceId = null;
                Refresh();
            }

            return removed;
        }

        public void ZoomIn()
        {
            ZoomScale += ZoomStep;
        }

        public void ZoomOut()
        {
            ZoomScale -= ZoomStep;
        }

        public void ResetZoom()
        {
            ZoomScale = 1.0;
        }

        public void RestoreWorkspaceFromDocument()
        {
            RecalculateWorkspaceFromDocument();
            UpdateInstanceOffsets();
        }

        private void NormalizeInstances()
        {
            foreach (ProjectSheetTableInstance instance in _sheet.Tabelas.Where(i => i != null))
                NormalizeInstance(instance);
        }

        private void RecalculateWorkspaceFromDocument()
        {
            RecalculateWorkspace(_sheet.Tabelas.Where(i => i != null).Select(i => new WorkspaceBoundsItem(i.X, i.Y, i.Width, i.Height)));
        }

        private void RecalculateWorkspace(IEnumerable<WorkspaceBoundsItem> items)
        {
            double maxX = SheetWidth;
            double maxY = SheetHeight;

            foreach (WorkspaceBoundsItem item in items)
            {
                double x = MoverTabelaNaPranchaUseCase.NormalizePosition(item.X);
                double y = MoverTabelaNaPranchaUseCase.NormalizePosition(item.Y);
                double width = RedimensionarTabelaNaPranchaUseCase.NormalizeDimension(item.Width, ProjectSheetTableInstance.MinWidth);
                double height = RedimensionarTabelaNaPranchaUseCase.NormalizeDimension(item.Height, ProjectSheetTableInstance.MinHeight);

                maxX = Math.Max(maxX, x + width);
                maxY = Math.Max(maxY, y + height);
            }

            WorkspaceWidth = SheetOriginOffsetX + maxX + WorkspaceMargin;
            WorkspaceHeight = SheetOriginOffsetY + maxY + WorkspaceMargin;
            UpdateInstanceOffsets();
        }

        private void UpdateInstanceOffsets()
        {
            foreach (ProjectSheetTableInstanceViewModel instance in TableInstances)
                instance.SetSheetOriginOffset(SheetOriginOffsetX, SheetOriginOffsetY);
        }

        private static void NormalizeInstance(ProjectSheetTableInstance instance)
        {
            instance.X = MoverTabelaNaPranchaUseCase.NormalizePosition(instance.X);
            instance.Y = MoverTabelaNaPranchaUseCase.NormalizePosition(instance.Y);
            instance.Width = RedimensionarTabelaNaPranchaUseCase.NormalizeDimension(instance.Width, ProjectSheetTableInstance.MinWidth);
            instance.Height = RedimensionarTabelaNaPranchaUseCase.NormalizeDimension(instance.Height, ProjectSheetTableInstance.MinHeight);
        }

        private static double NormalizeWorkspaceDimension(double value, double fallback)
        {
            return double.IsNaN(value) || double.IsInfinity(value) || value <= 0
                ? fallback
                : value;
        }

        private static double NormalizeZoom(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return 1.0;

            if (value < MinZoomScale)
                return MinZoomScale;

            if (value > MaxZoomScale)
                return MaxZoomScale;

            return value;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnSelectionChanged()
        {
            OnPropertyChanged(nameof(SelectedInstanceId));
            OnPropertyChanged(nameof(HasSelectedInstance));
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly struct WorkspaceBoundsItem
        {
            public WorkspaceBoundsItem(double x, double y, double width, double height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }

            public double X { get; }
            public double Y { get; }
            public double Width { get; }
            public double Height { get; }
        }
    }
}
