// =========================
// ARQUIVO: Controls/Base/ElementoControlBase.cs
// =========================

using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

using Araci.ViewModels;

namespace Araci.Controls.Base
{
    public abstract class ElementoControlBase
        : UserControl
    {
        // =========================
        // PROPRIEDADES VISUAIS
        // =========================

        private static readonly System.Collections.Generic.HashSet<string>
            _propriedadesVisuais = new()
        {
            nameof(ElementoViewModel.Stroke),
            nameof(ElementoViewModel.StrokeThickness),
            nameof(ElementoViewModel.RenderData),
            nameof(ElementoViewModel.IsSelecionado),
            nameof(ElementoViewModel.X),
            nameof(ElementoViewModel.Y),
            nameof(ElementoViewModel.WorldX),
            nameof(ElementoViewModel.WorldY),
            nameof(ElementoViewModel.ScreenX),
            nameof(ElementoViewModel.ScreenY),
            nameof(ElementoViewModel.Largura),
            nameof(ElementoViewModel.Altura),
            nameof(ElementoViewModel.Bounds),
            nameof(ElementoViewModel.Centro),
            nameof(ElementoViewModel.Geometry),

            "X2",
            "Y2",
        };

        // =========================
        // CONSTRUTOR
        // =========================

        protected ElementoControlBase()
        {
            Cursor = Cursors.Hand;

            Loaded += OnLoaded;

            DataContextChanged +=
                OnDataContextChanged;
        }

        // =========================
        // LOADED
        // =========================

        private void OnLoaded(
            object sender,
            System.Windows.RoutedEventArgs e)
        {
            if (UsaBindings)
                return;

            AtualizarVisual();
        }

        // =========================
        // DATA CONTEXT CHANGED
        // =========================

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
                if (UsaBindings)
                    return;

                novo.PropertyChanged +=
                    OnViewModelPropertyChanged;

                AtualizarVisual();
            }
        }

        // =========================
        // PROPERTY CHANGED FILTRADO
        // =========================

        private void OnViewModelPropertyChanged(
            object? sender,
            PropertyChangedEventArgs e)
        {
            // =========================
            // FILTRAGEM DE PROPRIEDADES
            // =========================


            bool deveAtualizar =
                string.IsNullOrEmpty(e.PropertyName) ||
                _propriedadesVisuais.Contains(
                    e.PropertyName ?? string.Empty);

            if (!deveAtualizar)
                return;

            AtualizarVisual();
        }

        // =========================
        // ATUALIZAR VISUAL
        // =========================

        protected virtual void AtualizarVisual()
        {
            if (DataContext is not ElementoViewModel vm)
                return;

            AplicarEstadoVisual(vm);
        }

        protected virtual bool UsaBindings =>
            false;

        // =========================
        // APLICAR ESTADO VISUAL
        // =========================

        protected virtual void AplicarEstadoVisual(
            ElementoViewModel vm)
        {
        }
    }
}
