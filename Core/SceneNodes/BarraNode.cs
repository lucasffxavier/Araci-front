using System.Windows;
using Araci.Core.Rendering;
using Araci.Models;

namespace Araci.Core.SceneNodes
{
    public class BarraNode : ElementoNode
    {
        private readonly Barra _barra;

        public BarraNode(Barra barra)
            : base(barra)
        {
            _barra = barra;
            AtualizarGeometria();
        }

        public override double Largura => ElementGeometryDefaults.BarraLargura;

        public override double Altura => _barra.Altura;

        public override void AtualizarGeometria()
        {
            Bounds = new Rect(
                Modelo.PosicaoX,
                Modelo.PosicaoY,
                Largura,
                Altura);
        }
    }
}