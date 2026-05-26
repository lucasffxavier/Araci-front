using System.Windows;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Gerador : ElementoEquipamento
    {
        public const string PARAM_POTENCIA_APARENTE = "PotenciaAparente";

        public TipoGerador TipoGerador => (TipoGerador)Tipo!;

        public double PotenciaAparente
        {
            get => Obter<double>(PARAM_POTENCIA_APARENTE);
            set => Definir(PARAM_POTENCIA_APARENTE, value);
        }

        public Gerador()
        {
            DefinirParametro(new Parameter<double>(PARAM_POTENCIA_APARENTE, 0));

            Nome = "GERADOR-001";
            Barra = "GERADOR-001";
            Alimentador = 1;
            PotenciaAparente = 1020;
            PotenciaAtiva = 1000;
            PotenciaReativa = 203;
            TensaoLinha = "12.47∠0°";
            TensaoFaseA = "7.2∠0°";
            TensaoFaseB = "7.2∠-120°";
            TensaoFaseC = "7.2∠120°";
            CorrenteLinha = "0∠0°";
            CorrenteFaseA = "0∠0°";
            CorrenteFaseB = "0∠-120°";
            CorrenteFaseC = "0∠120°";

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
                    terminais.Add(new Terminal(this, new Point(), IdTerminal(i)) { Barra = Barra });
            }

            terminais[0].Barra = Barra;
            terminais[0].Posicao = new Point(PosicaoX + largura / 2, PosicaoY);
            terminais[1].Posicao = new Point(PosicaoX + largura / 2, PosicaoY + altura);
            terminais[2].Posicao = new Point(PosicaoX, PosicaoY + altura / 2);
            terminais[3].Posicao = new Point(PosicaoX + largura, PosicaoY + altura / 2);
        }

        public override Elemento Clonar()
        {
            var clone = new Gerador();

            CopiarEquipamentoPara(clone);

            return clone;
        }

        private static string IdTerminal(int index)
        {
            return index switch
            {
                0 => "TOPO",
                1 => "BASE",
                2 => "ESQUERDA",
                3 => "DIREITA",
                _ => $"T{index + 1}"
            };
        }
    }
}
