using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Views
{
    public partial class ProjectSheetTypeView : UserControl
    {
        private enum LinhaTemplateInteractionMode
        {
            None,
            Line,
            Endpoint
        }

        private const double LineHitTolerance = 6.0;
        private const double EndpointHitTolerance = 8.0;
        private const double DragThresholdSquared = 9.0;

        private EditorContext? _context;
        private LinhaTemplateInteractionMode _linhaTemplateInteractionMode;
        private Guid? _linhaTemplateEmArrasteId;
        private Point _linhaTemplateDragStart;
        private bool _linhaTemplateArrastando;
        private Guid? _linhaTemplateEndpointEmArrasteId;
        private ProjectSheetTemplateLineEndpoint _linhaTemplateEndpointEmArraste;
        private Point _linhaTemplateEndpointDragStart;
        private bool _linhaTemplateEndpointArrastando;
        private double _linhaTemplateEndpointOriginalX1;
        private double _linhaTemplateEndpointOriginalY1;
        private double _linhaTemplateEndpointOriginalX2;
        private double _linhaTemplateEndpointOriginalY2;

        public ProjectSheetTypeView()
        {
            InitializeComponent();
            DataContext = null;
            DataContextChanged += OnDataContextChanged;
        }

        public void Inicializar(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            if (DataContext is ProjectSheetTypeViewModel viewModel)
                _context.ProjectSheetTypeViewModelAtivo = viewModel;
        }

        private void TemplatePageBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_context == null)
                return;

            Focus();
            Keyboard.Focus(this);
            Point position = e.GetPosition(TemplatePageBorder);

            if (TentarIniciarEdicaoExtremidadeLinhaTemplate(position))
            {
                e.Handled = true;
                return;
            }

            if (TentarIniciarInteracaoLinhaTemplate(position))
            {
                e.Handled = true;
                return;
            }

            ToolInputState inputState = CriarInputState(e, position, e.ChangedButton, e.ClickCount);
            _context.Tools.FerramentaAtual.OnMouseDown(null, position, inputState);
            TemplatePageBorder.CaptureMouse();
            AtualizarHandlesOverlay();
            e.Handled = true;
        }

        private void TemplatePageBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (_context == null)
                return;

            Point position = e.GetPosition(TemplatePageBorder);

            if (AtualizarEdicaoExtremidadeLinhaTemplate(position, e))
            {
                e.Handled = true;
                return;
            }

            if (AtualizarArrasteLinhaTemplate(position, e))
            {
                e.Handled = true;
                return;
            }

            _context.Tools.FerramentaAtual.OnMouseMove(position, CriarInputState(e, position));
        }

        private void TemplatePageBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_context == null)
                return;

            Point position = e.GetPosition(TemplatePageBorder);

            if (FinalizarEdicaoExtremidadeLinhaTemplate(position))
            {
                if (TemplatePageBorder.IsMouseCaptured)
                    TemplatePageBorder.ReleaseMouseCapture();

                AtualizarHandlesOverlay();
                e.Handled = true;
                return;
            }

            if (FinalizarArrasteLinhaTemplate(position))
            {
                if (TemplatePageBorder.IsMouseCaptured)
                    TemplatePageBorder.ReleaseMouseCapture();

                AtualizarHandlesOverlay();
                e.Handled = true;
                return;
            }

            _context.Tools.FerramentaAtual.OnMouseUp(position, CriarInputState(e, position, e.ChangedButton, e.ClickCount));

            if (TemplatePageBorder.IsMouseCaptured)
                TemplatePageBorder.ReleaseMouseCapture();

            AtualizarHandlesOverlay();
            e.Handled = true;
        }

        private void TemplatePageBorder_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (_linhaTemplateEndpointEmArrasteId.HasValue)
                CancelarEdicaoExtremidadeLinhaTemplate();

            if (_linhaTemplateEmArrasteId.HasValue)
                CancelarArrasteLinhaTemplate();

            AtualizarHandlesOverlay();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_context == null)
                return;

            if (e.Key == Key.Escape && _linhaTemplateEndpointEmArrasteId.HasValue)
            {
                CancelarEdicaoExtremidadeLinhaTemplate();
                AtualizarHandlesOverlay();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape && _linhaTemplateEmArrasteId.HasValue)
            {
                CancelarArrasteLinhaTemplate();
                AtualizarHandlesOverlay();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Delete && ExcluirLinhaSelecionada())
            {
                AtualizarHandlesOverlay();
                e.Handled = true;
                return;
            }

            if (_context.Input.KeyDown(e.Key))
                e.Handled = true;
        }

        private bool TentarIniciarEdicaoExtremidadeLinhaTemplate(Point position)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null)
                return false;

            ITool ferramentaAtual = _context.Tools.FerramentaAtual;

            if (!string.Equals(ferramentaAtual.Nome, "Selecionar", StringComparison.OrdinalIgnoreCase) || ferramentaAtual.IsBusy)
                return false;

            if (!viewModel.TryHitSelectedLineEndpoint(position, EndpointHitTolerance, out Guid lineId, out ProjectSheetTemplateLineEndpoint endpoint))
                return false;

            if (!viewModel.TryGetLineCoordinates(
                    lineId,
                    out _linhaTemplateEndpointOriginalX1,
                    out _linhaTemplateEndpointOriginalY1,
                    out _linhaTemplateEndpointOriginalX2,
                    out _linhaTemplateEndpointOriginalY2))
                return false;

            _linhaTemplateEndpointEmArrasteId = lineId;
            _linhaTemplateEndpointEmArraste = endpoint;
            _linhaTemplateEndpointDragStart = position;
            _linhaTemplateEndpointArrastando = false;
            _linhaTemplateInteractionMode = LinhaTemplateInteractionMode.Endpoint;
            _linhaTemplateEmArrasteId = null;
            _linhaTemplateArrastando = false;
            TemplatePageBorder.CaptureMouse();
            AtualizarHandlesOverlay();
            return true;
        }

        private bool AtualizarEdicaoExtremidadeLinhaTemplate(Point position, MouseEventArgs e)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel == null || !_linhaTemplateEndpointEmArrasteId.HasValue)
                return false;

            if (e.LeftButton != MouseButtonState.Pressed)
                return false;

            Vector delta = position - _linhaTemplateEndpointDragStart;

            if (!_linhaTemplateEndpointArrastando && delta.LengthSquared < DragThresholdSquared)
                return true;

            _linhaTemplateEndpointArrastando = true;

            double previewX1 = _linhaTemplateEndpointEmArraste == ProjectSheetTemplateLineEndpoint.Start
                ? position.X
                : _linhaTemplateEndpointOriginalX1;
            double previewY1 = _linhaTemplateEndpointEmArraste == ProjectSheetTemplateLineEndpoint.Start
                ? position.Y
                : _linhaTemplateEndpointOriginalY1;
            double previewX2 = _linhaTemplateEndpointEmArraste == ProjectSheetTemplateLineEndpoint.End
                ? position.X
                : _linhaTemplateEndpointOriginalX2;
            double previewY2 = _linhaTemplateEndpointEmArraste == ProjectSheetTemplateLineEndpoint.End
                ? position.Y
                : _linhaTemplateEndpointOriginalY2;

            viewModel.SetLinePreviewCoordinates(
                _linhaTemplateEndpointEmArrasteId.Value,
                previewX1,
                previewY1,
                previewX2,
                previewY2);

            AtualizarHandlesOverlay();
            return true;
        }

        private bool FinalizarEdicaoExtremidadeLinhaTemplate(Point position)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null || !_linhaTemplateEndpointEmArrasteId.HasValue)
                return false;

            Guid lineId = _linhaTemplateEndpointEmArrasteId.Value;
            bool arrastou = _linhaTemplateEndpointArrastando;
            ProjectSheetTemplateLineEndpoint endpoint = _linhaTemplateEndpointEmArraste;

            viewModel.ClearLinePreviewCoordinates(lineId);
            LimparEstadoEdicaoExtremidadeLinhaTemplate();

            if (!arrastou)
            {
                AtualizarHandlesOverlay();
                return true;
            }

            double newX1 = endpoint == ProjectSheetTemplateLineEndpoint.Start ? position.X : _linhaTemplateEndpointOriginalX1;
            double newY1 = endpoint == ProjectSheetTemplateLineEndpoint.Start ? position.Y : _linhaTemplateEndpointOriginalY1;
            double newX2 = endpoint == ProjectSheetTemplateLineEndpoint.End ? position.X : _linhaTemplateEndpointOriginalX2;
            double newY2 = endpoint == ProjectSheetTemplateLineEndpoint.End ? position.Y : _linhaTemplateEndpointOriginalY2;

            _context.MoverLinhaDoTipoPrancha.AlterarCoordenadas(viewModel.Id, lineId, newX1, newY1, newX2, newY2);
            viewModel.SelectLine(lineId);
            AtualizarHandlesOverlay();
            return true;
        }

        private void CancelarEdicaoExtremidadeLinhaTemplate()
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel != null && _linhaTemplateEndpointEmArrasteId.HasValue)
                viewModel.ClearLinePreviewCoordinates(_linhaTemplateEndpointEmArrasteId.Value);

            LimparEstadoEdicaoExtremidadeLinhaTemplate();
            AtualizarHandlesOverlay();
        }

        private void LimparEstadoEdicaoExtremidadeLinhaTemplate()
        {
            _linhaTemplateEndpointEmArrasteId = null;
            _linhaTemplateEndpointArrastando = false;
            _linhaTemplateEndpointOriginalX1 = 0.0;
            _linhaTemplateEndpointOriginalY1 = 0.0;
            _linhaTemplateEndpointOriginalX2 = 0.0;
            _linhaTemplateEndpointOriginalY2 = 0.0;
            _linhaTemplateInteractionMode = LinhaTemplateInteractionMode.None;
        }

        private bool TentarIniciarInteracaoLinhaTemplate(Point position)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null)
                return false;

            ITool ferramentaAtual = _context.Tools.FerramentaAtual;

            if (!string.Equals(ferramentaAtual.Nome, "Selecionar", StringComparison.OrdinalIgnoreCase) || ferramentaAtual.IsBusy)
                return false;

            bool selecionou = viewModel.SelectLineAt(position, LineHitTolerance);
            AtualizarHandlesOverlay();

            if (!selecionou)
            {
                LimparEstadoArrasteLinhaTemplate();
                AtualizarHandlesOverlay();
                return true;
            }

            if (!viewModel.TryGetSelectedLineId(out Guid lineId))
                return true;

            _linhaTemplateEmArrasteId = lineId;
            _linhaTemplateDragStart = position;
            _linhaTemplateArrastando = false;
            _linhaTemplateInteractionMode = LinhaTemplateInteractionMode.Line;
            TemplatePageBorder.CaptureMouse();
            AtualizarHandlesOverlay();
            return true;
        }

        private bool AtualizarArrasteLinhaTemplate(Point position, MouseEventArgs e)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel == null || !_linhaTemplateEmArrasteId.HasValue)
                return false;

            if (e.LeftButton != MouseButtonState.Pressed)
                return false;

            Vector delta = position - _linhaTemplateDragStart;

            if (!_linhaTemplateArrastando && delta.LengthSquared < DragThresholdSquared)
                return true;

            _linhaTemplateArrastando = true;
            viewModel.SetLinePreviewOffset(_linhaTemplateEmArrasteId.Value, delta.X, delta.Y);
            AtualizarHandlesOverlay();
            return true;
        }

        private bool FinalizarArrasteLinhaTemplate(Point position)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null || !_linhaTemplateEmArrasteId.HasValue)
                return false;

            Guid lineId = _linhaTemplateEmArrasteId.Value;
            bool arrastou = _linhaTemplateArrastando;
            Vector delta = position - _linhaTemplateDragStart;

            viewModel.ClearLinePreviewOffset(lineId);
            LimparEstadoArrasteLinhaTemplate();

            if (!arrastou)
            {
                AtualizarHandlesOverlay();
                return true;
            }

            bool moveu = _context.MoverLinhaDoTipoPrancha.Mover(viewModel.Id, lineId, delta.X, delta.Y);

            if (moveu)
                viewModel.SelectLine(lineId);

            AtualizarHandlesOverlay();
            return true;
        }

        private void CancelarArrasteLinhaTemplate()
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel != null && _linhaTemplateEmArrasteId.HasValue)
                viewModel.ClearLinePreviewOffset(_linhaTemplateEmArrasteId.Value);

            LimparEstadoArrasteLinhaTemplate();
            AtualizarHandlesOverlay();
        }

        private void LimparEstadoArrasteLinhaTemplate()
        {
            _linhaTemplateEmArrasteId = null;
            _linhaTemplateArrastando = false;
            _linhaTemplateInteractionMode = LinhaTemplateInteractionMode.None;
        }

        private bool ExcluirLinhaSelecionada()
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null)
                return false;

            if (!viewModel.TryGetSelectedLineId(out Guid lineId))
                return false;

            bool excluiu = _context.ExcluirLinhaDoTipoPrancha.Excluir(viewModel.Id, lineId);

            if (excluiu)
                viewModel.ClearLineSelection();

            AtualizarHandlesOverlay();
            return excluiu;
        }

        private ProjectSheetTypeViewModel? ObterViewModelAtivo()
        {
            if (DataContext is ProjectSheetTypeViewModel viewModel)
            {
                if (_context != null && !ReferenceEquals(_context.ProjectSheetTypeViewModelAtivo, viewModel))
                    _context.ProjectSheetTypeViewModelAtivo = viewModel;

                return viewModel;
            }

            return _context?.ProjectSheetTypeViewModelAtivo;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ProjectSheetTypeViewModel viewModel)
            {
                if (_context != null)
                    _context.ProjectSheetTypeViewModelAtivo = viewModel;
            }

            AtualizarHandlesOverlay();
        }

        private void AtualizarHandlesOverlay()
        {
            ObterViewModelAtivo();
        }

        private static ToolInputState CriarInputState(MouseEventArgs e, Point localPosition, MouseButton? button = null, int clickCount = 0)
        {
            return new ToolInputState(Keyboard.Modifiers, button, clickCount, localPosition, e.GetPosition(null));
        }
    }
}
