using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Core.Commands;
using Araci.ViewModels;

namespace Araci.Services
{
    public class MoveService
    {
        private readonly EditorContext _context;

        private readonly Dictionary<ElementoViewModel, ElementoEstado>
            _estadoInicial = new();

        private bool _movendo;

        public MoveService(EditorContext context)
        {
            _context = context ??
                throw new ArgumentNullException(nameof(context));
        }

        public void BeginMove(IEnumerable<ElementoViewModel> elementos)
        {
            _estadoInicial.Clear();

            foreach (var vm in elementos.Distinct())
            {
                _estadoInicial[vm] = vm.CapturarEstado();
            }

            _movendo = true;
        }

        public void MoverVisual(
            ElementoViewModel vm,
            Vector delta)
        {
            if (!_movendo)
                return;

            vm.Mover(delta);
            _context.SceneQueries.Invalidate();
        }

        public void EndMove(IEnumerable<ElementoViewModel> elementos)
        {
            if (!_movendo)
                return;

            using var transaction =
                _context.BeginTransaction();

            foreach (var vm in elementos.Distinct())
            {
                if (!_estadoInicial.TryGetValue(vm, out var antes))
                    continue;

                var depois = vm.CapturarEstado();

                bool mudou =
                    antes.X != depois.X ||
                    antes.Y != depois.Y ||
                    antes.X2 != depois.X2 ||
                    antes.Y2 != depois.Y2 ||
                    !antes.Vertices.SequenceEqual(depois.Vertices);

                if (!mudou)
                    continue;

                transaction.Add(
                    new MoveElementoCommand(
                        vm,
                        antes,
                        depois));
            }

            transaction.Commit();
            _context.SceneQueries.Invalidate();

            _estadoInicial.Clear();

            _movendo = false;
        }

        public void AbortMove()
        {
            if (!_movendo)
                return;

            foreach (var item in _estadoInicial)
                item.Key.AplicarEstado(item.Value);

            _context.SceneQueries.Invalidate();

            _estadoInicial.Clear();
            _movendo = false;
        }
    }
}
