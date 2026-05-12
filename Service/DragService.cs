using System;
using System.Windows;
using System.Windows.Input;
using Araci.ViewModels;

namespace Araci.Services
{
    public class DragService
    {
        private readonly UIElement _elemento;

        private bool _arrastando;
        private Point _ultimoPonto;

        public event Action<Vector>? DragDelta;

        public DragService(UIElement elemento)
        {
            _elemento = elemento;

            _elemento.MouseLeftButtonDown += MouseDown;
            _elemento.MouseMove += MouseMove;
            _elemento.MouseLeftButtonUp += MouseUp;
        }

        private FrameworkElement? Ref => AppServices.ViewportReference;

        private void MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_elemento is not FrameworkElement fe)
                return;

            if (fe.DataContext is not ElementoViewModel vm)
                return;

            if (Ref == null)
                return;

            _arrastando = true;

            _ultimoPonto = e.GetPosition(Ref); // 🔥 FIX REAL

            AppServices.Tools.HandleMouseDown(vm, _ultimoPonto);

            _elemento.CaptureMouse();
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (Ref == null)
                return;

            Point pos = e.GetPosition(Ref); // 🔥 FIX REAL

            if (_arrastando)
            {
                Vector delta = pos - _ultimoPonto;
                _ultimoPonto = pos;

                DragDelta?.Invoke(delta);
            }

            AppServices.Tools.HandleMouseMove(pos);
        }

        private void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Ref == null)
                return;

            Point pos = e.GetPosition(Ref); // 🔥 FIX REAL

            _arrastando = false;

            AppServices.Tools.HandleMouseUp(pos);

            _elemento.ReleaseMouseCapture();
        }
    }
}