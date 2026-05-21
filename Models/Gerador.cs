using System.Collections.Generic;
using System.Windows;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Gerador : ElementoEquipamento
    {
        public const string PARAM_FATOR_POTENCIA = "FatorPotencia";

        public TipoGerador TipoGerador =>
            (TipoGerador)Tipo!;

        public double FatorPotencia
        {
            get => Obter<double>(PARAM_FATOR_POTENCIA);
            set => Definir(PARAM_FATOR_POTENCIA, value);
        }

        public Gerador()
        {
            Nome = "GER-01";
            Barra = "BUS-01";
            Alimentador = "AL-01";
            PotenciaAtivaKW = 5000;

            DefinirParametro(
                new Parameter<double>(
                    PARAM_FATOR_POTENCIA,
                    0.98));

            PosicaoX = 300;
            PosicaoY = 200;
        }

        public void AtualizarTerminais(double largura, double altura)
        {
            var terminais = ObterTerminaisInternos();

            if (terminais.Count < 4)
            {
                terminais.Clear();

                for (int i = 0; i < 4; i++)
                    terminais.Add(new Terminal(this, new Point()));
            }

            // Norte
            terminais[0].Posicao = new Point(
                PosicaoX + largura / 2,
                PosicaoY
            );

            // Sul
            terminais[1].Posicao = new Point(
                PosicaoX + largura / 2,
                PosicaoY + altura
            );

            // Oeste
            terminais[2].Posicao = new Point(
                PosicaoX,
                PosicaoY + altura / 2
            );

            // Leste
            terminais[3].Posicao = new Point(
                PosicaoX + largura,
                PosicaoY + altura / 2
            );
        }

        public override Elemento Clonar()
        {
            var clone = new Gerador();
            CopiarEquipamentoPara(clone);
            return clone;
        }
    }
}