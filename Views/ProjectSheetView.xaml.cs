using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
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

        private ProjectSheetTableInstanceViewModel? _resizedInstance;
        private FrameworkElement? _resizeElement;
        private Point _resizeStartPoint;
        private double _resizeStartWidth;
        private double _resizeStartHeight;
        private SheetTableResizeMode _resizeMode = SheetTableResizeMode.None;
        private bool _isResizing;

        public ProjectSheetView()
        {
            InitializeComponent();
        }

        private ProjectSheetViewModel? ViewModel => DataContext as ProjectSheetViewModel;

        private void ProjectSheetView_Loaded(object sender, RoutedEventArgs e)
        {
            CenterSheetInViewportDeferred();
        }

        private void ProjectSheetView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CenterSheetInViewportDeferred();
        }

        private void SheetSurface_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CancelResizePreview();
            CancelDragPreview();
            ViewModel?.LimparSelecao();
            Focus();
        }

        private void TableInstance_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isResizing)
                return;

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

            if (ViewModel != null)
                ViewModel.SetPreviewPosition(_draggedInstance, newX, newY);
            else
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

            if (_isResizing)
                CancelResizePreview();
        }

        private void ResizeHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement element ||
                element.DataContext is not ProjectSheetTableInstanceViewModel instance ||
                ViewModel == null)
                return;

            CancelDragPreview();
            ViewModel.SelecionarInstancia(instance.Id);
            Focus();

            _resizedInstance = instance;
            _resizeElement = element;
            _resizeStartPoint = e.GetPosition(SheetSurface);
            _resizeStartWidth = instance.Width;
            _resizeStartHeight = instance.Height;
            _resizeMode = ObterModoRedimensionamento(element.Tag);
            _isResizing = _resizeMode != SheetTableResizeMode.None;

            if (_isResizing)
                element.CaptureMouse();

            e.Handled = true;
        }

        private void ResizeHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isResizing || _resizedInstance == null || e.LeftButton != MouseButtonState.Pressed)
                return;

            Point current = e.GetPosition(SheetSurface);
            double deltaX = current.X - _resizeStartPoint.X;
            double deltaY = current.Y - _resizeStartPoint.Y;
            double newWidth = _resizeStartWidth;
            double newHeight = _resizeStartHeight;

            if (_resizeMode is SheetTableResizeMode.Right or SheetTableResizeMode.BottomRight)
                newWidth = _resizeStartWidth + deltaX;

            if (_resizeMode is SheetTableResizeMode.Bottom or SheetTableResizeMode.BottomRight)
                newHeight = _resizeStartHeight + deltaY;

            if (ViewModel != null)
                ViewModel.SetPreviewSize(_resizedInstance, newWidth, newHeight);
            else
                _resizedInstance.SetPreviewSize(newWidth, newHeight);

            e.Handled = true;
        }

        private void ResizeHandle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CommitResize();
            e.Handled = true;
        }

        private void ResizeHandle_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (_isResizing)
                CancelResizePreview();
        }

        private void RemoveSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedInstance();
            Focus();
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            ApplySheetCenteredZoom(() => ViewModel?.ZoomIn());
            Focus();
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            ApplySheetCenteredZoom(() => ViewModel?.ZoomOut());
            Focus();
        }

        private void ResetZoomButton_Click(object sender, RoutedEventArgs e)
        {
            ApplySheetCenteredZoom(() => ViewModel?.ResetZoom());
            Focus();
        }

        private void ProjectSheetView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                return;

            ApplySheetCenteredZoom(() =>
            {
                if (e.Delta > 0)
                    ViewModel?.ZoomIn();
                else if (e.Delta < 0)
                    ViewModel?.ZoomOut();
            });

            e.Handled = true;
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
            CancelResizePreview();
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

        private void CommitResize()
        {
            if (!_isResizing || _resizedInstance == null)
                return;

            ProjectSheetTableInstanceViewModel instance = _resizedInstance;
            double newWidth = instance.Width;
            double newHeight = instance.Height;
            ReleaseResize();

            if (Math.Abs(newWidth - _resizeStartWidth) < 0.000001 && Math.Abs(newHeight - _resizeStartHeight) < 0.000001)
                return;

            bool resized = ViewModel?.RedimensionarInstancia(instance.Id, newWidth, newHeight) == true;

            if (!resized)
                instance.SetPreviewSize(_resizeStartWidth, _resizeStartHeight);
        }

        private void CancelDragPreview()
        {
            if (_draggedInstance != null)
                _draggedInstance.SetPreviewPosition(_dragStartX, _dragStartY);

            ReleaseDrag();
            ViewModel?.RestoreWorkspaceFromDocument();
        }

        private void CancelResizePreview()
        {
            if (_resizedInstance != null)
                _resizedInstance.SetPreviewSize(_resizeStartWidth, _resizeStartHeight);

            ReleaseResize();
            ViewModel?.RestoreWorkspaceFromDocument();
        }

        private void ReleaseDrag()
        {
            _isDragging = false;

            if (_dragElement?.IsMouseCaptured == true)
                _dragElement.ReleaseMouseCapture();

            _dragElement = null;
            _draggedInstance = null;
        }

        private void ReleaseResize()
        {
            _isResizing = false;

            if (_resizeElement?.IsMouseCaptured == true)
                _resizeElement.ReleaseMouseCapture();

            _resizeElement = null;
            _resizedInstance = null;
            _resizeMode = SheetTableResizeMode.None;
        }

        private void ApplySheetCenteredZoom(Action zoomAction)
        {
            if (zoomAction == null)
                return;

            zoomAction();
            CenterSheetInViewport(updateLayout: true);
        }

        private void CenterSheetInViewportDeferred()
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(() => CenterSheetInViewport(updateLayout: true)));
        }

        private void CenterSheetInViewport(bool updateLayout = false)
        {
            if (!IsLoaded || SheetPageBorder.ActualWidth <= 0 || SheetPageBorder.ActualHeight <= 0)
                return;

            if (updateLayout)
                SheetScrollViewer.UpdateLayout();

            Rect sheetBounds;

            try
            {
                sheetBounds = SheetPageBorder
                    .TransformToAncestor(SheetScrollViewer)
                    .TransformBounds(new Rect(0, 0, SheetPageBorder.ActualWidth, SheetPageBorder.ActualHeight));
            }
            catch (InvalidOperationException)
            {
                return;
            }

            double viewportWidth = SheetScrollViewer.ViewportWidth;
            double viewportHeight = SheetScrollViewer.ViewportHeight;

            if (double.IsNaN(viewportWidth) || double.IsInfinity(viewportWidth) || viewportWidth <= 0)
                viewportWidth = SheetScrollViewer.ActualWidth;

            if (double.IsNaN(viewportHeight) || double.IsInfinity(viewportHeight) || viewportHeight <= 0)
                viewportHeight = SheetScrollViewer.ActualHeight;

            if (viewportWidth <= 0 || viewportHeight <= 0)
                return;

            double targetHorizontal = SheetScrollViewer.HorizontalOffset + sheetBounds.Left + sheetBounds.Width / 2.0 - viewportWidth / 2.0;
            double targetVertical = SheetScrollViewer.VerticalOffset + sheetBounds.Top + sheetBounds.Height / 2.0 - viewportHeight / 2.0;

            SheetScrollViewer.ScrollToHorizontalOffset(NormalizeScrollOffset(targetHorizontal, SheetScrollViewer.ScrollableWidth));
            SheetScrollViewer.ScrollToVerticalOffset(NormalizeScrollOffset(targetVertical, SheetScrollViewer.ScrollableHeight));
        }

        private static double NormalizeScrollOffset(double value, double maximum)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
                return 0;

            if (double.IsNaN(maximum) || double.IsInfinity(maximum) || maximum < 0)
                return value;

            return Math.Min(value, maximum);
        }

        private static SheetTableResizeMode ObterModoRedimensionamento(object? tag)
        {
            string? value = tag?.ToString();

            return value switch
            {
                "Right" => SheetTableResizeMode.Right,
                "Bottom" => SheetTableResizeMode.Bottom,
                "BottomRight" => SheetTableResizeMode.BottomRight,
                _ => SheetTableResizeMode.None
            };
        }

        private enum SheetTableResizeMode
        {
            None,
            Right,
            Bottom,
            BottomRight
        }
    }
}
