using System;
using System.Windows;

using Araci.ViewModels;

namespace Araci.Core.Commands
{
    public class MoveElementCommand
        : IUndoableCommand
    {
        // =========================
        // ELEMENTO
        // =========================

        private readonly ElementoViewModel
            _vm;

        // =========================
        // ESTADO
        // =========================

        private readonly double _xInicial;
        private readonly double _yInicial;

        private readonly double _xFinal;
        private readonly double _yFinal;

        // =========================
        // CABO
        // =========================

        private readonly double? _x2Inicial;
        private readonly double? _y2Inicial;

        private readonly double? _x2Final;
        private readonly double? _y2Final;

        // =========================
        // CONSTRUTOR
        // =========================

        public MoveElementCommand(
            ElementoViewModel vm,
            Vector delta)
        {
            _vm = vm;

            _xInicial = vm.X;
            _yInicial = vm.Y;

            _xFinal = vm.X + delta.X;
            _yFinal = vm.Y + delta.Y;

            if (vm is CaboViewModel cabo)
            {
                _x2Inicial = cabo.X2;
                _y2Inicial = cabo.Y2;

                _x2Final =
                    cabo.X2 + delta.X;

                _y2Final =
                    cabo.Y2 + delta.Y;
            }
        }

        // =========================
        // EXECUTE
        // =========================

        public void Execute()
        {
            _vm.X = _xFinal;
            _vm.Y = _yFinal;

            if (_vm is CaboViewModel cabo)
            {
                cabo.X2 = _x2Final ?? cabo.X2;
                cabo.Y2 = _y2Final ?? cabo.Y2;
            }
        }

        // =========================
        // UNDO
        // =========================

        public void Undo()
        {
            _vm.X = _xInicial;
            _vm.Y = _yInicial;

            if (_vm is CaboViewModel cabo)
            {
                cabo.X2 = _x2Inicial ?? cabo.X2;
                cabo.Y2 = _y2Inicial ?? cabo.Y2;
            }
        }
    }
}