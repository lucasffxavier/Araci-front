using System.Windows;
using Araci.Core.Rendering;
using Araci.Models;

namespace Araci.Core.SceneNodes
{
    public class EquipamentoNode : ElementoNode
    {
        private readonly double _largura;
        private readonly double _altura;

        public EquipamentoNode(
            ElementoEquipamento modelo,
            double largura = ElementGeometryDefaults.EquipamentoLargura,
            double altura = ElementGeometryDefaults.EquipamentoAltura)
            : base(modelo)
        {
            _largura = largura;
            _altura = altura;

            AtualizarGeometria();
        }

        public override void AtualizarGeometria()
        {
            Bounds = new Rect(Modelo.PosicaoX, Modelo.PosicaoY, _largura, _altura);
        }
    }
}