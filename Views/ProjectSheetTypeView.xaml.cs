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
        private EditorContext? _context;

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
            _context.Tools.FerramentaAtual.OnMouseMove(position, CriarInputState(e, position));
        }

        private void TemplatePageBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_context == null)
                return;

            Point position = e.GetPosition(TemplatePageBorder);
            _context.Tools.FerramentaAtual.OnMouseUp(position, CriarInputState(e, position, e.ChangedButton, e.ClickCount));

            if (TemplatePageBorder.IsMouseCaptured)
                TemplatePageBorder.ReleaseMouseCapture();

            e.Handled = true;
        }

        private void TemplatePageBorder_LostMouseCapture(object sender, MouseEventArgs e)
        {
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_context?.Input.KeyDown(e.Key) == true)
                e.Handled = true;
        }

        private static ToolInputState CriarInputState(MouseEventArgs e, Point localPosition, MouseButton? button = null, int clickCount = 0)
        {
            return new ToolInputState(Keyboard.Modifiers, button, clickCount, localPosition, e.GetPosition(null));
        }
    }
}