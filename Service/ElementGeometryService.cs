using System.Windows;
using Araci.Models;

namespace Araci.Services
{
    public class ElementGeometryService
    {
        private const double LarguraBarra = 10;
        private const double TamanhoEquipamento = 70;

        public Size ObterTamanho(Elemento elemento)
        {
            return elemento switch
            {
                Barra barra => new Size(LarguraBarra, barra.Altura),
                ElementoEquipamento => new Size(TamanhoEquipamento, TamanhoEquipamento),
                ElementoLinear => Size.Empty,
                _ => new Size(TamanhoEquipamento, TamanhoEquipamento)
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
