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
        private const double LineHitTolerance = 6.0;
        private const double DragThresholdSquared = 9.0;

        private EditorContext? _context;
        private Guid? _linhaTemplateEmArrasteId;
        private Point _linhaTemplateDragStart;
        private bool _linhaTemplateArrastando;

        public ProjectSheetTypeView()
        {
            InitializeComponent();
            DataContext = null;
        }

        public void Inicializar(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private void TemplatePageBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_context == null)
                return;

            Focus();
            Keyboard.Focus(this);
            Point position = e.GetPosition(TemplatePageBorder);

            if (TentarIniciarInteracaoLinhaTemplate(position))
            {
                e.Handled = true;
                return;
            }

            ToolInputState inputState = CriarInputState(e, position, e.ChangedButton, e.ClickCount);
            _context.Tools.FerramentaAtual.OnMouseDown(null, position, inputState);
            TemplatePageBorder.CaptureMouse();
            e.Handled = true;
        }

        private void TemplatePageBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (_context == null)
                return;

            Point position = e.GetPosition(TemplatePageBorder);

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

            if (FinalizarArrasteLinhaTemplate(position))
            {
                if (TemplatePageBorder.IsMouseCaptured)
                    TemplatePageBorder.ReleaseMouseCapture();

                e.Handled = true;
                return;
            }

            _context.Tools.FerramentaAtual.OnMouseUp(position, CriarInputState(e, position, e.ChangedButton, e.ClickCount));

            if (TemplatePageBorder.IsMouseCaptured)
                TemplatePageBorder.ReleaseMouseCapture();

            e.Handled = true;
        }

        private void TemplatePageBorder_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (_linhaTemplateEmArrasteId.HasValue)
                CancelarArrasteLinhaTemplate();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_context == null)
                return;

            if (e.Key == Key.Escape && _linhaTemplateEmArrasteId.HasValue)
            {
                CancelarArrasteLinhaTemplate();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Delete && ExcluirLinhaSelecionada())
            {
                e.Handled = true;
                return;
            }

            if (_context.Input.KeyDown(e.Key))
                e.Handled = true;
        }

        private bool TentarIniciarInteracaoLinhaTemplate(Point position)
        {
            if (_context == null || _context.ProjectSheetTypeViewModelAtivo == null)
                return false;

            ITool ferramentaAtual = _context.Tools.FerramentaAtual;

            if (!string.Equals(ferramentaAtual.Nome, "Selecionar", StringComparison.OrdinalIgnoreCase) || ferramentaAtual.IsBusy)
                return false;

            ProjectSheetTypeViewModel viewModel = _context.ProjectSheetTypeViewModelAtivo;
            bool selecionou = viewModel.SelectLineAt(position, LineHitTolerance);

            if (!selecionou)
            {
                LimparEstadoArrasteLinhaTemplate();
                return true;
            }

            if (!viewModel.TryGetSelectedLineId(out Guid lineId))
                return true;

            _linhaTemplateEmArrasteId = lineId;
            _linhaTemplateDragStart = position;
            _linhaTemplateArrastando = false;
            TemplatePageBorder.CaptureMouse();
            return true;
        }

        private bool AtualizarArrasteLinhaTemplate(Point position, MouseEventArgs e)
        {
            if (_context == null || _context.ProjectSheetTypeViewModelAtivo == null || !_linhaTemplateEmArrasteId.HasValue)
                return false;

            if (e.LeftButton != MouseButtonState.Pressed)
                return false;

            Vector delta = position - _linhaTemplateDragStart;

            if (!_linhaTemplateArrastando && delta.LengthSquared < DragThresholdSquared)
                return true;

            _linhaTemplateArrastando = true;
            _context.ProjectSheetTypeViewModelAtivo.SetLinePreviewOffset(_linhaTemplateEmArrasteId.Value, delta.X, delta.Y);
            return true;
        }

        private bool FinalizarArrasteLinhaTemplate(Point position)
        {
            if (_context == null || _context.ProjectSheetTypeViewModelAtivo == null || !_linhaTemplateEmArrasteId.HasValue)
                return false;

            Guid lineId = _linhaTemplateEmArrasteId.Value;
            bool arrastou = _linhaTemplateArrastando;
            Vector delta = position - _linhaTemplateDragStart;
            ProjectSheetTypeViewModel viewModel = _context.ProjectSheetTypeViewModelAtivo;

            viewModel.ClearLinePreviewOffset(lineId);
            LimparEstadoArrasteLinhaTemplate();

            if (!arrastou)
                return true;

            bool moveu = _context.MoverLinhaDoTipoPrancha.Mover(viewModel.Id, lineId, delta.X, delta.Y);

            if (moveu)
                viewModel.SelectLine(lineId);

            return true;
        }

        private void CancelarArrasteLinhaTemplate()
        {
            if (_context?.ProjectSheetTypeViewModelAtivo != null && _linhaTemplateEmArrasteId.HasValue)
                _context.ProjectSheetTypeViewModelAtivo.ClearLinePreviewOffset(_linhaTemplateEmArrasteId.Value);

            LimparEstadoArrasteLinhaTemplate();
        }

        private void LimparEstadoArrasteLinhaTemplate()
        {
            _linhaTemplateEmArrasteId = null;
            _linhaTemplateArrastando = false;
        }

        private bool ExcluirLinhaSelecionada()
        {
            if (_context == null || _context.ProjectSheetTypeViewModelAtivo == null)
                return false;

            ProjectSheetTypeViewModel viewModel = _context.ProjectSheetTypeViewModelAtivo;

            if (!viewModel.TryGetSelectedLineId(out Guid lineId))
                return false;

            bool excluiu = _context.ExcluirLinhaDoTipoPrancha.Excluir(viewModel.Id, lineId);

            if (excluiu)
                viewModel.ClearLineSelection();

            return excluiu;
        }

        private static ToolInputState CriarInputState(MouseEventArgs e, Point localPosition, MouseButton? button = null, int clickCount = 0)
        {
            return new ToolInputState(Keyboard.Modifiers, button, clickCount, localPosition, e.GetPosition(null));
        }
    }
}