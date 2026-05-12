using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using Araci.Controls.Base;
using Araci.ViewModels;

namespace Araci.Controls
{
    public class CaboControl : ElementoControlBase
    {
        private readonly Line _line;
        private readonly Canvas _canvas;

        public CaboControl()
        {
            ClipToBounds = true;

            _line = new Line
            {
                Stroke = Brushes.Lime,
                StrokeThickness = 4
            };

            _canvas = new Canvas();

            _canvas.Children.Add(_line);

            Content = _canvas;

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(
            object sender,
            System.Windows.DependencyPropertyChangedEventArgs e)
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
            {
                Atualizar(vm);
            }
        }

        private void Atualizar(CaboViewModel vm)
        {
            double x1 = vm.X;
            double y1 = vm.Y;

            double x2 = vm.X2;
            double y2 = vm.Y2;

            double minX = Math.Min(x1, x2);
            double minY = Math.Min(y1, y2);

            double largura =
                Math.Abs(x2 - x1) + 4;

            double altura =
                Math.Abs(y2 - y1) + 4;

            Width = largura;
            Height = altura;

            _canvas.Width = largura;
            _canvas.Height = altura;

            _line.X1 = x1 - minX + 2;
            _line.Y1 = y1 - minY + 2;

            _line.X2 = x2 - minX + 2;
            _line.Y2 = y2 - minY + 2;
        }

        protected override void AtualizarVisualSelecionado()
        {
            _line.Stroke = Brushes.DeepSkyBlue;
            _line.StrokeThickness = 6;
        }

        protected override void AtualizarVisualNormal()
        {
            _line.Stroke = Brushes.Lime;
            _line.StrokeThickness = 4;
        }
    }
}