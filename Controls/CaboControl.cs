using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Araci.Services;
using Araci.ViewModels;

namespace Araci.Controls
{
    public class CaboControl : UserControl
    {
        private readonly Line _line;
        private readonly Canvas _canvas;
        private readonly DragService _drag;

        public CaboControl()
        {
            Cursor = Cursors.Hand;

            ClipToBounds = true;
            SnapsToDevicePixels = true;
            UseLayoutRounding = true;

            _line = new Line
            {
                Stroke = Brushes.Lime,
                StrokeThickness = 4,
                SnapsToDevicePixels = true
            };

            _canvas = new Canvas
            {
                ClipToBounds = true,
                SnapsToDevicePixels = true
            };

            _canvas.Children.Add(_line);

            Content = _canvas;

            DataContextChanged += OnDataContextChanged;

            _drag = new DragService(this);
        }

        private void OnDataContextChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is CaboViewModel antigo)
                antigo.PropertyChanged -= OnVmChanged;

            if (e.NewValue is CaboViewModel novo)
            {
                novo.PropertyChanged += OnVmChanged;
                Atualizar(novo);
            }
        }

        private void OnVmChanged(
            object? sender,
            PropertyChangedEventArgs e)
        {
            if (sender is CaboViewModel vm)
                Atualizar(vm);
        }

        private void Atualizar(CaboViewModel vm)
        {
            double x1 = vm.X;
            double y1 = vm.Y;
            double x2 = vm.X2;
            double y2 = vm.Y2;

            double minX = Math.Min(x1, x2);
            double minY = Math.Min(y1, y2);

            double largura = Math.Abs(x2 - x1);
            double altura = Math.Abs(y2 - y1);

            // margem para o stroke
            largura = Math.Max(4, largura + 4);
            altura = Math.Max(4, altura + 4);

            Width = largura;
            Height = altura;

            _canvas.Width = largura;
            _canvas.Height = altura;

            _line.X1 = x1 - minX + 2;
            _line.Y1 = y1 - minY + 2;
            _line.X2 = x2 - minX + 2;
            _line.Y2 = y2 - minY + 2;
        }
    }
}