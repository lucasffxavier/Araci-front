using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using Araci.ViewModels;

namespace Araci.Controls.Base
{
    public abstract class ElementoControlBase : UserControl
    {
        private static readonly HashSet<string> _propriedadesVisuais = new()
        {
            nameof(ElementoViewModel.Stroke),
            nameof(ElementoViewModel.StrokeThickness),
            nameof(ElementoViewModel.RenderData),
            nameof(ElementoViewModel.IsSelecionado),
            nameof(ElementoViewModel.IsHover),
            nameof(ElementoViewModel.IsPreview),
            nameof(ElementoViewModel.X),
            nameof(ElementoViewModel.Y),
            nameof(ElementoViewModel.WorldX),
            nameof(ElementoViewModel.WorldY),
            nameof(ElementoViewModel.Largura),
            nameof(ElementoViewModel.Altura),
            nameof(ElementoViewModel.Bounds),
            nameof(ElementoViewModel.Centro),
            "X2",
            "Y2",
        };

        protected ElementoControlBase()
        {
            Cursor = Cursors.Hand;
            Loaded += OnLoaded;
            DataContextChanged += OnDataContextChanged;
            MouseEnter += OnMouseEnter;
            MouseLeave += OnMouseLeave;
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (DataContext is ElementoViewModel vm)
                vm.IsHover = true;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (DataContext is ElementoViewModel vm)
                vm.IsHover = false;
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (UsaBindings)
                return;

            AtualizarVisual();
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ElementoViewModel antigo)
                antigo.PropertyChanged -= OnViewModelPropertyChanged;

            if (e.NewValue is ElementoViewModel novo)
            {
                if (UsaBindings)
                    return;

                novo.PropertyChanged += OnViewModelPropertyChanged;
                AtualizarVisual();
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            bool deveAtualizar =
                string.IsNullOrEmpty(e.PropertyName) ||
                _propriedadesVisuais.Contains(e.PropertyName ?? string.Empty);

            if (!deveAtualizar)
                return;

            AtualizarVisual();
        }

        protected virtual void AtualizarVisual()
        {
            if (DataContext is not ElementoViewModel vm)
                return;

            AplicarEstadoVisual(vm);
        }

        protected virtual bool UsaBindings => false;

        protected virtual void AplicarEstadoVisual(ElementoViewModel vm)
        {
        }
    }
}
