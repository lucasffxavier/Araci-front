using System.Windows;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Gerador : ElementoEquipamento
    {
        public const string PARAM_POTENCIA_APARENTE_KVA = "PotenciaAparenteKVA";

        public TipoGerador TipoGerador => (TipoGerador)Tipo!;

        public double PotenciaAparenteKVA
        {
            get => Obter<double>(PARAM_POTENCIA_APARENTE_KVA);
            set => Definir(PARAM_POTENCIA_APARENTE_KVA, value);
        }

        public Gerador()
        {
            DefinirParametro(new Parameter<double>(PARAM_POTENCIA_APARENTE_KVA, 0));

            Nome = "GERADOR-001";
            Alimentador = "AL-01";
            PotenciaAparenteKVA = 5100;
            PotenciaAtivaKW = 5000;
            PotenciaReativaKvar = 995;
            TensaoLinha = "34.5 kV";
            TensaoFaseA = "19.92 kV";
            TensaoFaseB = "19.92 kV";
            TensaoFaseC = "19.92 kV";
            CorrenteLinha = "0 A";
            CorrenteFaseA = "0 A";
            CorrenteFaseB = "0 A";
            CorrenteFaseC = "0 A";

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
    }
}
