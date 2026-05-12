using Araci.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        private void MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!AppServices.Tools.FerramentaAtual.PermiteArrastar)
                return;

            if (_elemento is not FrameworkElement fe)
                return;

            if (fe.DataContext is not ElementoViewModel vm)
                return;

            if (!vm.IsSelecionado)
                return;

            if (VisualTreeHelper.GetParent(fe) is not ContentPresenter presenter)
                return;

            if (VisualTreeHelper.GetParent(presenter) is not Canvas canvas)
                return;

            _arrastando = true;
            _ultimoPonto = e.GetPosition(canvas);

            _elemento.CaptureMouse();
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (!_arrastando)
                return;

            if (_elemento is not FrameworkElement fe)
                return;

            if (VisualTreeHelper.GetParent(fe) is not ContentPresenter presenter)
                return;

            if (VisualTreeHelper.GetParent(presenter) is not Canvas canvas)
                return;

            Point atual = e.GetPosition(canvas);

            Vector delta = atual - _ultimoPonto;

            DragDelta?.Invoke(delta);

            _ultimoPonto = atual;
        }

        private void MouseUp(object sender, MouseButtonEventArgs e)
        {
            _arrastando = false;
            _elemento.ReleaseMouseCapture();
        }
    }
}