using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Araci.Core.Documents;
using Araci.Applications.Editar.Base;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Views
{
    public partial class ProjectSheetTypeView : UserControl
    {
        private const double LineHitTolerance = 6.0;
        private const double EndpointHitTolerance = 8.0;
        private const double RectangleHitTolerance = 3.0;
        private const double CircleHitTolerance = 3.0;
        private const double TextHitTolerance = 3.0;
        private const double RectangleResizeHandleHitTolerance = 8.0;
        private const double CircleResizeHandleHitTolerance = 8.0;
        private const double TextResizeHandleHitTolerance = 8.0;
        private const double TextLeaderHandleHitTolerance = 8.0;
        private const double DragThresholdSquared = 9.0;
        private const double LayoutTolerance = 1.0;
        private const int CenterSheetMaxAttempts = 10;
        private const int CenterSheetSettlingAttempts = 2;

        private EditorContext? _context;
        private Guid? _linhaTemplateEmArrasteId;
        private Point _linhaTemplateDragStart;
        private bool _linhaTemplateArrastando;
        private Guid? _retanguloTemplateEmArrasteId;
        private Point _retanguloTemplateDragStart;
        private bool _retanguloTemplateArrastando;
        private Guid? _circuloTemplateEmArrasteId;
        private Point _circuloTemplateDragStart;
        private bool _circuloTemplateArrastando;
        private Guid? _textoTemplateEmArrasteId;
        private Point _textoTemplateDragStart;
        private bool _textoTemplateArrastando;
        private Guid? _textoTemplateResizeEmArrasteId;
        private Point _textoTemplateResizeDragStart;
        private Point _textoTemplateResizeLastPosition;
        private bool _textoTemplateResizeLastPositionValid;
        private bool _textoTemplateResizeArrastando;
        private double _textoTemplateResizeOriginalX;
        private Guid? _textoTemplateLeaderEmArrasteId;
        private ProjectSheetTemplateTextLeaderHandleKind? _textoTemplateLeaderHandleEmArraste;
        private Point _textoTemplateLeaderDragStart;
        private Point _textoTemplateLeaderLastPosition;
        private bool _textoTemplateLeaderLastPositionValid;
        private bool _textoTemplateLeaderArrastando;
        private bool _textoTemplateInlineCommitEmAndamento;
        private Guid? _circuloTemplateResizeEmArrasteId;
        private Point _circuloTemplateResizeDragStart;
        private Point _circuloTemplateResizeLastPosition;
        private bool _circuloTemplateResizeLastPositionValid;
        private bool _circuloTemplateResizeArrastando;
        private double _circuloTemplateResizeOriginalX;
        private double _circuloTemplateResizeOriginalY;
        private Guid? _retanguloTemplateResizeEmArrasteId;
        private RetanguloResizeHandleKind? _retanguloTemplateResizeHandleEmArraste;
        private Point _retanguloTemplateResizeDragStart;
        private Point _retanguloTemplateResizeLastPosition;
        private bool _retanguloTemplateResizeLastPositionValid;
        private bool _retanguloTemplateResizeArrastando;
        private double _retanguloTemplateResizeOriginalX;
        private double _retanguloTemplateResizeOriginalY;
        private double _retanguloTemplateResizeOriginalLargura;
        private double _retanguloTemplateResizeOriginalAltura;
        private Guid? _linhaTemplateEndpointEmArrasteId;
        private ProjectSheetTemplateLineEndpoint _linhaTemplateEndpointEmArraste;
        private Point _linhaTemplateEndpointDragStart;
        private Point _linhaTemplateEndpointLastPosition;
        private bool _linhaTemplateEndpointLastPositionValid;
        private bool _linhaTemplateEndpointArrastando;
        private double _linhaTemplateEndpointOriginalX1;
        private double _linhaTemplateEndpointOriginalY1;
        private double _linhaTemplateEndpointOriginalX2;
        private double _linhaTemplateEndpointOriginalY2;
        private int _centerSheetRequestVersion;

        public ProjectSheetTypeView()
        {
            InitializeComponent();
            DataContext = null;
            Loaded += ProjectSheetTypeView_Loaded;
            SizeChanged += ProjectSheetTypeView_SizeChanged;
            DataContextChanged += OnDataContextChanged;
        }

        public void Inicializar(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            if (DataContext is ProjectSheetTypeViewModel viewModel)
            {
                DefinirComoSuperficieAtiva(viewModel);
                CenterSheetInViewportDeferred();
            }
        }

        private void ProjectSheetTypeView_Loaded(object sender, RoutedEventArgs e)
        {
            CenterSheetInViewportDeferred();
        }

        private void ProjectSheetTypeView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CenterSheetInViewportDeferred();
        }

        private void TemplatePageBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_context == null)
                return;

            if (OrigemEstaDentroDeEditorInline(e.OriginalSource as DependencyObject))
                return;

            Focus();
            Keyboard.Focus(this);

            if (ObterViewModelAtivo() is ProjectSheetTypeViewModel viewModel)
                DefinirComoSuperficieAtiva(viewModel);

            Point position = ObterPontoLocalFolha(e);

            if (TentarIniciarEdicaoLeaderTextoTemplate(position))
            {
                e.Handled = true;
                return;
            }

            if (TentarIniciarResizeCirculoTemplate(position))
            {
                e.Handled = true;
                return;
            }

            if (TentarIniciarResizeRetanguloTemplate(position))
            {
                e.Handled = true;
                return;
            }

            if (TentarIniciarResizeTextoTemplate(position))
            {
                e.Handled = true;
                return;
            }

            if (TentarIniciarEdicaoExtremidadeLinhaTemplate(position))
            {
                e.Handled = true;
                return;
            }

            if (TentarIniciarInteracaoTemplate(position, e.ClickCount))
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

            Point position = ObterPontoLocalFolha(e);

            if (TextoInlineEmEdicaoAtivo())
                return;

            if (AtualizarEdicaoLeaderTextoTemplate(position, e))
            {
                e.Handled = true;
                return;
            }

            if (AtualizarResizeCirculoTemplate(position, e))
            {
                e.Handled = true;
                return;
            }

            if (AtualizarResizeRetanguloTemplate(position, e))
            {
                e.Handled = true;
                return;
            }

            if (AtualizarResizeTextoTemplate(position, e))
            {
                e.Handled = true;
                return;
            }

            if (AtualizarEdicaoExtremidadeLinhaTemplate(position, e))
            {
                e.Handled = true;
                return;
            }

            if (AtualizarArrasteTextoTemplate(position, e))
            {
                e.Handled = true;
                return;
            }

            if (AtualizarArrasteLinhaTemplate(position, e))
            {
                e.Handled = true;
                return;
            }

            if (AtualizarArrasteRetanguloTemplate(position, e))
            {
                e.Handled = true;
                return;
            }

            if (AtualizarArrasteCirculoTemplate(position, e))
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

            Point position = ObterPontoLocalFolha(e);

            if (FinalizarEdicaoLeaderTextoTemplate())
            {
                if (TemplatePageBorder.IsMouseCaptured)
                    TemplatePageBorder.ReleaseMouseCapture();

                AtualizarHandlesOverlay();
                e.Handled = true;
                return;
            }

            if (FinalizarResizeCirculoTemplate())
            {
                if (TemplatePageBorder.IsMouseCaptured)
                    TemplatePageBorder.ReleaseMouseCapture();

                AtualizarHandlesOverlay();
                e.Handled = true;
                return;
            }

            if (FinalizarResizeRetanguloTemplate())
            {
                if (TemplatePageBorder.IsMouseCaptured)
                    TemplatePageBorder.ReleaseMouseCapture();

                AtualizarHandlesOverlay();
                e.Handled = true;
                return;
            }

            if (FinalizarResizeTextoTemplate())
            {
                if (TemplatePageBorder.IsMouseCaptured)
                    TemplatePageBorder.ReleaseMouseCapture();

                AtualizarHandlesOverlay();
                e.Handled = true;
                return;
            }

            if (FinalizarEdicaoExtremidadeLinhaTemplate())
            {
                if (TemplatePageBorder.IsMouseCaptured)
                    TemplatePageBorder.ReleaseMouseCapture();

                AtualizarHandlesOverlay();
                e.Handled = true;
                return;
            }

            if (FinalizarArrasteTextoTemplate(position))
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

            if (FinalizarArrasteRetanguloTemplate(position))
            {
                if (TemplatePageBorder.IsMouseCaptured)
                    TemplatePageBorder.ReleaseMouseCapture();

                AtualizarHandlesOverlay();
                e.Handled = true;
                return;
            }

            if (FinalizarArrasteCirculoTemplate(position))
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
            if (_circuloTemplateResizeEmArrasteId.HasValue)
                CancelarResizeCirculoTemplate();

            if (_retanguloTemplateResizeEmArrasteId.HasValue)
                CancelarResizeRetanguloTemplate();

            if (_textoTemplateResizeEmArrasteId.HasValue)
                CancelarResizeTextoTemplate();

            if (_textoTemplateLeaderEmArrasteId.HasValue)
                CancelarEdicaoLeaderTextoTemplate();

            if (_linhaTemplateEndpointEmArrasteId.HasValue)
                CancelarEdicaoExtremidadeLinhaTemplate();

            if (_linhaTemplateEmArrasteId.HasValue)
                CancelarArrasteLinhaTemplate();

            if (_retanguloTemplateEmArrasteId.HasValue)
                CancelarArrasteRetanguloTemplate();

            if (_circuloTemplateEmArrasteId.HasValue)
                CancelarArrasteCirculoTemplate();

            if (_textoTemplateEmArrasteId.HasValue)
                CancelarArrasteTextoTemplate();

            AtualizarHandlesOverlay();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_context == null)
                return;

            if (ProcessarTecladoEdicaoInlineTexto(e))
                return;

            if (e.Key == Key.Enter && IniciarEdicaoInlineTextoSelecionado())
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape && _textoTemplateLeaderEmArrasteId.HasValue)
            {
                CancelarEdicaoLeaderTextoTemplate();
                AtualizarHandlesOverlay();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape && _circuloTemplateResizeEmArrasteId.HasValue)
            {
                CancelarResizeCirculoTemplate();
                AtualizarHandlesOverlay();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape && _retanguloTemplateResizeEmArrasteId.HasValue)
            {
                CancelarResizeRetanguloTemplate();
                AtualizarHandlesOverlay();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape && _textoTemplateResizeEmArrasteId.HasValue)
            {
                CancelarResizeTextoTemplate();
                AtualizarHandlesOverlay();
                e.Handled = true;
                return;
            }

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

            if (e.Key == Key.Escape && _retanguloTemplateEmArrasteId.HasValue)
            {
                CancelarArrasteRetanguloTemplate();
                AtualizarHandlesOverlay();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape && _circuloTemplateEmArrasteId.HasValue)
            {
                CancelarArrasteCirculoTemplate();
                AtualizarHandlesOverlay();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape && _textoTemplateEmArrasteId.HasValue)
            {
                CancelarArrasteTextoTemplate();
                AtualizarHandlesOverlay();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Delete && ExcluirTemplateSelecionado())
            {
                AtualizarHandlesOverlay();
                e.Handled = true;
                return;
            }

            if (_context.Input.KeyDown(e.Key))
                e.Handled = true;
        }

        private bool TentarIniciarResizeCirculoTemplate(Point position)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null)
                return false;

            ITool ferramentaAtual = _context.Tools.FerramentaAtual;

            if (!string.Equals(ferramentaAtual.Nome, "Selecionar", StringComparison.OrdinalIgnoreCase) || ferramentaAtual.IsBusy)
                return false;

            if (!viewModel.TryHitSelectedCircleResizeHandle(position, CircleResizeHandleHitTolerance, out Guid circleId))
                return false;

            if (!viewModel.TryGetCircleGeometry(
                    circleId,
                    out _circuloTemplateResizeOriginalX,
                    out _circuloTemplateResizeOriginalY,
                    out _))
            {
                return false;
            }

            _circuloTemplateResizeEmArrasteId = circleId;
            _circuloTemplateResizeDragStart = position;
            _circuloTemplateResizeLastPosition = position;
            _circuloTemplateResizeLastPositionValid = PontoValido(position);
            _circuloTemplateResizeArrastando = false;
            LimparEstadoArrasteLinhaTemplate();
            LimparEstadoArrasteRetanguloTemplate();
            LimparEstadoArrasteCirculoTemplate();
            LimparEstadoArrasteTextoTemplate();
            LimparEstadoResizeRetanguloTemplate();
            LimparEstadoResizeTextoTemplate();
            LimparEstadoEdicaoLeaderTextoTemplate();
            TemplatePageBorder.CaptureMouse();
            AtualizarHandlesOverlay();
            return true;
        }

        private bool AtualizarResizeCirculoTemplate(Point position, MouseEventArgs e)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel == null || !_circuloTemplateResizeEmArrasteId.HasValue)
                return false;

            if (e.LeftButton != MouseButtonState.Pressed)
                return false;

            Vector delta = position - _circuloTemplateResizeDragStart;

            if (!_circuloTemplateResizeArrastando && delta.LengthSquared < DragThresholdSquared)
                return true;

            if (!PontoValido(position))
                return true;

            double raio = CalcularRaioCirculoTemplate(
                _circuloTemplateResizeOriginalX,
                _circuloTemplateResizeOriginalY,
                position);

            bool previewAplicado = viewModel.SetCirclePreviewRadius(_circuloTemplateResizeEmArrasteId.Value, raio);

            if (!previewAplicado)
                return true;

            _circuloTemplateResizeArrastando = true;
            _circuloTemplateResizeLastPosition = position;
            _circuloTemplateResizeLastPositionValid = true;
            AtualizarHandlesOverlay();
            return true;
        }

        private bool FinalizarResizeCirculoTemplate()
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null || !_circuloTemplateResizeEmArrasteId.HasValue)
                return false;

            Guid circleId = _circuloTemplateResizeEmArrasteId.Value;
            bool arrastou = _circuloTemplateResizeArrastando;
            Point finalPosition = _circuloTemplateResizeLastPosition;
            bool finalPositionValid = _circuloTemplateResizeLastPositionValid && PontoValido(finalPosition);
            double originalX = _circuloTemplateResizeOriginalX;
            double originalY = _circuloTemplateResizeOriginalY;

            viewModel.ClearCirclePreviewRadius(circleId);
            LimparEstadoResizeCirculoTemplate();

            if (!arrastou || !finalPositionValid)
            {
                AtualizarHandlesOverlay();
                return true;
            }

            double raio = CalcularRaioCirculoTemplate(originalX, originalY, finalPosition);
            bool alterou = _context.MoverCirculoDoTipoPrancha.AlterarRaio(viewModel.Id, circleId, raio);

            if (alterou)
                viewModel.SelectCircle(circleId);

            AtualizarPainelPropriedadesTemplateSelecionado();
            AtualizarHandlesOverlay();
            return true;
        }

        private void CancelarResizeCirculoTemplate()
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel != null && _circuloTemplateResizeEmArrasteId.HasValue)
                viewModel.ClearCirclePreviewRadius(_circuloTemplateResizeEmArrasteId.Value);

            LimparEstadoResizeCirculoTemplate();
            AtualizarHandlesOverlay();
        }

        private void LimparEstadoResizeCirculoTemplate()
        {
            _circuloTemplateResizeEmArrasteId = null;
            _circuloTemplateResizeDragStart = default;
            _circuloTemplateResizeLastPosition = default;
            _circuloTemplateResizeLastPositionValid = false;
            _circuloTemplateResizeArrastando = false;
            _circuloTemplateResizeOriginalX = 0.0;
            _circuloTemplateResizeOriginalY = 0.0;
        }

        private bool TentarIniciarEdicaoLeaderTextoTemplate(Point position)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null)
                return false;

            ITool ferramentaAtual = _context.Tools.FerramentaAtual;

            if (!string.Equals(ferramentaAtual.Nome, "Selecionar", StringComparison.OrdinalIgnoreCase) || ferramentaAtual.IsBusy)
                return false;

            if (!viewModel.TryHitSelectedTextLeaderHandle(position, TextLeaderHandleHitTolerance, out Guid textId, out ProjectSheetTemplateTextLeaderHandleKind kind))
                return false;

            _textoTemplateLeaderEmArrasteId = textId;
            _textoTemplateLeaderHandleEmArraste = kind;
            _textoTemplateLeaderDragStart = position;
            _textoTemplateLeaderLastPosition = position;
            _textoTemplateLeaderLastPositionValid = PontoValido(position);
            _textoTemplateLeaderArrastando = false;
            LimparEstadoArrasteLinhaTemplate();
            LimparEstadoArrasteRetanguloTemplate();
            LimparEstadoArrasteCirculoTemplate();
            LimparEstadoArrasteTextoTemplate();
            LimparEstadoResizeRetanguloTemplate();
            LimparEstadoResizeCirculoTemplate();
            LimparEstadoResizeTextoTemplate();
            TemplatePageBorder.CaptureMouse();
            AtualizarHandlesOverlay();
            return true;
        }

        private bool AtualizarEdicaoLeaderTextoTemplate(Point position, MouseEventArgs e)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel == null || !_textoTemplateLeaderEmArrasteId.HasValue || !_textoTemplateLeaderHandleEmArraste.HasValue)
                return false;

            if (e.LeftButton != MouseButtonState.Pressed)
                return false;

            Vector delta = position - _textoTemplateLeaderDragStart;

            if (!_textoTemplateLeaderArrastando && delta.LengthSquared < DragThresholdSquared)
                return true;

            if (!PontoValido(position))
                return true;

            bool previewAplicado = AplicarPreviewLeaderTextoTemplate(viewModel, _textoTemplateLeaderEmArrasteId.Value, _textoTemplateLeaderHandleEmArraste.Value, position);

            if (!previewAplicado)
                return true;

            _textoTemplateLeaderArrastando = true;
            _textoTemplateLeaderLastPosition = position;
            _textoTemplateLeaderLastPositionValid = true;
            AtualizarHandlesOverlay();
            return true;
        }

        private bool FinalizarEdicaoLeaderTextoTemplate()
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null || !_textoTemplateLeaderEmArrasteId.HasValue || !_textoTemplateLeaderHandleEmArraste.HasValue)
                return false;

            Guid textId = _textoTemplateLeaderEmArrasteId.Value;
            ProjectSheetTemplateTextLeaderHandleKind kind = _textoTemplateLeaderHandleEmArraste.Value;
            bool arrastou = _textoTemplateLeaderArrastando;
            Point finalPosition = _textoTemplateLeaderLastPosition;
            bool finalPositionValid = _textoTemplateLeaderLastPositionValid && PontoValido(finalPosition);

            LimparPreviewLeaderTextoTemplate(viewModel, textId, kind);
            LimparEstadoEdicaoLeaderTextoTemplate();

            if (!arrastou || !finalPositionValid)
            {
                AtualizarHandlesOverlay();
                return true;
            }

            bool alterou = kind == ProjectSheetTemplateTextLeaderHandleKind.End
                ? _context.MoverTextoDoTipoPrancha.AlterarLeaderPoint(viewModel.Id, textId, finalPosition.X, finalPosition.Y)
                : _context.MoverTextoDoTipoPrancha.AlterarLeaderCotoveloPoint(viewModel.Id, textId, finalPosition.X, finalPosition.Y);

            if (alterou)
                viewModel.SelectText(textId);

            AtualizarPainelPropriedadesTemplateSelecionado();
            AtualizarHandlesOverlay();
            return true;
        }

        private void CancelarEdicaoLeaderTextoTemplate()
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel != null && _textoTemplateLeaderEmArrasteId.HasValue && _textoTemplateLeaderHandleEmArraste.HasValue)
                LimparPreviewLeaderTextoTemplate(viewModel, _textoTemplateLeaderEmArrasteId.Value, _textoTemplateLeaderHandleEmArraste.Value);

            LimparEstadoEdicaoLeaderTextoTemplate();
            AtualizarHandlesOverlay();
        }

        private void LimparEstadoEdicaoLeaderTextoTemplate()
        {
            _textoTemplateLeaderEmArrasteId = null;
            _textoTemplateLeaderHandleEmArraste = null;
            _textoTemplateLeaderDragStart = default;
            _textoTemplateLeaderLastPosition = default;
            _textoTemplateLeaderLastPositionValid = false;
            _textoTemplateLeaderArrastando = false;
        }

        private static bool AplicarPreviewLeaderTextoTemplate(
            ProjectSheetTypeViewModel viewModel,
            Guid textId,
            ProjectSheetTemplateTextLeaderHandleKind kind,
            Point position)
        {
            return kind == ProjectSheetTemplateTextLeaderHandleKind.End
                ? viewModel.SetTextPreviewLeaderPoint(textId, position)
                : viewModel.SetTextPreviewLeaderCotoveloPoint(textId, position);
        }

        private static void LimparPreviewLeaderTextoTemplate(
            ProjectSheetTypeViewModel viewModel,
            Guid textId,
            ProjectSheetTemplateTextLeaderHandleKind kind)
        {
            if (kind == ProjectSheetTemplateTextLeaderHandleKind.End)
                viewModel.ClearTextPreviewLeaderPoint(textId);
            else
                viewModel.ClearTextPreviewLeaderCotoveloPoint(textId);
        }

        private bool TentarIniciarResizeTextoTemplate(Point position)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null)
                return false;

            ITool ferramentaAtual = _context.Tools.FerramentaAtual;

            if (!string.Equals(ferramentaAtual.Nome, "Selecionar", StringComparison.OrdinalIgnoreCase) || ferramentaAtual.IsBusy)
                return false;

            if (!viewModel.TryHitSelectedTextResizeHandle(position, TextResizeHandleHitTolerance, out Guid textId))
                return false;

            if (!viewModel.TryGetTextGeometry(
                    textId,
                    out _textoTemplateResizeOriginalX,
                    out _,
                    out _,
                    out _))
            {
                return false;
            }

            _textoTemplateResizeEmArrasteId = textId;
            _textoTemplateResizeDragStart = position;
            _textoTemplateResizeLastPosition = position;
            _textoTemplateResizeLastPositionValid = PontoValido(position);
            _textoTemplateResizeArrastando = false;
            LimparEstadoArrasteLinhaTemplate();
            LimparEstadoArrasteRetanguloTemplate();
            LimparEstadoArrasteCirculoTemplate();
            LimparEstadoArrasteTextoTemplate();
            LimparEstadoResizeRetanguloTemplate();
            LimparEstadoResizeCirculoTemplate();
            TemplatePageBorder.CaptureMouse();
            AtualizarHandlesOverlay();
            return true;
        }

        private bool AtualizarResizeTextoTemplate(Point position, MouseEventArgs e)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel == null || !_textoTemplateResizeEmArrasteId.HasValue)
                return false;

            if (e.LeftButton != MouseButtonState.Pressed)
                return false;

            Vector delta = position - _textoTemplateResizeDragStart;

            if (!_textoTemplateResizeArrastando && delta.LengthSquared < DragThresholdSquared)
                return true;

            if (!PontoValido(position))
                return true;

            double larguraCaixa = CalcularLarguraTextoTemplate(_textoTemplateResizeOriginalX, position);
            bool previewAplicado = viewModel.SetTextPreviewBoxWidth(_textoTemplateResizeEmArrasteId.Value, larguraCaixa);

            if (!previewAplicado)
                return true;

            _textoTemplateResizeArrastando = true;
            _textoTemplateResizeLastPosition = position;
            _textoTemplateResizeLastPositionValid = true;
            AtualizarHandlesOverlay();
            return true;
        }

        private bool FinalizarResizeTextoTemplate()
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null || !_textoTemplateResizeEmArrasteId.HasValue)
                return false;

            Guid textId = _textoTemplateResizeEmArrasteId.Value;
            bool arrastou = _textoTemplateResizeArrastando;
            Point finalPosition = _textoTemplateResizeLastPosition;
            bool finalPositionValid = _textoTemplateResizeLastPositionValid && PontoValido(finalPosition);
            double originalX = _textoTemplateResizeOriginalX;

            viewModel.ClearTextPreviewBoxWidth(textId);
            LimparEstadoResizeTextoTemplate();
            LimparEstadoEdicaoLeaderTextoTemplate();

            if (!arrastou || !finalPositionValid)
            {
                AtualizarHandlesOverlay();
                return true;
            }

            double larguraCaixa = CalcularLarguraTextoTemplate(originalX, finalPosition);
            bool alterou = _context.MoverTextoDoTipoPrancha.AlterarLarguraCaixa(viewModel.Id, textId, larguraCaixa);

            if (alterou)
                viewModel.SelectText(textId);

            AtualizarPainelPropriedadesTemplateSelecionado();
            AtualizarHandlesOverlay();
            return true;
        }

        private void CancelarResizeTextoTemplate()
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel != null && _textoTemplateResizeEmArrasteId.HasValue)
                viewModel.ClearTextPreviewBoxWidth(_textoTemplateResizeEmArrasteId.Value);

            LimparEstadoResizeTextoTemplate();
            LimparEstadoEdicaoLeaderTextoTemplate();
            AtualizarHandlesOverlay();
        }

        private void LimparEstadoResizeTextoTemplate()
        {
            _textoTemplateResizeEmArrasteId = null;
            _textoTemplateResizeDragStart = default;
            _textoTemplateResizeLastPosition = default;
            _textoTemplateResizeLastPositionValid = false;
            _textoTemplateResizeArrastando = false;
            _textoTemplateResizeOriginalX = 0.0;
        }

        private bool TentarIniciarResizeRetanguloTemplate(Point position)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null)
                return false;

            ITool ferramentaAtual = _context.Tools.FerramentaAtual;

            if (!string.Equals(ferramentaAtual.Nome, "Selecionar", StringComparison.OrdinalIgnoreCase) || ferramentaAtual.IsBusy)
                return false;

            if (!viewModel.TryHitSelectedRectangleResizeHandle(position, RectangleResizeHandleHitTolerance, out Guid rectangleId, out RetanguloResizeHandleKind kind))
                return false;

            if (!viewModel.TryGetRectangleGeometry(
                    rectangleId,
                    out _retanguloTemplateResizeOriginalX,
                    out _retanguloTemplateResizeOriginalY,
                    out _retanguloTemplateResizeOriginalLargura,
                    out _retanguloTemplateResizeOriginalAltura))
            {
                return false;
            }

            _retanguloTemplateResizeEmArrasteId = rectangleId;
            _retanguloTemplateResizeHandleEmArraste = kind;
            _retanguloTemplateResizeDragStart = position;
            _retanguloTemplateResizeLastPosition = position;
            _retanguloTemplateResizeLastPositionValid = PontoValido(position);
            _retanguloTemplateResizeArrastando = false;
            LimparEstadoArrasteLinhaTemplate();
            LimparEstadoArrasteRetanguloTemplate();
            LimparEstadoArrasteCirculoTemplate();
            LimparEstadoArrasteTextoTemplate();
            LimparEstadoResizeCirculoTemplate();
            LimparEstadoResizeTextoTemplate();
            LimparEstadoEdicaoLeaderTextoTemplate();
            TemplatePageBorder.CaptureMouse();
            AtualizarHandlesOverlay();
            return true;
        }

        private bool AtualizarResizeRetanguloTemplate(Point position, MouseEventArgs e)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel == null || !_retanguloTemplateResizeEmArrasteId.HasValue || !_retanguloTemplateResizeHandleEmArraste.HasValue)
                return false;

            if (e.LeftButton != MouseButtonState.Pressed)
                return false;

            Vector delta = position - _retanguloTemplateResizeDragStart;

            if (!_retanguloTemplateResizeArrastando && delta.LengthSquared < DragThresholdSquared)
                return true;

            if (!PontoValido(position))
                return true;

            Rect geometry = CalcularGeometriaRetanguloTemplate(
                _retanguloTemplateResizeOriginalX,
                _retanguloTemplateResizeOriginalY,
                _retanguloTemplateResizeOriginalLargura,
                _retanguloTemplateResizeOriginalAltura,
                _retanguloTemplateResizeHandleEmArraste.Value,
                position);

            bool previewAplicado = viewModel.SetRectanglePreviewGeometry(
                _retanguloTemplateResizeEmArrasteId.Value,
                geometry.X,
                geometry.Y,
                geometry.Width,
                geometry.Height);

            if (!previewAplicado)
                return true;

            _retanguloTemplateResizeArrastando = true;
            _retanguloTemplateResizeLastPosition = position;
            _retanguloTemplateResizeLastPositionValid = true;
            AtualizarHandlesOverlay();
            return true;
        }

        private bool FinalizarResizeRetanguloTemplate()
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null || !_retanguloTemplateResizeEmArrasteId.HasValue || !_retanguloTemplateResizeHandleEmArraste.HasValue)
                return false;

            Guid rectangleId = _retanguloTemplateResizeEmArrasteId.Value;
            RetanguloResizeHandleKind kind = _retanguloTemplateResizeHandleEmArraste.Value;
            bool arrastou = _retanguloTemplateResizeArrastando;
            Point finalPosition = _retanguloTemplateResizeLastPosition;
            bool finalPositionValid = _retanguloTemplateResizeLastPositionValid && PontoValido(finalPosition);
            double originalX = _retanguloTemplateResizeOriginalX;
            double originalY = _retanguloTemplateResizeOriginalY;
            double originalLargura = _retanguloTemplateResizeOriginalLargura;
            double originalAltura = _retanguloTemplateResizeOriginalAltura;

            viewModel.ClearRectanglePreviewGeometry(rectangleId);
            LimparEstadoResizeRetanguloTemplate();

            if (!arrastou || !finalPositionValid)
            {
                AtualizarHandlesOverlay();
                return true;
            }

            Rect geometry = CalcularGeometriaRetanguloTemplate(
                originalX,
                originalY,
                originalLargura,
                originalAltura,
                kind,
                finalPosition);

            bool alterou = _context.MoverRetanguloDoTipoPrancha.AlterarGeometria(
                viewModel.Id,
                rectangleId,
                geometry.X,
                geometry.Y,
                geometry.Width,
                geometry.Height);

            if (alterou)
                viewModel.SelectRectangle(rectangleId);

            AtualizarPainelPropriedadesTemplateSelecionado();
            AtualizarHandlesOverlay();
            return true;
        }

        private void CancelarResizeRetanguloTemplate()
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel != null && _retanguloTemplateResizeEmArrasteId.HasValue)
                viewModel.ClearRectanglePreviewGeometry(_retanguloTemplateResizeEmArrasteId.Value);

            LimparEstadoResizeRetanguloTemplate();
            AtualizarHandlesOverlay();
        }

        private void LimparEstadoResizeRetanguloTemplate()
        {
            _retanguloTemplateResizeEmArrasteId = null;
            _retanguloTemplateResizeHandleEmArraste = null;
            _retanguloTemplateResizeDragStart = default;
            _retanguloTemplateResizeLastPosition = default;
            _retanguloTemplateResizeLastPositionValid = false;
            _retanguloTemplateResizeArrastando = false;
            _retanguloTemplateResizeOriginalX = 0.0;
            _retanguloTemplateResizeOriginalY = 0.0;
            _retanguloTemplateResizeOriginalLargura = 0.0;
            _retanguloTemplateResizeOriginalAltura = 0.0;
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
            _linhaTemplateEndpointLastPosition = position;
            _linhaTemplateEndpointLastPositionValid = PontoValido(position);
            _linhaTemplateEndpointArrastando = false;
            _linhaTemplateEmArrasteId = null;
            _linhaTemplateArrastando = false;
            LimparEstadoArrasteCirculoTemplate();
            LimparEstadoArrasteTextoTemplate();
            LimparEstadoResizeRetanguloTemplate();
            LimparEstadoResizeTextoTemplate();
            LimparEstadoEdicaoLeaderTextoTemplate();
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

            if (!PontoValido(position))
                return true;

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

            bool previewAplicado = viewModel.SetLinePreviewCoordinates(
                _linhaTemplateEndpointEmArrasteId.Value,
                previewX1,
                previewY1,
                previewX2,
                previewY2);

            if (!previewAplicado)
                return true;

            _linhaTemplateEndpointArrastando = true;
            _linhaTemplateEndpointLastPosition = position;
            _linhaTemplateEndpointLastPositionValid = true;
            AtualizarHandlesOverlay();
            return true;
        }

        private bool FinalizarEdicaoExtremidadeLinhaTemplate()
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null || !_linhaTemplateEndpointEmArrasteId.HasValue)
                return false;

            Guid lineId = _linhaTemplateEndpointEmArrasteId.Value;
            bool arrastou = _linhaTemplateEndpointArrastando;
            ProjectSheetTemplateLineEndpoint endpoint = _linhaTemplateEndpointEmArraste;
            Point finalPosition = _linhaTemplateEndpointLastPosition;
            bool finalPositionValid = _linhaTemplateEndpointLastPositionValid && PontoValido(finalPosition);
            double originalX1 = _linhaTemplateEndpointOriginalX1;
            double originalY1 = _linhaTemplateEndpointOriginalY1;
            double originalX2 = _linhaTemplateEndpointOriginalX2;
            double originalY2 = _linhaTemplateEndpointOriginalY2;

            viewModel.ClearLinePreviewCoordinates(lineId);
            LimparEstadoEdicaoExtremidadeLinhaTemplate();

            if (!arrastou || !finalPositionValid)
            {
                AtualizarHandlesOverlay();
                return true;
            }

            double newX1 = endpoint == ProjectSheetTemplateLineEndpoint.Start ? finalPosition.X : originalX1;
            double newY1 = endpoint == ProjectSheetTemplateLineEndpoint.Start ? finalPosition.Y : originalY1;
            double newX2 = endpoint == ProjectSheetTemplateLineEndpoint.End ? finalPosition.X : originalX2;
            double newY2 = endpoint == ProjectSheetTemplateLineEndpoint.End ? finalPosition.Y : originalY2;

            bool alterou = _context.MoverLinhaDoTipoPrancha.AlterarCoordenadas(viewModel.Id, lineId, newX1, newY1, newX2, newY2);

            if (alterou)
                viewModel.SelectLine(lineId);

            AtualizarPainelPropriedadesTemplateSelecionado();
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
            _linhaTemplateEndpointDragStart = default;
            _linhaTemplateEndpointLastPosition = default;
            _linhaTemplateEndpointLastPositionValid = false;
            _linhaTemplateEndpointOriginalX1 = 0.0;
            _linhaTemplateEndpointOriginalY1 = 0.0;
            _linhaTemplateEndpointOriginalX2 = 0.0;
            _linhaTemplateEndpointOriginalY2 = 0.0;
        }

        private bool TentarIniciarInteracaoTemplate(Point position, int clickCount)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null)
                return false;

            ITool ferramentaAtual = _context.Tools.FerramentaAtual;

            if (!string.Equals(ferramentaAtual.Nome, "Selecionar", StringComparison.OrdinalIgnoreCase) || ferramentaAtual.IsBusy)
                return false;

            if (viewModel.TryHitTextAt(position, TextHitTolerance, out Guid textId))
            {
                viewModel.SelectText(textId);
                LimparEstadoArrasteLinhaTemplate();
                LimparEstadoArrasteRetanguloTemplate();
                LimparEstadoArrasteCirculoTemplate();
                LimparEstadoResizeRetanguloTemplate();
                LimparEstadoResizeCirculoTemplate();
                LimparEstadoResizeTextoTemplate();
                LimparEstadoEdicaoLeaderTextoTemplate();

                if (clickCount >= 2)
                {
                    LimparEstadoArrasteTextoTemplate();
                    IniciarEdicaoInlineTextoTemplate(textId);
                    AtualizarPainelPropriedadesTemplateSelecionado();
                    AtualizarHandlesOverlay();
                    return true;
                }

                _textoTemplateEmArrasteId = textId;
                _textoTemplateDragStart = position;
                _textoTemplateArrastando = false;
                TemplatePageBorder.CaptureMouse();
                AtualizarPainelPropriedadesTemplateSelecionado();
                AtualizarHandlesOverlay();
                return true;
            }

            if (viewModel.TryHitLineAt(position, LineHitTolerance, out Guid lineId))
            {
                viewModel.SelectLine(lineId);
                _linhaTemplateEmArrasteId = lineId;
                _linhaTemplateDragStart = position;
                _linhaTemplateArrastando = false;
                LimparEstadoArrasteRetanguloTemplate();
                LimparEstadoArrasteCirculoTemplate();
                LimparEstadoArrasteTextoTemplate();
                LimparEstadoResizeRetanguloTemplate();
                LimparEstadoResizeCirculoTemplate();
                LimparEstadoResizeTextoTemplate();
                LimparEstadoEdicaoLeaderTextoTemplate();
                TemplatePageBorder.CaptureMouse();
                AtualizarPainelPropriedadesTemplateSelecionado();
                AtualizarHandlesOverlay();
                return true;
            }

            if (viewModel.TryHitRectangleAt(position, RectangleHitTolerance, out Guid rectangleId))
            {
                viewModel.SelectRectangle(rectangleId);
                _retanguloTemplateEmArrasteId = rectangleId;
                _retanguloTemplateDragStart = position;
                _retanguloTemplateArrastando = false;
                LimparEstadoArrasteLinhaTemplate();
                LimparEstadoArrasteCirculoTemplate();
                LimparEstadoArrasteTextoTemplate();
                LimparEstadoResizeRetanguloTemplate();
                LimparEstadoResizeCirculoTemplate();
                LimparEstadoResizeTextoTemplate();
                LimparEstadoEdicaoLeaderTextoTemplate();
                TemplatePageBorder.CaptureMouse();
                AtualizarPainelPropriedadesTemplateSelecionado();
                AtualizarHandlesOverlay();
                return true;
            }

            if (viewModel.TryHitCircleAt(position, CircleHitTolerance, out Guid circleId))
            {
                viewModel.SelectCircle(circleId);
                _circuloTemplateEmArrasteId = circleId;
                _circuloTemplateDragStart = position;
                _circuloTemplateArrastando = false;
                LimparEstadoArrasteLinhaTemplate();
                LimparEstadoArrasteRetanguloTemplate();
                LimparEstadoArrasteTextoTemplate();
                LimparEstadoResizeRetanguloTemplate();
                LimparEstadoResizeCirculoTemplate();
                LimparEstadoResizeTextoTemplate();
                LimparEstadoEdicaoLeaderTextoTemplate();
                TemplatePageBorder.CaptureMouse();
                AtualizarPainelPropriedadesTemplateSelecionado();
                AtualizarHandlesOverlay();
                return true;
            }

            viewModel.ClearTemplateSelection();
            LimparEstadoArrasteLinhaTemplate();
            LimparEstadoArrasteRetanguloTemplate();
            LimparEstadoArrasteCirculoTemplate();
            LimparEstadoArrasteTextoTemplate();
            LimparEstadoResizeRetanguloTemplate();
            LimparEstadoResizeCirculoTemplate();
            LimparEstadoResizeTextoTemplate();
            LimparEstadoEdicaoLeaderTextoTemplate();
            AtualizarPainelPropriedadesTemplateSelecionado();
            AtualizarHandlesOverlay();
            return true;
        }

        private bool AtualizarArrasteTextoTemplate(Point position, MouseEventArgs e)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel == null || !_textoTemplateEmArrasteId.HasValue)
                return false;

            if (e.LeftButton != MouseButtonState.Pressed)
                return false;

            Vector delta = position - _textoTemplateDragStart;

            if (!_textoTemplateArrastando && delta.LengthSquared < DragThresholdSquared)
                return true;

            _textoTemplateArrastando = true;
            viewModel.SetTextPreviewOffset(_textoTemplateEmArrasteId.Value, delta.X, delta.Y);
            AtualizarHandlesOverlay();
            return true;
        }

        private bool FinalizarArrasteTextoTemplate(Point position)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null || !_textoTemplateEmArrasteId.HasValue)
                return false;

            Guid textId = _textoTemplateEmArrasteId.Value;
            bool arrastou = _textoTemplateArrastando;
            Vector delta = position - _textoTemplateDragStart;

            viewModel.ClearTextPreviewOffset(textId);
            LimparEstadoArrasteTextoTemplate();

            if (!arrastou)
            {
                AtualizarHandlesOverlay();
                return true;
            }

            bool moveu = _context.MoverTextoDoTipoPrancha.Mover(viewModel.Id, textId, delta.X, delta.Y);

            if (moveu)
                viewModel.SelectText(textId);

            AtualizarPainelPropriedadesTemplateSelecionado();
            AtualizarHandlesOverlay();
            return true;
        }

        private void CancelarArrasteTextoTemplate()
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel != null && _textoTemplateEmArrasteId.HasValue)
                viewModel.ClearTextPreviewOffset(_textoTemplateEmArrasteId.Value);

            LimparEstadoArrasteTextoTemplate();
            AtualizarHandlesOverlay();
        }

        private void LimparEstadoArrasteTextoTemplate()
        {
            _textoTemplateEmArrasteId = null;
            _textoTemplateArrastando = false;
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
        }

        private bool AtualizarArrasteRetanguloTemplate(Point position, MouseEventArgs e)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel == null || !_retanguloTemplateEmArrasteId.HasValue)
                return false;

            if (e.LeftButton != MouseButtonState.Pressed)
                return false;

            Vector delta = position - _retanguloTemplateDragStart;

            if (!_retanguloTemplateArrastando && delta.LengthSquared < DragThresholdSquared)
                return true;

            _retanguloTemplateArrastando = true;
            viewModel.SetRectanglePreviewOffset(_retanguloTemplateEmArrasteId.Value, delta.X, delta.Y);
            AtualizarHandlesOverlay();
            return true;
        }

        private bool FinalizarArrasteRetanguloTemplate(Point position)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null || !_retanguloTemplateEmArrasteId.HasValue)
                return false;

            Guid rectangleId = _retanguloTemplateEmArrasteId.Value;
            bool arrastou = _retanguloTemplateArrastando;
            Vector delta = position - _retanguloTemplateDragStart;

            viewModel.ClearRectanglePreviewOffset(rectangleId);
            LimparEstadoArrasteRetanguloTemplate();

            if (!arrastou)
            {
                AtualizarHandlesOverlay();
                return true;
            }

            bool moveu = _context.MoverRetanguloDoTipoPrancha.Mover(viewModel.Id, rectangleId, delta.X, delta.Y);

            if (moveu)
                viewModel.SelectRectangle(rectangleId);

            AtualizarPainelPropriedadesTemplateSelecionado();
            AtualizarHandlesOverlay();
            return true;
        }

        private void CancelarArrasteRetanguloTemplate()
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel != null && _retanguloTemplateEmArrasteId.HasValue)
                viewModel.ClearRectanglePreviewOffset(_retanguloTemplateEmArrasteId.Value);

            LimparEstadoArrasteRetanguloTemplate();
            AtualizarHandlesOverlay();
        }

        private void LimparEstadoArrasteRetanguloTemplate()
        {
            _retanguloTemplateEmArrasteId = null;
            _retanguloTemplateArrastando = false;
        }

        private bool AtualizarArrasteCirculoTemplate(Point position, MouseEventArgs e)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel == null || !_circuloTemplateEmArrasteId.HasValue)
                return false;

            if (e.LeftButton != MouseButtonState.Pressed)
                return false;

            Vector delta = position - _circuloTemplateDragStart;

            if (!_circuloTemplateArrastando && delta.LengthSquared < DragThresholdSquared)
                return true;

            _circuloTemplateArrastando = true;
            viewModel.SetCirclePreviewOffset(_circuloTemplateEmArrasteId.Value, delta.X, delta.Y);
            AtualizarHandlesOverlay();
            return true;
        }

        private bool FinalizarArrasteCirculoTemplate(Point position)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null || !_circuloTemplateEmArrasteId.HasValue)
                return false;

            Guid circleId = _circuloTemplateEmArrasteId.Value;
            bool arrastou = _circuloTemplateArrastando;
            Vector delta = position - _circuloTemplateDragStart;

            viewModel.ClearCirclePreviewOffset(circleId);
            LimparEstadoArrasteCirculoTemplate();

            if (!arrastou)
            {
                AtualizarHandlesOverlay();
                return true;
            }

            bool moveu = _context.MoverCirculoDoTipoPrancha.Mover(viewModel.Id, circleId, delta.X, delta.Y);

            if (moveu)
                viewModel.SelectCircle(circleId);

            AtualizarPainelPropriedadesTemplateSelecionado();
            AtualizarHandlesOverlay();
            return true;
        }

        private void CancelarArrasteCirculoTemplate()
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel != null && _circuloTemplateEmArrasteId.HasValue)
                viewModel.ClearCirclePreviewOffset(_circuloTemplateEmArrasteId.Value);

            LimparEstadoArrasteCirculoTemplate();
            AtualizarHandlesOverlay();
        }

        private void LimparEstadoArrasteCirculoTemplate()
        {
            _circuloTemplateEmArrasteId = null;
            _circuloTemplateArrastando = false;
        }

        private bool IniciarEdicaoInlineTextoSelecionado()
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel == null || !viewModel.TryGetSelectedTextId(out Guid textId))
                return false;

            return IniciarEdicaoInlineTextoTemplate(textId);
        }

        private bool IniciarEdicaoInlineTextoTemplate(Guid textId)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel == null)
                return false;

            if (!viewModel.BeginTextInlineEditing(textId))
                return false;

            LimparEstadoArrasteTextoTemplate();
            LimparEstadoResizeTextoTemplate();
            LimparEstadoEdicaoLeaderTextoTemplate();
            FocarEditorTextoInlineDeferred(textId);
            AtualizarHandlesOverlay();
            return true;
        }

        private bool TextoInlineEmEdicaoAtivo()
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();
            return viewModel?.HasTextInlineEditing == true;
        }

        private bool ProcessarTecladoEdicaoInlineTexto(KeyEventArgs e)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel == null || !viewModel.TryGetEditingText(out ProjectSheetTemplateTextViewModel? texto) || texto == null)
                return false;

            if (e.Key == Key.Escape)
            {
                CancelarEdicaoInlineTextoTemplate(texto);
                e.Handled = true;
                return true;
            }

            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                ConfirmarEdicaoInlineTextoTemplate(texto, devolverFoco: true);
                e.Handled = true;
                return true;
            }

            return true;
        }

        private void TemplateTextInlineEditor_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox || textBox.DataContext is not ProjectSheetTemplateTextViewModel texto || !texto.IsEditingInline)
                return;

            textBox.Focus();
            Keyboard.Focus(textBox);
            textBox.SelectAll();
        }

        private void TemplateTextInlineEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox || textBox.DataContext is not ProjectSheetTemplateTextViewModel texto)
                return;

            if (e.Key == Key.Escape)
            {
                CancelarEdicaoInlineTextoTemplate(texto);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                ConfirmarEdicaoInlineTextoTemplate(texto, devolverFoco: true);
                e.Handled = true;
            }
        }

        private void TemplateTextInlineEditor_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is not TextBox textBox || textBox.DataContext is not ProjectSheetTemplateTextViewModel texto || !texto.IsEditingInline)
                return;

            ConfirmarEdicaoInlineTextoTemplate(texto, devolverFoco: false);
        }

        private bool ConfirmarEdicaoInlineTextoTemplate(ProjectSheetTemplateTextViewModel texto, bool devolverFoco)
        {
            if (_textoTemplateInlineCommitEmAndamento)
                return true;

            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null)
                return false;

            Guid textId = texto.Id;
            string novoConteudo = texto.ConteudoEdicao ?? string.Empty;
            bool alterouConteudo = !string.Equals(texto.Conteudo, novoConteudo, StringComparison.Ordinal);

            _textoTemplateInlineCommitEmAndamento = true;

            try
            {
                viewModel.EndTextInlineEditing(textId);

                if (alterouConteudo)
                    _context.MoverTextoDoTipoPrancha.AlterarConteudo(viewModel.Id, textId, novoConteudo);

                viewModel.SelectText(textId);
                AtualizarPainelPropriedadesTemplateSelecionado();
                AtualizarHandlesOverlay();

                if (devolverFoco)
                {
                    Focus();
                    Keyboard.Focus(this);
                }

                return true;
            }
            finally
            {
                _textoTemplateInlineCommitEmAndamento = false;
            }
        }

        private bool CancelarEdicaoInlineTextoTemplate(ProjectSheetTemplateTextViewModel texto)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel == null)
                return false;

            Guid textId = texto.Id;
            bool cancelou = viewModel.CancelTextInlineEditing(textId);

            if (cancelou)
            {
                viewModel.SelectText(textId);
                AtualizarPainelPropriedadesTemplateSelecionado();
                AtualizarHandlesOverlay();
                Focus();
                Keyboard.Focus(this);
            }

            return cancelou;
        }

        private void FocarEditorTextoInlineDeferred(Guid textId)
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Input,
                new Action(() =>
                {
                    TextBox? editor = EncontrarTextBoxInline(TemplatePageContent, textId);

                    if (editor == null)
                        return;

                    editor.Focus();
                    Keyboard.Focus(editor);
                    editor.SelectAll();
                }));
        }

        private static TextBox? EncontrarTextBoxInline(DependencyObject origem, Guid textId)
        {
            int count = VisualTreeHelper.GetChildrenCount(origem);

            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(origem, i);

                if (child is TextBox textBox && textBox.DataContext is ProjectSheetTemplateTextViewModel texto && texto.Id == textId)
                    return textBox;

                TextBox? result = EncontrarTextBoxInline(child, textId);

                if (result != null)
                    return result;
            }

            return null;
        }

        private static bool OrigemEstaDentroDeEditorInline(DependencyObject? origem)
        {
            while (origem != null)
            {
                if (origem is TextBox { DataContext: ProjectSheetTemplateTextViewModel texto } && texto.IsEditingInline)
                    return true;

                origem = VisualTreeHelper.GetParent(origem);
            }

            return false;
        }

        private bool ExcluirTemplateSelecionado()
        {
            return ExcluirTextoSelecionado() || ExcluirLinhaSelecionada() || ExcluirRetanguloSelecionado() || ExcluirCirculoSelecionado();
        }

        private bool ExcluirTextoSelecionado()
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null)
                return false;

            if (!viewModel.TryGetSelectedTextId(out Guid textId))
                return false;

            bool excluiu = _context.ExcluirTextoDoTipoPrancha.Excluir(viewModel.Id, textId);

            if (excluiu)
                viewModel.ClearTextSelection();

            AtualizarPainelPropriedadesTemplateSelecionado();
            AtualizarHandlesOverlay();
            return excluiu;
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

            AtualizarPainelPropriedadesTemplateSelecionado();
            AtualizarHandlesOverlay();
            return excluiu;
        }

        private bool ExcluirRetanguloSelecionado()
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null)
                return false;

            if (!viewModel.TryGetSelectedRectangleId(out Guid rectangleId))
                return false;

            bool excluiu = _context.ExcluirRetanguloDoTipoPrancha.Excluir(viewModel.Id, rectangleId);

            if (excluiu)
                viewModel.ClearRectangleSelection();

            AtualizarPainelPropriedadesTemplateSelecionado();
            AtualizarHandlesOverlay();
            return excluiu;
        }

        private bool ExcluirCirculoSelecionado()
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (_context == null || viewModel == null)
                return false;

            if (!viewModel.TryGetSelectedCircleId(out Guid circleId))
                return false;

            bool excluiu = _context.ExcluirCirculoDoTipoPrancha.Excluir(viewModel.Id, circleId);

            if (excluiu)
                viewModel.ClearCircleSelection();

            AtualizarPainelPropriedadesTemplateSelecionado();
            AtualizarHandlesOverlay();
            return excluiu;
        }

        private ProjectSheetTypeViewModel? ObterViewModelAtivo()
        {
            if (DataContext is ProjectSheetTypeViewModel viewModel)
            {
                DefinirComoSuperficieAtiva(viewModel);
                return viewModel;
            }

            return _context?.ProjectSheetTypeViewModelAtivo;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ProjectSheetTypeViewModel viewModel)
            {
                DefinirComoSuperficieAtiva(viewModel);
                CenterSheetInViewportDeferred();
            }

            AtualizarHandlesOverlay();
        }

        private void AtualizarHandlesOverlay()
        {
            ObterViewModelAtivo();
        }

        private void DefinirComoSuperficieAtiva(ProjectSheetTypeViewModel viewModel)
        {
            if (_context == null)
                return;

            if (!ReferenceEquals(_context.ProjectSheetTypeViewModelAtivo, viewModel))
                _context.ProjectSheetTypeViewModelAtivo = viewModel;

            if (_context.Editor.SuperficieAtiva != EditorSurfaceKind.ProjectSheetType)
                _context.Editor.SuperficieAtiva = EditorSurfaceKind.ProjectSheetType;
        }

        private void AtualizarPainelPropriedadesTemplateSelecionado()
        {
            if (_context == null)
                return;

            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (viewModel == null)
                return;

            if (viewModel.TryGetSelectedTextId(out Guid textId) &&
                viewModel.TryGetText(textId, out ProjectSheetTemplateText? texto) &&
                texto != null)
            {
                _context.Editor.ElementoSelecionado = new ProjectSheetTemplateTextPropertiesViewModel(
                    viewModel.Tipo,
                    texto,
                    _context.MoverTextoDoTipoPrancha,
                    _context.Types,
                    _context.TypePropertiesDialogs);
                return;
            }

            if (viewModel.TryGetSelectedLineId(out Guid lineId) &&
                viewModel.TryGetLine(lineId, out ProjectSheetTemplateLine? linha) &&
                linha != null)
            {
                _context.Editor.ElementoSelecionado = new ProjectSheetTemplateLinePropertiesViewModel(
                    viewModel.Tipo,
                    linha,
                    _context.MoverLinhaDoTipoPrancha,
                    _context.Types,
                    _context.TypePropertiesDialogs);
                return;
            }

            if (viewModel.TryGetSelectedRectangleId(out Guid rectangleId) &&
                viewModel.TryGetRectangle(rectangleId, out ProjectSheetTemplateRectangle? retangulo) &&
                retangulo != null)
            {
                _context.Editor.ElementoSelecionado = new ProjectSheetTemplateRectanglePropertiesViewModel(
                    viewModel.Tipo,
                    retangulo,
                    _context.MoverRetanguloDoTipoPrancha,
                    _context.Types,
                    _context.TypePropertiesDialogs);
                return;
            }

            if (viewModel.TryGetSelectedCircleId(out Guid circleId) &&
                viewModel.TryGetCircle(circleId, out ProjectSheetTemplateCircle? circulo) &&
                circulo != null)
            {
                _context.Editor.ElementoSelecionado = new ProjectSheetTemplateCirclePropertiesViewModel(
                    viewModel.Tipo,
                    circulo,
                    _context.MoverCirculoDoTipoPrancha,
                    _context.Types,
                    _context.TypePropertiesDialogs);
                return;
            }

            _context.Editor.ElementoSelecionado = new ProjectSheetTypePropertiesViewModel(
                _context.Document,
                viewModel.Tipo,
                _context.RenomearItemProjeto,
                _context.EditarPropriedadesTipoPrancha);
        }

        private void CenterSheetInViewportDeferred()
        {
            int version = ++_centerSheetRequestVersion;
            QueueCenterSheetInViewport(version, 0, DispatcherPriority.Loaded);
        }

        private void QueueCenterSheetInViewport(int version, int attempt, DispatcherPriority priority)
        {
            Dispatcher.BeginInvoke(
                priority,
                new Action(() =>
                {
                    if (version != _centerSheetRequestVersion)
                        return;

                    bool centered = CenterSheetInViewport(updateLayout: true);

                    if (version != _centerSheetRequestVersion)
                        return;

                    if (!centered && attempt < CenterSheetMaxAttempts)
                    {
                        QueueCenterSheetInViewport(version, attempt + 1, DispatcherPriority.ContextIdle);
                        return;
                    }

                    if (centered && attempt < CenterSheetSettlingAttempts)
                        QueueCenterSheetInViewport(version, attempt + 1, DispatcherPriority.Render);
                }));
        }

        private bool CenterSheetInViewport(bool updateLayout = false)
        {
            ProjectSheetTypeViewModel? viewModel = ObterViewModelAtivo();

            if (!IsLoaded || viewModel == null)
                return false;

            if (updateLayout)
            {
                SheetTypeSurface.UpdateLayout();
                TemplatePageBorder.UpdateLayout();
                SheetTypeScrollViewer.UpdateLayout();
            }

            double viewportWidth = ResolveViewportDimension(SheetTypeScrollViewer.ViewportWidth, SheetTypeScrollViewer.ActualWidth);
            double viewportHeight = ResolveViewportDimension(SheetTypeScrollViewer.ViewportHeight, SheetTypeScrollViewer.ActualHeight);

            if (viewportWidth <= 0 || viewportHeight <= 0)
                return false;

            double extentWidth = ResolveExtentDimension(SheetTypeScrollViewer.ExtentWidth, SheetTypeSurface.ActualWidth, viewModel.WorkspaceWidth);
            double extentHeight = ResolveExtentDimension(SheetTypeScrollViewer.ExtentHeight, SheetTypeSurface.ActualHeight, viewModel.WorkspaceHeight);
            double expectedScrollableWidth = Math.Max(0.0, extentWidth - viewportWidth);
            double expectedScrollableHeight = Math.Max(0.0, extentHeight - viewportHeight);

            if (!ScrollableDimensionReady(SheetTypeScrollViewer.ScrollableWidth, expectedScrollableWidth) ||
                !ScrollableDimensionReady(SheetTypeScrollViewer.ScrollableHeight, expectedScrollableHeight))
                return false;

            double sheetCenterX = viewModel.SheetOriginOffsetX + viewModel.SheetWidth / 2.0;
            double sheetCenterY = viewModel.SheetOriginOffsetY + viewModel.SheetHeight / 2.0;
            double targetHorizontal = sheetCenterX - viewportWidth / 2.0;
            double targetVertical = sheetCenterY - viewportHeight / 2.0;
            double maximumHorizontal = Math.Max(SheetTypeScrollViewer.ScrollableWidth, expectedScrollableWidth);
            double maximumVertical = Math.Max(SheetTypeScrollViewer.ScrollableHeight, expectedScrollableHeight);
            double normalizedHorizontal = NormalizeScrollOffset(targetHorizontal, maximumHorizontal);
            double normalizedVertical = NormalizeScrollOffset(targetVertical, maximumVertical);

            SheetTypeScrollViewer.ScrollToHorizontalOffset(normalizedHorizontal);
            SheetTypeScrollViewer.ScrollToVerticalOffset(normalizedVertical);
            SheetTypeScrollViewer.UpdateLayout();
            return true;
        }

        private static double ResolveViewportDimension(double viewport, double actual)
        {
            bool viewportValido = ValorPositivo(viewport);
            bool actualValido = ValorPositivo(actual);

            if (!viewportValido)
                return actualValido ? actual : 0.0;

            if (actualValido && viewport > actual + LayoutTolerance)
                return actual;

            return viewport;
        }

        private static double ResolveExtentDimension(double extent, double actual, double fallback)
        {
            double resolved = 0.0;

            if (ValorPositivo(fallback))
                resolved = Math.Max(resolved, fallback);

            if (ValorPositivo(actual))
                resolved = Math.Max(resolved, actual);

            if (ValorPositivo(extent))
                resolved = Math.Max(resolved, extent);

            return resolved;
        }

        private static bool ScrollableDimensionReady(double scrollable, double expectedScrollable)
        {
            if (expectedScrollable <= LayoutTolerance)
                return true;

            return ValorFinito(scrollable) && scrollable + LayoutTolerance >= expectedScrollable;
        }

        private static double NormalizeScrollOffset(double value, double maximum)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
                return 0;

            if (double.IsNaN(maximum) || double.IsInfinity(maximum) || maximum < 0)
                return value;

            return Math.Min(value, maximum);
        }

        private static double CalcularLarguraTextoTemplate(double originalX, Point position)
        {
            return Math.Max(ProjectSheetTemplateText.MinBoxWidth, position.X - originalX);
        }

        private static double CalcularRaioCirculoTemplate(double centerX, double centerY, Point position)
        {
            double dx = position.X - centerX;
            double dy = position.Y - centerY;
            return Math.Max(ProjectSheetTemplateCircle.MinRadius, Math.Sqrt(dx * dx + dy * dy));
        }

        private static Rect CalcularGeometriaRetanguloTemplate(
            double originalX,
            double originalY,
            double originalLargura,
            double originalAltura,
            RetanguloResizeHandleKind kind,
            Point position)
        {
            double left = originalX;
            double top = originalY;
            double right = originalX + originalLargura;
            double bottom = originalY + originalAltura;
            double minDimension = ProjectSheetTemplateRectangle.MinDimension;

            if (kind is RetanguloResizeHandleKind.TopLeft or RetanguloResizeHandleKind.Left or RetanguloResizeHandleKind.BottomLeft)
                left = Math.Min(position.X, right - minDimension);

            if (kind is RetanguloResizeHandleKind.TopRight or RetanguloResizeHandleKind.Right or RetanguloResizeHandleKind.BottomRight)
                right = Math.Max(position.X, left + minDimension);

            if (kind is RetanguloResizeHandleKind.TopLeft or RetanguloResizeHandleKind.Top or RetanguloResizeHandleKind.TopRight)
                top = Math.Min(position.Y, bottom - minDimension);

            if (kind is RetanguloResizeHandleKind.BottomLeft or RetanguloResizeHandleKind.Bottom or RetanguloResizeHandleKind.BottomRight)
                bottom = Math.Max(position.Y, top + minDimension);

            return new Rect(left, top, Math.Max(minDimension, right - left), Math.Max(minDimension, bottom - top));
        }

        private Point ObterPontoLocalFolha(MouseEventArgs e)
        {
            return e.GetPosition(TemplatePageContent);
        }

        private static bool PontoValido(Point point)
        {
            return ValorFinito(point.X) && ValorFinito(point.Y);
        }

        private static bool ValorPositivo(double value)
        {
            return ValorFinito(value) && value > 0;
        }

        private static bool ValorFinito(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static ToolInputState CriarInputState(MouseEventArgs e, Point localPosition, MouseButton? button = null, int clickCount = 0)
        {
            return new ToolInputState(Keyboard.Modifiers, button, clickCount, localPosition, e.GetPosition(null));
        }
    }
}