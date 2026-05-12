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
    public abstract class ElementoControlBase
        : UserControl
    {
        // =========================
        // CONSTRUTOR
        // =========================

        protected ElementoControlBase()
        {
            Loaded += OnLoaded;

            MouseLeftButtonDown +=
                OnMouseLeftButtonDown;

            DataContextChanged +=
                OnDataContextChanged;

            Cursor = Cursors.Hand;
        }

        // =========================
        // LOADED
        // =========================

        private void OnLoaded(
            object sender,
            RoutedEventArgs e)
        {
            AtualizarPosicao();

            AtualizarVisual();
        }

        // =========================
        // MOUSE
        // =========================

        private void OnMouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (DataContext is not ElementoViewModel vm)
                return;

            // =========================
            // SELECIONAR
            // =========================

            if (AppServices.Tools.FerramentaAtual
                is SelecionarTool)
            {
                SelectionService
                    .Selecionar(vm);

                return;
            }

            // =========================
            // MOVER
            // =========================

            if (AppServices.Tools.FerramentaAtual
                is MoverTool)
            {
                SelectionService
                    .Selecionar(vm);

                return;
            }

            // =========================
            // DELETAR
            // =========================

            if (AppServices.Tools.FerramentaAtual
                is DeletarTool)
            {
                AppServices.Viewport
                    ?.RemoverElemento(vm);

                SelectionService
                    .Limpar();

                return;
            }
        }

        // =========================
        // DATACONTEXT
        // =========================

        private void OnDataContextChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
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

        // =========================
        // PROPERTY CHANGED
        // =========================

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

        // =========================
        // VISUAL
        // =========================

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

        // =========================
        // VIRTUAL
        // =========================

        protected abstract void
            AtualizarVisualSelecionado();

        protected abstract void
            AtualizarVisualNormal();

        // =========================
        // POSIÇÃO
        // =========================

        protected void AtualizarPosicao()
        {
            if (DataContext is not ElementoViewModel vm)
                return;

            Canvas.SetLeft(this, vm.X);

            Canvas.SetTop(this, vm.Y);
        }

        // =========================
        // BRUSH
        // =========================

        protected SolidColorBrush CriarBrush(
            string hexadecimal)
        {
            return new SolidColorBrush(
                (Color)ColorConverter
                .ConvertFromString(
                    hexadecimal));
        }
    }
}