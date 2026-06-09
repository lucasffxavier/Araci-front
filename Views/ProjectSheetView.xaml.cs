using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Araci.ViewModels;

namespace Araci.Views
{
    public partial class ProjectSheetView : UserControl
    {
        private ProjectSheetTableInstanceViewModel? _draggedInstance;
        private FrameworkElement? _dragElement;
        private Point _dragStartPoint;
        private double _dragStartX;
        private double _dragStartY;
        private bool _isDragging;

        public ProjectSheetView()
        {
            InitializeComponent();
        }

        private ProjectSheetViewModel? ViewModel => DataContext as ProjectSheetViewModel;

        private void SheetSurface_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel?.LimparSelecao();
            Focus();
        }

        private void TableInstance_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement element ||
                element.DataContext is not ProjectSheetTableInstanceViewModel instance ||
                ViewModel == null)
                return;

            ViewModel.SelecionarInstancia(instance.Id);
            Focus();
            _draggedInstance = instance;
            _dragElement = element;
            _dragStartPoint = e.GetPosition(SheetSurface);
            _dragStartX = instance.X;
            _dragStartY = instance.Y;
            _isDragging = true;
            element.CaptureMouse();
            e.Handled = true;
        }

        private void TableInstance_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || _draggedInstance == null || e.LeftButton != MouseButtonState.Pressed)
                return;

            Point current = e.GetPosition(SheetSurface);
            double newX = _dragStartX + current.X - _dragStartPoint.X;
            double newY = _dragStartY + current.Y - _dragStartPoint.Y;
            _draggedInstance.SetPreviewPosition(newX, newY);
            e.Handled = true;
        }

        private void TableInstance_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CommitDrag();
            e.Handled = true;
        }

        private void TableInstance_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (_isDragging)
                CancelDragPreview();
        }

        private void SheetSurface_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (_isDragging)
                CancelDragPreview();
        }

        private void RemoveSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedInstance();
            Focus();
        }

        private void ProjectSheetView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete)
                return;

            if (RemoveSelectedInstance())
                e.Handled = true;
        }

        private bool RemoveSelectedInstance()
        {
            CancelDragPreview();
            return ViewModel?.RemoverInstanciaSelecionada() == true;
        }

        private void CommitDrag()
        {
            if (!_isDragging || _draggedInstance == null)
                return;

            ProjectSheetTableInstanceViewModel instance = _draggedInstance;
            double newX = instance.X;
            double newY = instance.Y;
            ReleaseDrag();

            if (Math.Abs(newX - _dragStartX) < 0.000001 && Math.Abs(newY - _dragStartY) < 0.000001)
                return;

            bool moved = ViewModel?.MoverInstancia(instance.Id, newX, newY) == true;

            if (!moved)
                instance.SetPreviewPosition(_dragStartX, _dragStartY);
        }

        private void CancelDragPreview()
        {
            if (_draggedInstance != null)
                _draggedInstance.SetPreviewPosition(_dragStartX, _dragStartY);

            ReleaseDrag();
        }

        private void ReleaseDrag()
        {
            _isDragging = false;

            if (_dragElement?.IsMouseCaptured == true)
                _dragElement.ReleaseMouseCapture();

            _dragElement = null;
            _draggedInstance = null;
        }
    }
}