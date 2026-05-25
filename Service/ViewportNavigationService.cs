using System.Windows;
using System.Windows.Input;

namespace Araci.Services
{
    public class ViewportNavigationService
    {
        private readonly EditorContext _context;
        private bool _isPanning;
        private bool _isSpacePressed;
        private bool _isSpaceLeftPanning;
        private bool _suppressNextLeftButtonUp;
        private Point _lastPanPoint;

        public ViewportNavigationService(EditorContext context)
        {
            _context = context;
        }

        public bool IsPanning => _isPanning;

        public bool IsSpacePressed => _isSpacePressed;

        public bool SuppressNextLeftButtonUp => _suppressNextLeftButtonUp;

        public bool TryBeginMiddlePan(MouseButtonEventArgs e, IInputElement relativeTo)
        {
            if (_context.Viewport == null || e.ChangedButton != MouseButton.Middle || e.ClickCount >= 2)
                return false;

            BeginPan(e.GetPosition(relativeTo), spaceLeftPan: false);
            return true;
        }

        public bool TryBeginSpaceLeftPan(MouseButtonEventArgs e, IInputElement relativeTo)
        {
            if (_context.Viewport == null || !_isSpacePressed || e.ChangedButton != MouseButton.Left)
                return false;

            BeginPan(e.GetPosition(relativeTo), spaceLeftPan: true);
            return true;
        }

        public bool TryUpdatePan(MouseEventArgs e, IInputElement relativeTo)
        {
            if (_context.Viewport == null || !_isPanning)
                return false;

            Point current = e.GetPosition(relativeTo);
            Vector delta = current - _lastPanPoint;

            _context.Viewport.Pan(delta);

            _lastPanPoint = current;
            return true;
        }

        public bool TryEndMiddlePan(MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Middle || !_isPanning || _isSpaceLeftPanning)
                return false;

            CancelPan();
            return true;
        }

        public bool TryEndSpaceLeftPan(MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left || !_isSpaceLeftPanning)
                return false;

            CancelPan();
            return true;
        }

        public bool TryHandleKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                _isSpacePressed = true;
                return true;
            }

            return TryHandleViewportShortcut(e);
        }

        public bool TryHandleKeyUp(KeyEventArgs e)
        {
            if (e.Key != Key.Space)
                return false;

            _isSpacePressed = false;

            if (_isSpaceLeftPanning)
            {
                _suppressNextLeftButtonUp = true;
                CancelPan();
            }

            return true;
        }

        public bool TryHandleMouseWheel(MouseWheelEventArgs e, IInputElement relativeTo)
        {
            if (_context.Viewport == null)
                return false;

            Point cursor = e.GetPosition(relativeTo);

            if (e.Delta > 0)
                _context.Viewport.ZoomInAt(cursor);
            else if (e.Delta < 0)
                _context.Viewport.ZoomOutAt(cursor);

            return true;
        }

        public bool TryHandleViewportShortcut(KeyEventArgs e)
        {
            if (_context.Viewport == null)
                return false;

            if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                return false;

            switch (e.Key)
            {
                case Key.OemPlus:
                case Key.Add:
                    _context.Viewport.ZoomInAtCenter();
                    return true;

                case Key.OemMinus:
                case Key.Subtract:
                    _context.Viewport.ZoomOutAtCenter();
                    return true;

                case Key.D0:
                case Key.NumPad0:
                    _context.Viewport.ResetCamera();
                    return true;

                case Key.D1:
                case Key.NumPad1:
                    _context.Viewport.Zoom100AtCenter();
                    return true;

                default:
                    return false;
            }
        }

        public bool TryHandleMiddleDoubleClick(MouseButtonEventArgs e)
        {
            if (_context.Viewport == null || e.ChangedButton != MouseButton.Middle || e.ClickCount < 2)
                return false;

            CancelPan();
            _context.Viewport.ZoomExtents();
            return true;
        }

        public bool ConsumeSuppressNextLeftButtonUp()
        {
            if (!_suppressNextLeftButtonUp)
                return false;

            _suppressNextLeftButtonUp = false;
            return true;
        }

        public void CancelPan()
        {
            _isPanning = false;
            _isSpaceLeftPanning = false;
        }

        public void Reset()
        {
            _isPanning = false;
            _isSpacePressed = false;
            _isSpaceLeftPanning = false;
            _suppressNextLeftButtonUp = false;
            _lastPanPoint = new Point();
        }

        private void BeginPan(Point start, bool spaceLeftPan)
        {
            _isPanning = true;
            _isSpaceLeftPanning = spaceLeftPan;
            _lastPanPoint = start;
        }
    }
}
