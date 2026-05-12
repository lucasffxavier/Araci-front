using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Araci.Applications.Editar.Deletar;
using Araci.Applications.Editar.Mover;
using Araci.Applications.Editar.Selecionar;

using Araci.Services;
using Araci.ViewModels;

namespace Araci.Controls.Base
{
    public abstract class ElementoControlBase : UserControl
    {
        private readonly DragService _drag;

        protected ElementoControlBase()
        {
            Loaded += OnLoaded;
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            DataContextChanged += OnDataContextChanged;

            Cursor = Cursors.Hand;

            _drag = new DragService(this);
            _drag.DragDelta += OnDragDelta;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AtualizarPosicao();
            AtualizarVisual();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not ElementoViewModel vm)
                return;

            if (AppServices.Tools.FerramentaAtual is SelecionarTool ||
                AppServices.Tools.FerramentaAtual is MoverTool)
            {
                SelectionService.Selecionar(vm);
                return;
            }

            if (AppServices.Tools.FerramentaAtual is DeletarTool)
            {
                AppServices.Viewport?.RemoverElemento(vm);
                SelectionService.Limpar();
            }
        }

        private void OnDragDelta(Vector delta)
        {
            if (DataContext is not ElementoViewModel vm)
                return;

            var ferramenta = AppServices.Tools.FerramentaAtual;

            if (ferramenta is SelecionarTool)
            {
                MoveService.Mover(this, vm, delta);
            }
            else if (ferramenta is MoverTool)
            {
                MoveService.Mover(this, vm, delta);
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ElementoViewModel antigo)
                antigo.PropertyChanged -= OnViewModelPropertyChanged;

            if (e.NewValue is ElementoViewModel novo)
            {
                novo.PropertyChanged += OnViewModelPropertyChanged;
                AtualizarVisual();
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ElementoViewModel.IsSelecionado))
                AtualizarVisual();
        }

        private void AtualizarVisual()
        {
            if (DataContext is not ElementoViewModel vm)
                return;

            if (vm.IsSelecionado)
                AtualizarVisualSelecionado();
            else
                AtualizarVisualNormal();
        }

        protected abstract void AtualizarVisualSelecionado();
        protected abstract void AtualizarVisualNormal();

        protected void AtualizarPosicao()
        {
            if (DataContext is not ElementoViewModel vm)
                return;

            Canvas.SetLeft(this, vm.X);
            Canvas.SetTop(this, vm.Y);
        }

        protected SolidColorBrush CriarBrush(string hex)
        {
            return new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(hex));
        }
    }
}