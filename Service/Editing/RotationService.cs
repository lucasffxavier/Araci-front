using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Applications.UseCases.Editar;
using Araci.Models;
using Araci.ViewModels;
using Araci.Services.Topology;
using Araci.Services;
using Araci.Services.Viewport;

namespace Araci.Services.Editing
{
    public class RotationService
    {
        private readonly SelectionService _selection;
        private readonly ConnectivityService _connectivity;
        private readonly Func<ViewportService?> _viewportProvider;
        private readonly VisualUpdateService _visualUpdates;
        private readonly RotacionarElementoUseCase _rotacionarElemento;

        public RotationService(
            SelectionService selection,
            ConnectivityService connectivity,
            Func<ViewportService?> viewportProvider,
            VisualUpdateService visualUpdates,
            RotacionarElementoUseCase rotacionarElemento)
        {
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));
            _viewportProvider = viewportProvider ?? throw new ArgumentNullException(nameof(viewportProvider));
            _visualUpdates = visualUpdates ?? throw new ArgumentNullException(nameof(visualUpdates));
            _rotacionarElemento = rotacionarElemento ?? throw new ArgumentNullException(nameof(rotacionarElemento));
        }

        public bool RotateSelectionClockwise()
        {
            var targets = _selection.Selecionados
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
                _visualUpdates.AtualizarElementoRotacionado(vm.Modelo);

            var after = Capturar(affected);
            var items = affected
                .Select(vm => new RotacionarElementoItem(vm.Modelo, before[vm], after[vm]))
                .ToList();

            return _rotacionarElemento.Executar(items, _visualUpdates.AtualizarElementoRotacionado);
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
            ViewportService? viewport = _viewportProvider();

            foreach (ElementoViewModel vm in targets)
            {
                Adicionar(result, vm);

                foreach (Cabo cabo in _connectivity.ObterCabosConectados(vm.Modelo))
                {
                    ElementoViewModel? caboVm = viewport?.ObterViewModel(cabo);

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
