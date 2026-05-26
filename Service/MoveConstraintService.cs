using System;
using System.Windows;
using Araci.Applications.Editar.Base;

namespace Araci.Services
{
    public class MoveConstraintService
    {
        private const double GridStep = 10.0;

        private Point _start;
        private OrthogonalAxis? _orthogonalAxis;

        public void Begin(Point start)
        {
            _start = start;
            _orthogonalAxis = null;
        }

        public Point Apply(Point position, ToolInputState inputState)
        {
            Point result = position;

            if (!inputState.IsShiftPressed)
            {
                _orthogonalAxis = null;
            }
            else
            {
                result = ApplyOrthogonal(position);
            }

            return inputState.IsControlPressed
                ? ApplyGrid(result)
                : result;
        }

        public void End()
        {
            _orthogonalAxis = null;
        }

        public void Cancel()
        {
            _orthogonalAxis = null;
        }

        private Point ApplyOrthogonal(Point position)
        {
            Vector total = position - _start;

            if (!_orthogonalAxis.HasValue)
            {
                if (Math.Abs(total.X) < 0.0001 && Math.Abs(total.Y) < 0.0001)
                    return _start;

                _orthogonalAxis = Math.Abs(total.X) >= Math.Abs(total.Y)
                    ? OrthogonalAxis.Horizontal
                    : OrthogonalAxis.Vertical;
            }

            return _orthogonalAxis == OrthogonalAxis.Horizontal
                ? new Point(position.X, _start.Y)
                : new Point(_start.X, position.Y);
        }

        private Point ApplyGrid(Point position)
        {
            Vector delta = position - _start;

            return new Point(
                _start.X + Quantize(delta.X),
                _start.Y + Quantize(delta.Y));
        }

        private static double Quantize(double value)
        {
            return Math.Round(value / GridStep) * GridStep;
        }

        private enum OrthogonalAxis
        {
            Horizontal,
            Vertical
        }
    }
}
