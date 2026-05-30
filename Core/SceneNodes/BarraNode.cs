using System;
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

        public override Rect BoundsAlinhamento
        {
            get
            {
                double inset = Math.Min(Altura / 4.0, Math.Max(0, ElementGeometryDefaults.BarraLargura / 2.0));
                double alturaUtil = Math.Max(1, Altura - inset * 2);
                return new Rect(Modelo.PosicaoX, Modelo.PosicaoY + inset, Largura, alturaUtil);
            }
        }

        public override void AtualizarGeometria()
        {
            Bounds = new Rect(Modelo.PosicaoX, Modelo.PosicaoY, Largura, Altura);
        }
    }
}