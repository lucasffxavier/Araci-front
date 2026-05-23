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
            Alimentador = "AL-01";
            PotenciaAparente = 5100;
            PotenciaAtiva = 5000;
            PotenciaReativa = 995;
            TensaoLinha = "34.5∠0°";
            TensaoFaseA = "19.92∠0°";
            TensaoFaseB = "19.92∠-120°";
            TensaoFaseC = "19.92∠120°";
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
