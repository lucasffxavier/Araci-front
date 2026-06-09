using System;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public sealed class ResizeProjectSheetTableInstanceCommand : IUndoableCommand
    {
        private readonly ProjectSheetTableInstance _instance;
        private readonly double _oldWidth;
        private readonly double _oldHeight;
        private readonly double _newWidth;
        private readonly double _newHeight;
        private readonly Action? _onChanged;

        public ResizeProjectSheetTableInstanceCommand(
            ProjectSheetTableInstance instance,
            double oldWidth,
            double oldHeight,
            double newWidth,
            double newHeight,
            Action? onChanged = null)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            _oldWidth = oldWidth;
            _oldHeight = oldHeight;
            _newWidth = newWidth;
            _newHeight = newHeight;
            _onChanged = onChanged;
        }

        public void Execute()
        {
            Apply(_newWidth, _newHeight);
        }

        public void Undo()
        {
            Apply(_oldWidth, _oldHeight);
        }

        public void Redo()
        {
            Apply(_newWidth, _newHeight);
        }

        private void Apply(double width, double height)
        {
            _instance.Width = width;
            _instance.Height = height;
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            _onChanged?.Invoke();
        }
    }
}