using System.Collections.Generic;
using System.Windows;

using Araci.Core.Commands;
using Araci.ViewModels;

namespace Araci.Services
{
    public static class MoveService
    {
        // =========================
        // ESTADO DRAG
        // =========================

        private static bool _movendo;

        private static readonly Dictionary<
            ElementoViewModel,
            ElementoEstado>
            _estadoInicial = new();

        // =========================
        // BEGIN
        // =========================

        public static void BeginMove(
            IEnumerable<ElementoViewModel> elementos)
        {
            _estadoInicial.Clear();

            foreach (var vm in elementos)
            {
                _estadoInicial[vm] =
                    vm.CapturarEstado();
            }

            _movendo = true;
        }

        // =========================
        // MOVE VISUAL
        // =========================

        public static void MoverVisual(
            ElementoViewModel vm,
            Vector delta)
        {
            if (!_movendo)
                return;

            vm.Mover(delta);
        }

        // =========================
        // END MOVE
        // =========================

        public static void EndMove(
            IEnumerable<ElementoViewModel> elementos)
        {
            if (!_movendo)
                return;

            using var transaction =
                AppServices.BeginTransaction();

            foreach (var vm in elementos)
            {
                if (!_estadoInicial.ContainsKey(vm))
                    continue;

                ElementoEstado antes =
                    _estadoInicial[vm];

                ElementoEstado depois =
                    vm.CapturarEstado();

                bool mudou =
                    antes.X != depois.X ||
                    antes.Y != depois.Y ||
                    antes.X2 != depois.X2 ||
                    antes.Y2 != depois.Y2;

                if (!mudou)
                    continue;

                transaction.Add(
                    new MoveElementoCommand(
                        vm,
                        antes,
                        depois));
            }

            transaction.Commit();

            _estadoInicial.Clear();

            _movendo = false;
        }
    }
}