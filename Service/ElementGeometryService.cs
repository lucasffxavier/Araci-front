using System.Windows;
using Araci.Core.Rendering;
using Araci.Models;

namespace Araci.Services
{
    public class ElementGeometryService
    {
        public Size ObterTamanho(Elemento elemento)
        {
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
