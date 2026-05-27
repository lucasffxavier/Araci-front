using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Core.Commands;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Services
{
    public class RotationService
    {
        private readonly EditorContext _context;

        public RotationService(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public bool RotateSelectionClockwise()
        {
            var targets = _context.Selection.Selecionados
                .Where(PodeRotacionar)
                .Distinct()
                .ToList();

            if (targets.Count == 0)
                return false;

            var affected = ColetarAfetados(targets);
            var before = Capturar(affected);

            foreach (ElementoViewModel vm in targets)
                RotacionarModelo(vm.Modelo);

            foreach (ElementoViewModel vm in targets)
                AtualizarElementoRotacionado(vm.Modelo);

            var after = Capturar(affected);
            var items = affected
                .Where(vm => Mudou(before[vm], after[vm]))
                .Select(vm => new RotateElementoCommand.Item(vm.Modelo, before[vm], after[vm]))
                .ToList();

            if (items.Count == 0)
                return false;

            _context.Commands.Execute(new RotateElementoCommand(items, AtualizarAposComando));
            AtualizarEstadoVisual();
            return true;
        }

        public static double RotateClockwise(double value)
        {
            double normalized = Normalize(value);
            return normalized >= 270 ? 0 : normalized + 90;
        }

        public static bool PodeRotacionar(ElementoViewModel vm)
        {
            return vm.Modelo is Barra or Carga or Gerador or Sin or Transformador;
        }

        private static double Normalize(double value)
        {
            double normalized = value % 360;

            if (normalized < 0)
                normalized += 360;

            double snapped = Math.Round(normalized / 90.0) * 90.0;
            return snapped >= 360 ? 0 : snapped;
        }

        private void RotacionarModelo(Elemento elemento)
        {
            elemento.Rotacao = RotateClockwise(elemento.Rotacao);
        }

        private List<ElementoViewModel> ColetarAfetados(IEnumerable<ElementoViewModel> targets)
        {
            var result = new List<ElementoViewModel>();

            foreach (ElementoViewModel vm in targets)
            {
                Adicionar(result, vm);

                foreach (Cabo cabo in _context.Connectivity.ObterCabosConectados(vm.Modelo))
                {
                    ElementoViewModel? caboVm = _context.Viewport?.ObterViewModel(cabo);

                    if (caboVm != null)
                        Adicionar(result, caboVm);
                }
            }

            return result;
        }

        private static void Adicionar(ICollection<ElementoViewModel> items, ElementoViewModel vm)
        {
            if (!items.Contains(vm))
                items.Add(vm);
        }

        private static Dictionary<ElementoViewModel, ElementoEstado> Capturar(
            IEnumerable<ElementoViewModel> items)
        {
            return items.ToDictionary(vm => vm, vm => vm.CapturarEstado());
        }

        private void AtualizarElementoRotacionado(Elemento elemento)
        {
            _context.TerminalLayout.AtualizarTerminais(elemento);

            foreach (Cabo cabo in _context.Connectivity.ReancorarCabosConectados(elemento))
                _context.Viewport?.AtualizarViewModel(cabo);

            _context.Viewport?.AtualizarViewModel(elemento);
        }

        private void AtualizarAposComando(Elemento elemento)
        {
            _context.TerminalLayout.AtualizarTerminais(elemento);
            _context.Viewport?.AtualizarViewModel(elemento);
            _context.SceneQueries.Invalidate();
            _context.CableVertexEdit.Refresh();
        }

        private void AtualizarEstadoVisual()
        {
            _context.SceneQueries.Invalidate();
            _context.CableVertexEdit.Refresh();
        }

        private static bool Mudou(ElementoEstado antes, ElementoEstado depois)
        {
            return antes.X != depois.X ||
                antes.Y != depois.Y ||
                antes.X2 != depois.X2 ||
                antes.Y2 != depois.Y2 ||
                antes.Rotacao != depois.Rotacao ||
                !antes.Vertices.SequenceEqual(depois.Vertices);
        }
    }
}
