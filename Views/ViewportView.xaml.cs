using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Araci.Applications.Editar.Base;
using Araci.Core.Commands;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Views
{
    public partial class ViewportView : UserControl
    {
        private readonly MatrixTransform _cameraTransform = new();
        private ViewportViewModel? _viewportViewModel;
        private EditorContext? _context;
        private bool _commitInlineTextInProgress;
        private bool _cancelInlineTextInProgress;
        private TextoAnotativoViewModel? _textoWidthResizeAtivo;
        private TextoWidthResizeSide _textoWidthResizeSide = TextoWidthResizeSide.Right;
        private double _textoWidthResizeXInicial;
        private double _textoWidthResizeLarguraInicial;
        private double _textoWidthResizeDeltaAcumulado;

        public ViewportView()
        {
            InitializeComponent();
        }

        public void Inicializar(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _viewportViewModel = _context.CriarViewportViewModel();
            DataContext = _viewportViewModel;
            _context.InicializarViewport(_viewportViewModel);
            ConfigurarCamera();
            Unloaded += OnUnloaded;
        }

        private bool IsTextoWidthResizing => _textoWidthResizeAtivo != null;

        private void ConfigurarCamera()
        {
            if (_context?.Viewport == null)
                return;

            WorldLayer.RenderTransform = _cameraTransform;
            AlignmentGuideLayer.RenderTransform = _cameraTransform;
            SelectionLayer.RenderTransform = _cameraTransform;
            CableVertexHandleLayer.RenderTransform = _cameraTransform;
            CirculoResizeHandleLayer.RenderTransform = _cameraTransform;
            RetanguloResizeHandleLayer.RenderTransform = _cameraTransform;
            LinhaEndpointInsertionSnapLayer.RenderTransform = _cameraTransform;
            LinhaEndpointHandleLayer.RenderTransform = _cameraTransform;
            TerminalSnapLayer.RenderTransform = _cameraTransform;
            _context.Viewport.Camera.PropertyChanged += OnCameraChanged;
            AtualizarCameraTransform();
        }

        private void OnCameraChanged(object? sender, PropertyChangedEventArgs e)
        {
            AtualizarCameraTransform();
        }

        private void AtualizarCameraTransform()
        {
            if (_context?.Viewport == null)
                return;

            var camera = _context.Viewport.Camera;
            _cameraTransform.Matrix = new Matrix(camera.Zoom, 0, 0, camera.Zoom, camera.Offset.X, camera.Offset.Y);
            _viewportViewModel?.AtualizarZoomVisual(camera.Zoom);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Focus();
            Keyboard.Focus(this);
            AtualizarViewport();
            SizeChanged += (_, _) => AtualizarViewport();
        }

        private void AtualizarViewport()
        {
            if (_context?.Viewport == null)
                return;

            double largura = ActualWidth;
            double altura = ActualHeight;

            if (RootBorder != null)
            {
                largura -= RootBorder.BorderThickness.Left + RootBorder.BorderThickness.Right;
                altura -= RootBorder.BorderThickness.Top + RootBorder.BorderThickness.Bottom;
            }

            largura = Math.Max(0, largura);
            altura = Math.Max(0, altura);
            _context.Viewport.AtualizarTamanho(new Size(largura, altura));
        }

        private Point GetWorldPos(MouseEventArgs e)
        {
            Point screen = e.GetPosition(this);
            return _context?.Viewport?.ScreenToWorld(screen) ?? screen;
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_context == null)
                return;

            if (OrigemEstaDentroDeTextoWidthHandle(e.OriginalSource as DependencyObject))
                return;

            if (_context.Navigation.TryHandleMiddleDoubleClick(e))
            {
                AtualizarCursorNavegacao();
                LiberarCapturaMouse();
                e.Handled = true;
                return;
            }

            if (_context.Navigation.TryBeginMiddlePan(e, this))
            {
                Focus();
                Keyboard.Focus(this);
                AtualizarCursorNavegacao();
                CaptureMouse();
                e.Handled = true;
            }
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_context == null)
                return;

            if (OrigemEstaDentroDeTextoWidthHandle(e.OriginalSource as DependencyObject))
                return;

            if (_context.Navigation.TryBeginSpaceLeftPan(e, this))
            {
                Focus();
                Keyboard.Focus(this);
                AtualizarCursorNavegacao();
                CaptureMouse();
                e.Handled = true;
                return;
            }

            if (_context.Navigation.IsPanning)
            {
                e.Handled = true;
                return;
            }

            bool clicouDentroEditorInline = OrigemEstaDentroDeEditorInline(e.OriginalSource as DependencyObject);

            if (ExisteEdicaoInlineAtiva())
            {
                if (clicouDentroEditorInline)
                    return;

                ConfirmarEdicaoInlineAtiva();
            }

            Point worldPosition = GetWorldPos(e);
            ElementoViewModel? vm = ResolverElementoClicado(e.OriginalSource as DependencyObject, worldPosition);

            if (e.ClickCount >= 2 && vm is TextoAnotativoViewModel texto)
            {
                IniciarEdicaoInlineTexto(texto);
                e.Handled = true;
                return;
            }

            Focus();
            Keyboard.Focus(this);
            ToolInputState inputState = CriarInputState(e, e.ChangedButton, e.ClickCount);
            _context.Input.MouseDown(vm, worldPosition, inputState);
            AtualizarCursorInteracao(worldPosition);
            CaptureMouse();
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_context?.Navigation.TryUpdatePan(e, this) == true)
            {
                AtualizarCursorNavegacao();
                e.Handled = true;
                return;
            }

            if (_context == null)
                return;

            if (IsTextoWidthResizing)
            {
                Cursor = Cursors.SizeWE;
                return;
            }

            if (ExisteEdicaoInlineAtiva())
            {
                AtualizarCursorInteracao(GetWorldPos(e));
                return;
            }

            Point worldPosition = GetWorldPos(e);
            _context.Input.MouseMove(worldPosition, CriarInputState(e));
            AtualizarCursorInteracao(worldPosition);
        }

        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_context?.Navigation.TryEndMiddlePan(e) != true)
                return;

            AtualizarCursorNavegacao();
            LiberarCapturaMouse();
            e.Handled = true;
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsTextoWidthResizing)
                return;

            if (_context?.Navigation.TryEndSpaceLeftPan(e) == true)
            {
                AtualizarCursorNavegacao();
                LiberarCapturaMouse();
                e.Handled = true;
                return;
            }

            if (_context?.Navigation.ConsumeSuppressNextLeftButtonUp() == true)
            {
                e.Handled = true;
                return;
            }

            if (_context?.Navigation.IsPanning == true)
            {
                e.Handled = true;
                return;
            }

            if (ExisteEdicaoInlineAtiva())
            {
                LiberarCapturaMouse();
                e.Handled = true;
                return;
            }

            if (_context != null)
            {
                Point worldPosition = GetWorldPos(e);
                _context.Input.MouseUp(worldPosition, CriarInputState(e, e.ChangedButton, e.ClickCount));
                AtualizarCursorInteracao(worldPosition);
            }

            LiberarCapturaMouse();
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_context?.Navigation.TryHandleMouseWheel(e, this) != true)
                return;

            e.Handled = true;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource is TextBox textBox && textBox.DataContext is TextoAnotativoViewModel)
                return;

            if (ExisteEdicaoInlineAtiva())
            {
                if (e.Key == Key.Escape)
                {
                    CancelarEdicaoInlineAtiva();
                    Focus();
                    Keyboard.Focus(this);
                    e.Handled = true;
                }

                return;
            }

            if (_context != null && e.Key == Key.Space && _context.Input.KeyDown(e.Key))
            {
                e.Handled = true;
                return;
            }

            if (_context?.Navigation.TryHandleKeyDown(e) == true)
            {
                AtualizarCursorNavegacao();
                e.Handled = true;
                return;
            }

            if (_context == null)
                return;

            e.Handled = _context.Input.KeyDown(e.Key);

            if (e.Handled && e.Key == Key.Escape)
                LiberarCapturaMouse();
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (_context?.Navigation.TryHandleKeyUp(e) != true)
                return;

            AtualizarCursorNavegacao();
            LiberarCapturaMouse();
            e.Handled = true;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (ExisteEdicaoInlineAtiva() || IsTextoWidthResizing)
                return;

            _context?.Hover.Clear();
            _context?.TerminalSnap.Limpar();
            _context?.AlignmentGuides.Limpar();
            _context?.LinhaEndpointEdit.LimparSnapInsercao();
            _context?.Navigation.CancelPan();
            AtualizarCursorNavegacao();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ConfirmarEdicaoInlineAtiva();
            CancelarResizeLarguraTexto();
            _context?.Navigation.Reset();
            _context?.AlignmentGuides.Limpar();
            _context?.LinhaEndpointEdit.LimparSnapInsercao();
            LiberarCapturaMouse();
            Cursor = Cursors.Arrow;

            if (_context?.Viewport != null)
                _context.Viewport.Camera.PropertyChanged -= OnCameraChanged;
        }

        private void AtualizarCursorInteracao(Point worldPosition)
        {
            if (IsTextoWidthResizing)
            {
                Cursor = Cursors.SizeWE;
                return;
            }

            if (ExisteEdicaoInlineAtiva())
            {
                Cursor = Cursors.IBeam;
                return;
            }

            if (_context?.Navigation.IsPanning == true || _context?.Navigation.IsSpacePressed == true)
            {
                Cursor = Cursors.ScrollAll;
                return;
            }

            Cursor = _context?.BarraResize.GetCursor(worldPosition) ?? Cursors.Arrow;
        }

        private void AtualizarCursorNavegacao()
        {
            if (_context?.Navigation.IsPanning == true || _context?.Navigation.IsSpacePressed == true)
                Cursor = Cursors.ScrollAll;
            else
                Cursor = Cursors.Arrow;
        }

        private void LiberarCapturaMouse()
        {
            if (IsMouseCaptured)
                ReleaseMouseCapture();
        }

        private ToolInputState CriarInputState(MouseEventArgs e, MouseButton? button = null, int clickCount = 0)
        {
            Point screenPosition = e.GetPosition(this);
            Point worldPosition = _context?.Viewport?.ScreenToWorld(screenPosition) ?? screenPosition;
            return new ToolInputState(Keyboard.Modifiers, button, clickCount, worldPosition, screenPosition);
        }

        private ElementoViewModel? ResolverElementoClicado(DependencyObject? origem, Point worldPosition)
        {
            ElementoViewModel? visual = EncontrarElemento(origem);

            if (visual is not RetanguloAnotativoViewModel && visual is not CirculoAnotativoViewModel)
                return visual;

            return _context?.SceneQueries.HitTest(worldPosition)?.Elemento;
        }

        private void IniciarEdicaoInlineTexto(TextoAnotativoViewModel texto)
        {
            if (_context == null)
                return;

            ConfirmarEdicaoInlineAtiva();
            _context.Selection.Selecionar(texto);
            _context.Hover.Clear();
            texto.IniciarEdicaoInline();
            LiberarCapturaMouse();
            Dispatcher.BeginInvoke(new Action(() => FocarTextBoxInline(texto)));
        }

        private void FocarTextBoxInline(TextoAnotativoViewModel texto)
        {
            TextBox? textBox = EncontrarTextBoxInline(WorldLayer, texto);

            if (textBox == null || !texto.IsEditingInline)
                return;

            textBox.Focus();
            Keyboard.Focus(textBox);
            textBox.SelectAll();
        }

        private void OnInlineTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is TextoAnotativoViewModel texto && texto.IsEditingInline)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (!texto.IsEditingInline)
                        return;

                    textBox.Focus();
                    Keyboard.Focus(textBox);
                    textBox.SelectAll();
                }));
            }
        }


        private void OnInlineTextBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not TextBox textBox || textBox.DataContext is not TextoAnotativoViewModel texto || !texto.IsEditingInline)
                return;

            textBox.SelectAll();
            e.Handled = true;
        }

        private void OnInlineTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox || textBox.DataContext is not TextoAnotativoViewModel texto)
                return;

            if (e.Key == Key.Enter && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                return;

            if (e.Key == Key.Enter)
            {
                ConfirmarEdicaoInlineTexto(texto);
                Focus();
                Keyboard.Focus(this);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape)
            {
                CancelarEdicaoInlineTexto(texto);
                Focus();
                Keyboard.Focus(this);
                e.Handled = true;
            }
        }

        private void OnInlineTextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is not TextBox { DataContext: TextoAnotativoViewModel texto })
                return;

            if (_cancelInlineTextInProgress || _commitInlineTextInProgress || !texto.IsEditingInline)
                return;

            if (e.NewFocus is DependencyObject novoFoco && OrigemEstaDentroDeEditorInline(novoFoco))
                return;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!_cancelInlineTextInProgress && !_commitInlineTextInProgress && texto.IsEditingInline)
                    ConfirmarEdicaoInlineTexto(texto);
            }));
        }

        private void OnTextoWidthHandleDragStarted(object sender, DragStartedEventArgs e)
        {
            if (sender is not Thumb { DataContext: TextoAnotativoViewModel texto } thumb || _context == null)
                return;

            ConfirmarEdicaoInlineAtiva();
            _context.Selection.Selecionar(texto);
            _context.Hover.Clear();
            _textoWidthResizeAtivo = texto;
            _textoWidthResizeSide = ObterTextoResizeSide(thumb);
            _textoWidthResizeXInicial = texto.X;
            _textoWidthResizeLarguraInicial = texto.LarguraCaixa;
            _textoWidthResizeDeltaAcumulado = 0;
            Cursor = Cursors.SizeWE;
            e.Handled = true;
        }

        private void OnTextoWidthHandleDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (_textoWidthResizeAtivo == null)
                return;

            _textoWidthResizeDeltaAcumulado += e.HorizontalChange;
            double zoom = _context?.Viewport?.Camera.Zoom ?? 1;
            double deltaWorld = zoom > 0 ? _textoWidthResizeDeltaAcumulado / zoom : _textoWidthResizeDeltaAcumulado;

            if (_textoWidthResizeSide == TextoWidthResizeSide.Left)
                AplicarResizeTextoEsquerda(_textoWidthResizeAtivo, deltaWorld);
            else
                AplicarResizeTextoDireita(_textoWidthResizeAtivo, deltaWorld);

            _context?.SceneQueries.Invalidate();
            _viewportViewModel?.AtualizarViewModel(_textoWidthResizeAtivo.Modelo);
            Cursor = Cursors.SizeWE;
            e.Handled = true;
        }

        private void OnTextoWidthHandleDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (_textoWidthResizeAtivo == null || _context == null)
                return;

            TextoAnotativoViewModel texto = _textoWidthResizeAtivo;
            double xAntes = _textoWidthResizeXInicial;
            double larguraAntes = _textoWidthResizeLarguraInicial;
            double xDepois = texto.X;
            double larguraDepois = texto.LarguraCaixa;

            _textoWidthResizeAtivo = null;
            _textoWidthResizeDeltaAcumulado = 0;
            _textoWidthResizeSide = TextoWidthResizeSide.Right;
            _context.SceneQueries.Invalidate();
            _viewportViewModel?.AtualizarViewModel(texto.Modelo);
            AtualizarCursorNavegacao();

            var itens = new[]
            {
                new BulkPropertyChangeCommand.Item(texto, nameof(TextoAnotativoViewModel.X), xAntes, xDepois),
                new BulkPropertyChangeCommand.Item(texto, nameof(TextoAnotativoViewModel.LarguraCaixa), larguraAntes, larguraDepois)
            };

            var command = new BulkPropertyChangeCommand(itens);

            if (!command.IsEmpty && (Math.Abs(xAntes - xDepois) > 0.000001 || Math.Abs(larguraAntes - larguraDepois) > 0.000001))
                _context.Commands.Execute(command);

            e.Handled = true;
        }

        private void AplicarResizeTextoDireita(TextoAnotativoViewModel texto, double deltaWorld)
        {
            texto.X = _textoWidthResizeXInicial;
            texto.LarguraCaixa = Math.Max(TextoAnotativo.LarguraCaixaMinima, _textoWidthResizeLarguraInicial + deltaWorld);
        }

        private void AplicarResizeTextoEsquerda(TextoAnotativoViewModel texto, double deltaWorld)
        {
            double direitaFixa = _textoWidthResizeXInicial + _textoWidthResizeLarguraInicial;
            double novaLargura = Math.Max(TextoAnotativo.LarguraCaixaMinima, _textoWidthResizeLarguraInicial - deltaWorld);
            double novoX = direitaFixa - novaLargura;
            texto.X = novoX;
            texto.LarguraCaixa = novaLargura;
        }

        private void CancelarResizeLarguraTexto()
        {
            if (_textoWidthResizeAtivo == null)
                return;

            _textoWidthResizeAtivo.X = _textoWidthResizeXInicial;
            _textoWidthResizeAtivo.LarguraCaixa = _textoWidthResizeLarguraInicial;
            _context?.SceneQueries.Invalidate();
            _viewportViewModel?.AtualizarViewModel(_textoWidthResizeAtivo.Modelo);
            _textoWidthResizeAtivo = null;
            _textoWidthResizeDeltaAcumulado = 0;
            _textoWidthResizeSide = TextoWidthResizeSide.Right;
        }

        private bool ExisteEdicaoInlineAtiva()
        {
            return ObterTextoEmEdicaoInline() != null;
        }

        private TextoAnotativoViewModel? ObterTextoEmEdicaoInline()
        {
            if (_viewportViewModel == null)
                return null;

            foreach (ElementoViewModel elemento in _viewportViewModel.Elementos)
            {
                if (elemento is TextoAnotativoViewModel texto && texto.IsEditingInline)
                    return texto;
            }

            return null;
        }

        private void ConfirmarEdicaoInlineAtiva()
        {
            TextoAnotativoViewModel? texto = ObterTextoEmEdicaoInline();

            if (texto != null)
                ConfirmarEdicaoInlineTexto(texto);
        }

        private void CancelarEdicaoInlineAtiva()
        {
            TextoAnotativoViewModel? texto = ObterTextoEmEdicaoInline();

            if (texto != null)
                CancelarEdicaoInlineTexto(texto);
        }

        private void CancelarEdicaoInlineTexto(TextoAnotativoViewModel texto)
        {
            if (!texto.IsEditingInline || _commitInlineTextInProgress || _cancelInlineTextInProgress)
                return;

            _cancelInlineTextInProgress = true;

            try
            {
                texto.CancelarEdicaoInline();
            }
            finally
            {
                _cancelInlineTextInProgress = false;
            }
        }

        private void ConfirmarEdicaoInlineTexto(TextoAnotativoViewModel texto)
        {
            if (_context == null || !texto.IsEditingInline || _commitInlineTextInProgress || _cancelInlineTextInProgress)
                return;

            _commitInlineTextInProgress = true;

            try
            {
                string valorAntes = texto.Conteudo;
                string valorDepois = texto.ConteudoEdicao ?? string.Empty;
                texto.EncerrarEdicaoInline();

                if (string.IsNullOrWhiteSpace(valorDepois))
                {
                    _context.Selection.Selecionar(texto);
                    _context.SafeDelete.DeleteSelection();
                    return;
                }

                if (valorAntes == valorDepois)
                    return;

                var command = new BulkPropertyChangeCommand(new[]
                {
                    new BulkPropertyChangeCommand.Item(texto, nameof(TextoAnotativoViewModel.Conteudo), valorAntes, valorDepois)
                });

                if (!command.IsEmpty)
                    _context.Commands.Execute(command);
            }
            finally
            {
                _commitInlineTextInProgress = false;
                _context.SceneQueries.Invalidate();
                _viewportViewModel?.AtualizarViewModel(texto.Modelo);
            }
        }

        private static TextoWidthResizeSide ObterTextoResizeSide(Thumb thumb)
        {
            return string.Equals(thumb.Tag?.ToString(), "Left", StringComparison.OrdinalIgnoreCase)
                ? TextoWidthResizeSide.Left
                : TextoWidthResizeSide.Right;
        }

        private static bool OrigemEstaDentroDeEditorInline(DependencyObject? origem)
        {
            while (origem != null)
            {
                if (origem is TextBox { DataContext: TextoAnotativoViewModel texto } && texto.IsEditingInline)
                    return true;

                origem = VisualTreeHelper.GetParent(origem);
            }

            return false;
        }

        private static bool OrigemEstaDentroDeTextoWidthHandle(DependencyObject? origem)
        {
            while (origem != null)
            {
                if (origem is Thumb { DataContext: TextoAnotativoViewModel })
                    return true;

                origem = VisualTreeHelper.GetParent(origem);
            }

            return false;
        }

        private static TextBox? EncontrarTextBoxInline(DependencyObject origem, TextoAnotativoViewModel texto)
        {
            int count = VisualTreeHelper.GetChildrenCount(origem);

            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(origem, i);

                if (child is TextBox textBox && ReferenceEquals(textBox.DataContext, texto))
                    return textBox;

                TextBox? result = EncontrarTextBoxInline(child, texto);

                if (result != null)
                    return result;
            }

            return null;
        }

        private static ElementoViewModel? EncontrarElemento(DependencyObject? origem)
        {
            while (origem != null)
            {
                if (origem is FrameworkElement fe && fe.DataContext is ElementoViewModel vm)
                    return vm;

                origem = VisualTreeHelper.GetParent(origem);
            }

            return null;
        }

        private enum TextoWidthResizeSide
        {
            Left,
            Right
        }
    }
}
