using System;
using System.Windows;
using Araci.Applications.Editar.Base;
using Araci.Services;

namespace Araci.Services.Editing
{
    public class MoveConstraintService
    {
        private const double DefaultGridStep = 10.0;

        private readonly EditorSettings _settings;

        private Point _start;
        private OrthogonalAxis? _orthogonalAxis;

        public MoveConstraintService(EditorSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

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

            bool shouldApplyGrid =
                inputState.IsControlPressed &&
                _settings.GridSnapEnabled;

            return shouldApplyGrid
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

        private double Quantize(double value)
        {
            double step = GetGridStep();

            return Math.Round(value / step) * step;
        }

        private double GetGridStep()
        {
            return _settings.GridStep > 0
                ? _settings.GridStep
                : DefaultGridStep;
        }

        private enum OrthogonalAxis
        {
            Horizontal,
            Vertical
        }
    }
}
