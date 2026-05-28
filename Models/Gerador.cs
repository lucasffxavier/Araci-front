using System.Windows;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Gerador : ElementoEquipamento
    {
        public const string PARAM_POTENCIA_APARENTE = "PotenciaAparente";
        public const string PARAM_FATOR_POTENCIA = "FatorPotencia";

        public TipoGerador TipoGerador => (TipoGerador)Tipo!;

        public double PotenciaAparente
        {
            get => Obter<double>(PARAM_POTENCIA_APARENTE);
            set => Definir(PARAM_POTENCIA_APARENTE, value);
        }

        public double FatorPotencia
        {
            get => Obter<double>(PARAM_FATOR_POTENCIA);
            set => Definir(PARAM_FATOR_POTENCIA, value);
        }

        public Gerador()
        {
            DefinirParametro(new Parameter<double>(PARAM_POTENCIA_APARENTE, 0));
            DefinirParametro(new Parameter<double>(PARAM_FATOR_POTENCIA, 0));

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
                {
                    terminais.Add(
                        new Terminal(
                            this,
                            new Point(),
                            IdTerminal(i),
                            TerminalKind.Electrical,
                            DirecaoTerminal(i))
                        {
                            Barra = Barra
                        });
                }
            }

            terminais[0].Barra = Barra;
            terminais[0].DefinirPosicaoLocal(new Point(largura / 2, 0), largura, altura);
            terminais[1].DefinirPosicaoLocal(new Point(largura / 2, altura), largura, altura);
            terminais[2].DefinirPosicaoLocal(new Point(0, altura / 2), largura, altura);
            terminais[3].DefinirPosicaoLocal(new Point(largura, altura / 2), largura, altura);
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

        private static TerminalDirection DirecaoTerminal(int index)
        {
            return index switch
            {
                0 => TerminalDirection.North,
                1 => TerminalDirection.South,
                2 => TerminalDirection.West,
                3 => TerminalDirection.East,
                _ => TerminalDirection.None
            };
        }
    }
}
