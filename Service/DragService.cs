using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        private void MouseDown(object sender, MouseButtonEventArgs e)
        {
            // ✅ VOLTA PARA O CONTRATO CORRETO
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

            // 🔥 HUD apenas no Mover
            if (AppServices.Tools.FerramentaAtual is Applications.Editar.Mover.MoverTool)
            {
                var hud = AppServices.MoveHud;
                hud.Reset();
                hud.Visivel = true;
            }

            _elemento.CaptureMouse();
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (!_arrastando)
                return;

            if (_elemento is not FrameworkElement fe)
                return;

            if (fe.DataContext is not ElementoViewModel vm)
                return;

            if (VisualTreeHelper.GetParent(fe) is not ContentPresenter presenter)
                return;

            if (VisualTreeHelper.GetParent(presenter) is not Canvas canvas)
                return;

            Point atual = e.GetPosition(canvas);
            Vector delta = atual - _ultimoPonto;

            // 🔥 LIMITAÇÃO DA VIEWPORT
            double novoX = vm.X + delta.X;
            double novoY = vm.Y + delta.Y;

            double maxX = canvas.ActualWidth - fe.ActualWidth;
            double maxY = canvas.ActualHeight - fe.ActualHeight;

            novoX = Math.Max(0, Math.Min(maxX, novoX));
            novoY = Math.Max(0, Math.Min(maxY, novoY));

            delta = new Vector(novoX - vm.X, novoY - vm.Y);

            DragDelta?.Invoke(delta);

            _ultimoPonto = atual;

            // 🔥 HUD só no Mover
            if (AppServices.Tools.FerramentaAtual is Applications.Editar.Mover.MoverTool)
            {
                var hud = AppServices.MoveHud;
                hud.X = novoX + 20;
                hud.Y = novoY - 10;
            }
        }

        private void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_arrastando)
                return;

            _arrastando = false;
            _elemento.ReleaseMouseCapture();

            // 🔥 HUD OFF somente se estava ativo
            if (AppServices.Tools.FerramentaAtual is Applications.Editar.Mover.MoverTool)
            {
                AppServices.MoveHud.Visivel = false;
            }
        }
    }
}