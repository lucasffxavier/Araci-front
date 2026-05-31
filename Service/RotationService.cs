using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Applications.UseCases.Editar;
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
                _context.VisualUpdates.AtualizarElementoRotacionado(vm.Modelo);

            var after = Capturar(affected);
            var items = affected
                .Select(vm => new RotacionarElementoItem(vm.Modelo, before[vm], after[vm]))
                .ToList();

            return _context.RotacionarElemento.Executar(items, _context.VisualUpdates.AtualizarElementoRotacionado);
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

    }
}
