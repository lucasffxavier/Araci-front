using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Core.Commands;
using Araci.Models;
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
                CapturarEstadoInicial(vm);

            _movendo = true;
        }

        public void MoverVisual(
            ElementoViewModel vm,
            Vector delta)
        {
            if (!_movendo)
                return;

            vm.Mover(delta);
            ReancorarCabosConectados(vm.Modelo);
            _context.SceneQueries.Invalidate();
        }

        public void EndMove(IEnumerable<ElementoViewModel> elementos)
        {
            if (!_movendo)
                return;

            using var transaction =
                _context.BeginTransaction();

            foreach (var vm in elementos.Distinct())
                CapturarCabosConectados(vm.Modelo);

            foreach (var item in _estadoInicial)
            {
                var vm = item.Key;
                var antes = item.Value;

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
                        vm.Modelo,
                        antes,
                        depois,
                        AtualizarElementoMovido));
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

        private void AtualizarElementoMovido(Elemento elemento)
        {
            _context.Viewport?.AtualizarViewModel(elemento);
            _context.TerminalLayout.AtualizarTerminais(elemento);
            _context.SceneQueries.Invalidate();
            _context.CableVertexEdit.Refresh();
        }

        private void CapturarEstadoInicial(ElementoViewModel vm)
        {
            if (_estadoInicial.ContainsKey(vm))
                return;

            _estadoInicial[vm] = vm.CapturarEstado();
            CapturarCabosConectados(vm.Modelo);
        }

        private void CapturarCabosConectados(Elemento elemento)
        {
            if (elemento is Cabo)
                return;

            foreach (Cabo cabo in _context.Connectivity.ObterCabosConectados(elemento))
            {
                ElementoViewModel? caboVm = _context.Viewport?.ObterViewModel(cabo);

                if (caboVm != null && !_estadoInicial.ContainsKey(caboVm))
                    _estadoInicial[caboVm] = caboVm.CapturarEstado();
            }
        }

        private void ReancorarCabosConectados(Elemento elemento)
        {
            if (elemento is Cabo)
                return;

            _context.TerminalLayout.AtualizarTerminais(elemento);

            foreach (Cabo cabo in _context.Connectivity.ReancorarCabosConectados(elemento))
                _context.Viewport?.AtualizarViewModel(cabo);
        }
    }
}
