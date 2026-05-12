using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Araci.Applications.Editar.Deletar;
using Araci.Applications.Editar.Mover;
using Araci.Applications.Editar.Selecionar;

using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Controls
{
    public class CaboControl : UserControl
    {
        private readonly Line _line;
        private readonly Canvas _canvas;
        private readonly DragService _drag;

        public Cabo? Cabo
        {
            get
            {
                if (DataContext is CaboViewModel vm)
                    return (Cabo)vm.Modelo;

                return null;
            }
        }

        public CaboControl()
        {
            Cursor = Cursors.Hand;

            _line = new Line
            {
                Stroke = Brushes.Lime,
                StrokeThickness = 4,
                SnapsToDevicePixels = true
            };

            _canvas = new Canvas();
            _canvas.Children.Add(_line);

            Content = _canvas;

            MouseLeftButtonDown += OnMouseLeftButtonDown;
            DataContextChanged += OnDataContextChanged;

            _drag = new DragService(this);
            _drag.DragDelta += OnDragDelta; // 🔥 CORREÇÃO
        }

        private void OnDragDelta(Vector delta)
        {
            if (DataContext is not CaboViewModel vm)
                return;

            var ferramenta = AppServices.Tools.FerramentaAtual;

            if (ferramenta is SelecionarTool)
            {
                vm.X += delta.X;
                vm.Y += delta.Y;
                vm.X2 += delta.X;
                vm.Y2 += delta.Y;
            }
            else if (ferramenta is MoverTool)
            {
                MoveService.MoverCabo(vm, delta); // ✅ CORRETO
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not CaboViewModel vm)
                return;

            if (AppServices.Tools.FerramentaAtual is SelecionarTool)
            {
                SelectionService.Selecionar(vm);
                return;
            }

            if (AppServices.Tools.FerramentaAtual is MoverTool)
            {
                SelectionService.Selecionar(vm);
                return;
            }

            if (AppServices.Tools.FerramentaAtual is DeletarTool)
            {
                AppServices.Viewport?.RemoverElemento(vm);
                SelectionService.Limpar();
                return;
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is CaboViewModel antigo)
                antigo.PropertyChanged -= OnViewModelPropertyChanged;

            if (e.NewValue is CaboViewModel novo)
            {
                novo.PropertyChanged += OnViewModelPropertyChanged;
                AtualizarGeometria(novo);
                AtualizarVisual(novo);
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not CaboViewModel vm)
                return;

            AtualizarGeometria(vm);
            AtualizarVisual(vm);
        }

        private void AtualizarVisual(CaboViewModel vm)
        {
            if (vm.IsSelecionado)
            {
                _line.Stroke = Brushes.DeepSkyBlue;
                _line.StrokeThickness = 6;
            }
            else
            {
                _line.Stroke = Brushes.Lime;
                _line.StrokeThickness = 4;
            }
        }

        private void AtualizarGeometria(CaboViewModel vm)
        {
            double minX = Math.Min(vm.X, vm.X2);
            double minY = Math.Min(vm.Y, vm.Y2);

            double largura = Math.Abs(vm.X2 - vm.X);
            double altura = Math.Abs(vm.Y2 - vm.Y);

            Width = largura + 10;
            Height = altura + 10;

            _line.X1 = vm.X - minX;
            _line.Y1 = vm.Y - minY;
            _line.X2 = vm.X2 - minX;
            _line.Y2 = vm.Y2 - minY;
        }
    }
}