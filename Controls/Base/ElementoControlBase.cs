using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

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
            AtualizarVisual();
        }

        protected virtual void AtualizarVisual()
        {
            if (DataContext is not ElementoViewModel vm)
                return;

            AplicarEstadoVisual(vm);
        }

        protected abstract void AplicarEstadoVisual(
            ElementoViewModel vm);
    }
}