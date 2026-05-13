using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Araci.ViewModels;

namespace Araci.Controls.Base
{
    public abstract class ElementoControlBase
        : UserControl
    {
        protected ElementoControlBase()
        {
            Cursor = Cursors.Hand;

            Loaded += OnLoaded;

            DataContextChanged +=
                OnDataContextChanged;
        }

        private void OnLoaded(
            object sender,
            System.Windows.RoutedEventArgs e)
        {
            AtualizarVisual();
        }

        private void OnDataContextChanged(
            object sender,
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ElementoViewModel antigo)
            {
                antigo.PropertyChanged -=
                    OnViewModelPropertyChanged;
            }

            if (e.NewValue is ElementoViewModel novo)
            {
                novo.PropertyChanged +=
                    OnViewModelPropertyChanged;

                AtualizarVisual();
            }
        }

        private void OnViewModelPropertyChanged(
            object? sender,
            PropertyChangedEventArgs e)
        {
            if (e.PropertyName ==
                nameof(ElementoViewModel.IsSelecionado))
            {
                AtualizarVisual();
            }
        }

        private void AtualizarVisual()
        {
            if (DataContext is not ElementoViewModel vm)
                return;

            if (vm.IsSelecionado)
            {
                AtualizarVisualSelecionado();
            }
            else
            {
                AtualizarVisualNormal();
            }
        }

        protected abstract void AtualizarVisualSelecionado();

        protected abstract void AtualizarVisualNormal();

        protected SolidColorBrush CriarBrush(
            string hex)
        {
            return new SolidColorBrush(
                (Color)ColorConverter
                    .ConvertFromString(hex));
        }
    }
}