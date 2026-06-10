using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Araci.Core.Documents;
using Araci.Services.Catalog;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetTypeViewModel : INotifyPropertyChanged
    {
        private const double WorkspaceMargin = 360.0;
        private const double EndpointHandleSize = 12.0;

        private readonly AraciDocument _document;
        private readonly ProjectSheetType _tipo;
        private readonly TypeLibraryService _types;
        private ProjectSheetTemplateLineViewModel? _previewLine;
        private Guid? _selectedLineId;
        private Guid? _selectedRectangleId;

        public ProjectSheetTypeViewModel(AraciDocument document, ProjectSheetType tipo)
            : this(document, tipo, new TypeLibraryService())
        {
        }

        public ProjectSheetTypeViewModel(AraciDocument document, ProjectSheetType tipo, TypeLibraryService types)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _types = types ?? throw new ArgumentNullException(nameof(types));
            Lines = new ObservableCollection<ProjectSheetTemplateLineViewModel>();
            Rectangles = new ObservableCollection<ProjectSheetTemplateRectangleViewModel>();
            EndpointHandles = new ObservableCollection<ProjectSheetTemplateLineEndpointHandleViewModel>();
            _document.PropriedadesTipoPranchaAlteradas += OnPropriedadesTipoPranchaAlteradas;
            _document.ItemProjetoRenomeado += OnItemProjetoRenomeado;
            Refresh();
        }

        public Guid Id => _tipo.Id;
        public ProjectSheetType Tipo => _tipo;
        public string Nome => _tipo.Nome;
        public ProjectSheetFormat FormatoFolha => _tipo.FormatoFolha;
        public ProjectSheetOrientation OrientacaoFolha => _tipo.OrientacaoFolha;
        public double LarguraFolha => _tipo.LarguraFolha;
        public double AlturaFolha => _tipo.AlturaFolha;
        public double SheetWidth => _tipo.LarguraFolha;
        public double SheetHeight => _tipo.AlturaFolha;
        public double SheetOriginOffsetX => WorkspaceMargin;
        public double SheetOriginOffsetY => WorkspaceMargin;
        public double WorkspaceWidth => SheetWidth + WorkspaceMargin * 2;
        public double WorkspaceHeight => SheetHeight + WorkspaceMargin * 2;
        public string Titulo => $"Tipo de Prancha - {Nome}";
        public string Descricao => $"{FormatoFolha} {OrientacaoFolha} - {LarguraFolha:0.#} x {AlturaFolha:0.#}";
        public ObservableCollection<ProjectSheetTemplateLineViewModel> Lines { get; }
        public ObservableCollection<ProjectSheetTemplateRectangleViewModel> Rectangles { get; }
        public ObservableCollection<ProjectSheetTemplateLineEndpointHandleViewModel> EndpointHandles { get; }
        public Guid? SelectedLineId => _selectedLineId;
        public Guid? SelectedRectangleId => _selectedRectangleId;
        public bool HasSelectedLine => _selectedLineId.HasValue;
        public bool HasSelectedRectangle => _selectedRectangleId.HasValue;
        public bool HasTemplateSelection => HasSelectedLine || HasSelectedRectangle;
        public bool EndpointHandlesVisible => EndpointHandles.Count > 0;

        public ProjectSheetTemplateLineViewModel? PreviewLine
        {
            get => _previewLine;
            private set
            {
                if (ReferenceEquals(_previewLine, value))
                    return;

                _previewLine = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasPreviewLine));
            }
        }

        public bool HasPreviewLine => PreviewLine != null;

        public void Refresh()
        {
            OnPropertyChanged(nameof(Nome));
            OnPropertyChanged(nameof(FormatoFolha));
            OnPropertyChanged(nameof(OrientacaoFolha));
            OnPropertyChanged(nameof(LarguraFolha));
            OnPropertyChanged(nameof(AlturaFolha));
            OnPropertyChanged(nameof(SheetWidth));
            OnPropertyChanged(nameof(SheetHeight));
            OnPropertyChanged(nameof(WorkspaceWidth));
            OnPropertyChanged(nameof(WorkspaceHeight));
            OnPropertyChanged(nameof(Titulo));
            OnPropertyChanged(nameof(Descricao));
            RefreshRectangles();
            RefreshLines();
            OnPropertyChanged(nameof(HasTemplateSelection));
        }

        public void SetPreviewLine(ProjectSheetTemplateLine? linha)
        {
            PreviewLine = linha == null ? null : new ProjectSheetTemplateLineViewModel(linha, _types);
        }

        public bool TryHitLineAt(Point position, double tolerance, out Guid lineId)
        {
            ProjectSheetTemplateLineViewModel? hit = Lines
                .Reverse()
                .FirstOrDefault(l => DistanciaAoSegmento(position, new Point(l.X1, l.Y1), new Point(l.X2, l.Y2)) <= tolerance);

            lineId = hit?.Id ?? Guid.Empty;
            return hit != null;
        }

        public bool SelectLineAt(Point position, double tolerance)
        {
            if (!TryHitLineAt(position, tolerance, out Guid lineId))
            {
                ClearLineSelection();
                return false;
            }

            SelectLine(lineId);
            return true;
        }

        public bool SelectLine(Guid lineId)
        {
            if (!Lines.Any(l => l.Id == lineId))
            {
                ClearLineSelection();
                return false;
            }

            _selectedLineId = lineId;
            _selectedRectangleId = null;
            AtualizarSelecaoVisual();
            RefreshEndpointHandles();
            NotificarSelecao();
            return true;
        }

        public void ClearLineSelection()
        {
            if (!_selectedLineId.HasValue)
                return;

            _selectedLineId = null;
            AtualizarSelecaoVisual();
            RefreshEndpointHandles();
            NotificarSelecao();
        }

        public bool TryGetSelectedLineId(out Guid lineId)
        {
            if (_selectedLineId.HasValue && Lines.Any(l => l.Id == _selectedLineId.Value))
            {
                lineId = _selectedLineId.Value;
                return true;
            }

            lineId = Guid.Empty;
            return false;
        }

        public bool TryGetLine(Guid lineId, out ProjectSheetTemplateLine? linha)
        {
            linha = _tipo.Linhas.FirstOrDefault(l => l.Id == lineId);
            return linha != null;
        }

        public bool TryGetLineCoordinates(Guid lineId, out double x1, out double y1, out double x2, out double y2)
        {
            ProjectSheetTemplateLineViewModel? line = Lines.FirstOrDefault(l => l.Id == lineId);

            if (line == null)
            {
                x1 = 0.0;
                y1 = 0.0;
                x2 = 0.0;
                y2 = 0.0;
                return false;
            }

            x1 = line.ModelX1;
            y1 = line.ModelY1;
            x2 = line.ModelX2;
            y2 = line.ModelY2;
            return true;
        }

        public bool TryHitSelectedLineEndpoint(Point position, double tolerance, out Guid lineId, out ProjectSheetTemplateLineEndpoint endpoint)
        {
            lineId = Guid.Empty;
            endpoint = ProjectSheetTemplateLineEndpoint.Start;

            if (!_selectedLineId.HasValue)
                return false;

            ProjectSheetTemplateLineViewModel? line = Lines.FirstOrDefault(l => l.Id == _selectedLineId.Value && l.IsSelected);

            if (line == null)
                return false;

            double startDistance = Distancia(position, new Point(line.X1, line.Y1));
            double endDistance = Distancia(position, new Point(line.X2, line.Y2));

            if (startDistance <= tolerance && startDistance <= endDistance)
            {
                lineId = line.Id;
                endpoint = ProjectSheetTemplateLineEndpoint.Start;
                return true;
            }

            if (endDistance <= tolerance)
            {
                lineId = line.Id;
                endpoint = ProjectSheetTemplateLineEndpoint.End;
                return true;
            }

            return false;
        }

        public bool SetLinePreviewOffset(Guid lineId, double deltaX, double deltaY)
        {
            ProjectSheetTemplateLineViewModel? line = Lines.FirstOrDefault(l => l.Id == lineId);

            if (line == null)
                return false;

            line.SetPreviewOffset(deltaX, deltaY);
            RefreshEndpointHandles();
            return true;
        }

        public void ClearLinePreviewOffset(Guid lineId)
        {
            ProjectSheetTemplateLineViewModel? line = Lines.FirstOrDefault(l => l.Id == lineId);
            line?.ClearPreviewOffset();
            RefreshEndpointHandles();
        }

        public void ClearLinePreviewOffsets()
        {
            foreach (ProjectSheetTemplateLineViewModel line in Lines)
                line.ClearPreviewOffset();

            RefreshEndpointHandles();
        }

        public bool SetLineEndpointPreview(Guid lineId, ProjectSheetTemplateLineEndpoint endpoint, double x, double y)
        {
            ProjectSheetTemplateLineViewModel? line = Lines.FirstOrDefault(l => l.Id == lineId);

            if (line == null)
                return false;

            line.SetEndpointPreview(endpoint, x, y);
            RefreshEndpointHandles();
            return true;
        }

        public bool SetLinePreviewCoordinates(Guid lineId, double x1, double y1, double x2, double y2)
        {
            ProjectSheetTemplateLineViewModel? line = Lines.FirstOrDefault(l => l.Id == lineId);

            if (line == null)
                return false;

            line.SetPreviewCoordinates(x1, y1, x2, y2);
            RefreshEndpointHandles();
            return true;
        }

        public void ClearLineEndpointPreview(Guid lineId)
        {
            ProjectSheetTemplateLineViewModel? line = Lines.FirstOrDefault(l => l.Id == lineId);
            line?.ClearEndpointPreview();
            RefreshEndpointHandles();
        }

        public void ClearLinePreviewCoordinates(Guid lineId)
        {
            ClearLineEndpointPreview(lineId);
        }

        public void ClearLineEndpointPreviews()
        {
            foreach (ProjectSheetTemplateLineViewModel line in Lines)
                line.ClearEndpointPreview();

            RefreshEndpointHandles();
        }

        public bool TryHitRectangleAt(Point position, double tolerance, out Guid rectangleId)
        {
            ProjectSheetTemplateRectangleViewModel? hit = Rectangles
                .Reverse()
                .FirstOrDefault(r => r.Contains(position, tolerance));

            rectangleId = hit?.Id ?? Guid.Empty;
            return hit != null;
        }

        public bool SelectRectangleAt(Point position, double tolerance)
        {
            if (!TryHitRectangleAt(position, tolerance, out Guid rectangleId))
            {
                ClearRectangleSelection();
                return false;
            }

            SelectRectangle(rectangleId);
            return true;
        }

        public bool SelectRectangle(Guid rectangleId)
        {
            if (!Rectangles.Any(r => r.Id == rectangleId))
            {
                ClearRectangleSelection();
                return false;
            }

            _selectedRectangleId = rectangleId;
            _selectedLineId = null;
            AtualizarSelecaoVisual();
            RefreshEndpointHandles();
            NotificarSelecao();
            return true;
        }

        public void ClearRectangleSelection()
        {
            if (!_selectedRectangleId.HasValue)
                return;

            _selectedRectangleId = null;
            AtualizarSelecaoVisual();
            NotificarSelecao();
        }

        public bool TryGetSelectedRectangleId(out Guid rectangleId)
        {
            if (_selectedRectangleId.HasValue && Rectangles.Any(r => r.Id == _selectedRectangleId.Value))
            {
                rectangleId = _selectedRectangleId.Value;
                return true;
            }

            rectangleId = Guid.Empty;
            return false;
        }

        public bool TryGetRectangle(Guid rectangleId, out ProjectSheetTemplateRectangle? retangulo)
        {
            retangulo = _tipo.Retangulos.FirstOrDefault(r => r.Id == rectangleId);
            return retangulo != null;
        }

        public bool SetRectanglePreviewOffset(Guid rectangleId, double deltaX, double deltaY)
        {
            ProjectSheetTemplateRectangleViewModel? rectangle = Rectangles.FirstOrDefault(r => r.Id == rectangleId);

            if (rectangle == null)
                return false;

            rectangle.SetPreviewOffset(deltaX, deltaY);
            return true;
        }

        public void ClearRectanglePreviewOffset(Guid rectangleId)
        {
            ProjectSheetTemplateRectangleViewModel? rectangle = Rectangles.FirstOrDefault(r => r.Id == rectangleId);
            rectangle?.ClearPreviewOffset();
        }

        public void ClearRectanglePreviewOffsets()
        {
            foreach (ProjectSheetTemplateRectangleViewModel rectangle in Rectangles)
                rectangle.ClearPreviewOffset();
        }

        public void ClearTemplateSelection()
        {
            bool tinhaSelecao = _selectedLineId.HasValue || _selectedRectangleId.HasValue;
            _selectedLineId = null;
            _selectedRectangleId = null;
            AtualizarSelecaoVisual();
            RefreshEndpointHandles();

            if (tinhaSelecao)
                NotificarSelecao();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnItemProjetoRenomeado()
        {
            OnPropertyChanged(nameof(Nome));
            OnPropertyChanged(nameof(Titulo));
            OnPropertyChanged(nameof(Descricao));
        }

        private void OnPropriedadesTipoPranchaAlteradas(ProjectSheetType tipo)
        {
            if (tipo.Id != Id)
                return;

            Refresh();
        }

        private void RefreshRectangles()
        {
            Guid? selectedId = _selectedRectangleId;
            Rectangles.Clear();

            foreach (ProjectSheetTemplateRectangle retangulo in (_tipo.Retangulos ?? new()).Where(r => r != null && r.Visible))
                Rectangles.Add(new ProjectSheetTemplateRectangleViewModel(retangulo, _types));

            if (selectedId.HasValue && Rectangles.Any(r => r.Id == selectedId.Value))
                _selectedRectangleId = selectedId;
            else
                _selectedRectangleId = null;

            AtualizarSelecaoVisualRetangulos();
            OnPropertyChanged(nameof(SelectedRectangleId));
            OnPropertyChanged(nameof(HasSelectedRectangle));
        }

        private void RefreshLines()
        {
            Guid? selectedId = _selectedLineId;
            Lines.Clear();

            foreach (ProjectSheetTemplateLine linha in (_tipo.Linhas ?? new()).Where(l => l != null && l.Visible))
                Lines.Add(new ProjectSheetTemplateLineViewModel(linha, _types));

            if (selectedId.HasValue && Lines.Any(l => l.Id == selectedId.Value))
                _selectedLineId = selectedId;
            else
                _selectedLineId = null;

            AtualizarSelecaoVisualLinhas();
            RefreshEndpointHandles();
            OnPropertyChanged(nameof(SelectedLineId));
            OnPropertyChanged(nameof(HasSelectedLine));
        }

        private void AtualizarSelecaoVisual()
        {
            AtualizarSelecaoVisualLinhas();
            AtualizarSelecaoVisualRetangulos();
        }

        private void AtualizarSelecaoVisualLinhas()
        {
            foreach (ProjectSheetTemplateLineViewModel line in Lines)
                line.IsSelected = _selectedLineId.HasValue && line.Id == _selectedLineId.Value;
        }

        private void AtualizarSelecaoVisualRetangulos()
        {
            foreach (ProjectSheetTemplateRectangleViewModel rectangle in Rectangles)
                rectangle.IsSelected = _selectedRectangleId.HasValue && rectangle.Id == _selectedRectangleId.Value;
        }

        private void RefreshEndpointHandles()
        {
            EndpointHandles.Clear();

            if (_selectedLineId.HasValue)
            {
                ProjectSheetTemplateLineViewModel? line = Lines.FirstOrDefault(l => l.Id == _selectedLineId.Value && l.IsSelected);

                if (line != null)
                {
                    EndpointHandles.Add(ProjectSheetTemplateLineEndpointHandleViewModel.Create(line.Id, ProjectSheetTemplateLineEndpoint.Start, line.X1, line.Y1, EndpointHandleSize));
                    EndpointHandles.Add(ProjectSheetTemplateLineEndpointHandleViewModel.Create(line.Id, ProjectSheetTemplateLineEndpoint.End, line.X2, line.Y2, EndpointHandleSize));
                }
            }

            OnPropertyChanged(nameof(EndpointHandlesVisible));
        }

        private void NotificarSelecao()
        {
            OnPropertyChanged(nameof(SelectedLineId));
            OnPropertyChanged(nameof(SelectedRectangleId));
            OnPropertyChanged(nameof(HasSelectedLine));
            OnPropertyChanged(nameof(HasSelectedRectangle));
            OnPropertyChanged(nameof(HasTemplateSelection));
        }

        private static double DistanciaAoSegmento(Point point, Point a, Point b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double lengthSquared = dx * dx + dy * dy;

            if (lengthSquared < 0.0001)
                return Distancia(point, a);

            double t = ((point.X - a.X) * dx + (point.Y - a.Y) * dy) / lengthSquared;
            t = Math.Max(0, Math.Min(1, t));
            var projection = new Point(a.X + t * dx, a.Y + t * dy);
            return Distancia(point, projection);
        }

        private static double Distancia(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed class ProjectSheetTemplateLineEndpointHandleViewModel
    {
        private ProjectSheetTemplateLineEndpointHandleViewModel(
            Guid lineId,
            ProjectSheetTemplateLineEndpoint endpoint,
            double x,
            double y,
            double size)
        {
            LineId = lineId;
            Endpoint = endpoint;
            X = x;
            Y = y;
            Size = size;
        }

        public Guid LineId { get; }
        public ProjectSheetTemplateLineEndpoint Endpoint { get; }
        public double X { get; }
        public double Y { get; }
        public double Size { get; }
        public double Left => X - Size / 2.0;
        public double Top => Y - Size / 2.0;
        public Brush Fill => Brushes.White;
        public Brush Stroke => Brushes.DodgerBlue;
        public double StrokeThickness => 2.0;

        public static ProjectSheetTemplateLineEndpointHandleViewModel Create(
            Guid lineId,
            ProjectSheetTemplateLineEndpoint endpoint,
            double x,
            double y,
            double size)
        {
            return new ProjectSheetTemplateLineEndpointHandleViewModel(lineId, endpoint, x, y, size);
        }
    }
}