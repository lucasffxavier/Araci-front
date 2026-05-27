using System.Windows;
using Araci.Core.Rendering;
using Araci.Models;

namespace Araci.Services
{
    public class ElementGeometryService
    {
        private readonly ElementRegistryService? _registry;

        public ElementGeometryService()
        {
        }

        public ElementGeometryService(ElementRegistryService registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public Size ObterTamanho(Elemento elemento)
        {
            if (_registry != null)
                return _registry.GetSize(elemento);

            return elemento switch
            {
                Barra barra => new Size(ElementGeometryDefaults.BarraLargura, barra.Altura),
                ElementoEquipamento => new Size(
                    ElementGeometryDefaults.EquipamentoLargura,
                    ElementGeometryDefaults.EquipamentoAltura),
                ElementoLinear => Size.Empty,
                _ => new Size(
                    ElementGeometryDefaults.EquipamentoLargura,
                    ElementGeometryDefaults.EquipamentoAltura)
            };
        }

        public Point CalcularTopoEsquerdoPorCentro(Elemento elemento, Point centro)
        {
            Size tamanho = ObterTamanho(elemento);

            return new Point(
                centro.X - tamanho.Width / 2,
                centro.Y - tamanho.Height / 2);
        }
    }
}
