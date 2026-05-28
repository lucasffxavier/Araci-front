using System.Linq;
using System.Windows;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Transformador : ElementoEquipamento
    {
        public const string TERMINAL_PRIMARIO = "PRIMARIO";
        public const string TERMINAL_SECUNDARIO = "SECUNDARIO";
        public const string PARAM_FASES = "Fases";
        public const string PARAM_ENROLAMENTOS = "Enrolamentos";
        public const string PARAM_TENSAO_PRIMARIO_KV = "TensaoPrimarioKV";
        public const string PARAM_TENSAO_SECUNDARIO_KV = "TensaoSecundarioKV";
        public const string PARAM_POTENCIA_APARENTE = "PotenciaAparente";
        public const string PARAM_R_PERCENTUAL = "RPercentual";
        public const string PARAM_X_PERCENTUAL = "XPercentual";
        public const string PARAM_LIGACAO_PRIMARIO = "LigacaoPrimario";
        public const string PARAM_LIGACAO_SECUNDARIO = "LigacaoSecundario";

        public TipoTransformador TipoTransformador => (TipoTransformador)Tipo!;

        public int Fases
        {
            get => Obter<int>(PARAM_FASES);
            set => Definir(PARAM_FASES, value <= 0 ? 3 : value);
        }

        public int Enrolamentos
        {
            get => Obter<int>(PARAM_ENROLAMENTOS);
            set => Definir(PARAM_ENROLAMENTOS, value <= 0 ? 2 : value);
        }

        public double TensaoPrimarioKV
        {
            get => Obter<double>(PARAM_TENSAO_PRIMARIO_KV);
            set => Definir(PARAM_TENSAO_PRIMARIO_KV, value > 0 ? value : 13.8);
        }

        public double TensaoSecundarioKV
        {
            get => Obter<double>(PARAM_TENSAO_SECUNDARIO_KV);
            set => Definir(PARAM_TENSAO_SECUNDARIO_KV, value > 0 ? value : 0.38);
        }

        public double PotenciaAparente
        {
            get => Obter<double>(PARAM_POTENCIA_APARENTE);
            set => Definir(PARAM_POTENCIA_APARENTE, value > 0 ? value : 500);
        }

        public double RPercentual
        {
            get => Obter<double>(PARAM_R_PERCENTUAL);
            set => Definir(PARAM_R_PERCENTUAL, value < 0 ? 0 : value);
        }

        public double XPercentual
        {
            get => Obter<double>(PARAM_X_PERCENTUAL);
            set => Definir(PARAM_X_PERCENTUAL, value < 0 ? 0 : value);
        }

        public string LigacaoPrimario
        {
            get => Obter<string>(PARAM_LIGACAO_PRIMARIO);
            set => Definir(PARAM_LIGACAO_PRIMARIO, string.IsNullOrWhiteSpace(value) ? "Wye" : value);
        }

        public string LigacaoSecundario
        {
            get => Obter<string>(PARAM_LIGACAO_SECUNDARIO);
            set => Definir(PARAM_LIGACAO_SECUNDARIO, string.IsNullOrWhiteSpace(value) ? "Wye" : value);
        }

        public Transformador()
        {
            DefinirParametro(new Parameter<int>(PARAM_FASES, 3));
            DefinirParametro(new Parameter<int>(PARAM_ENROLAMENTOS, 2));
            DefinirParametro(new Parameter<double>(PARAM_TENSAO_PRIMARIO_KV, 13.8));
            DefinirParametro(new Parameter<double>(PARAM_TENSAO_SECUNDARIO_KV, 0.38));
            DefinirParametro(new Parameter<double>(PARAM_POTENCIA_APARENTE, 500));
            DefinirParametro(new Parameter<double>(PARAM_R_PERCENTUAL, 1));
            DefinirParametro(new Parameter<double>(PARAM_X_PERCENTUAL, 5));
            DefinirParametro(new Parameter<string>(PARAM_LIGACAO_PRIMARIO, "Wye"));
            DefinirParametro(new Parameter<string>(PARAM_LIGACAO_SECUNDARIO, "Wye"));

            Nome = "TR-001";
            Barra = "TR-001";
            Alimentador = 1;
            CorrenteLinha = "0";
            CorrenteFaseA = "0";
            CorrenteFaseB = "0";
            CorrenteFaseC = "0";
            PosicaoX = 260;
            PosicaoY = 180;
        }

        public void AtualizarTerminais(double largura, double altura)
        {
            var terminais = ObterTerminaisInternos();

            if (!TerminaisPadraoPresentes())
            {
                terminais.Clear();
                terminais.Add(new Terminal(this, new Point(), TERMINAL_PRIMARIO, TerminalKind.Electrical, TerminalDirection.North));
                terminais.Add(new Terminal(this, new Point(), TERMINAL_SECUNDARIO, TerminalKind.Electrical, TerminalDirection.South));
            }

            AtualizarTerminal(TERMINAL_PRIMARIO, new Point(largura / 2, 0), largura, altura);
            AtualizarTerminal(TERMINAL_SECUNDARIO, new Point(largura / 2, altura), largura, altura);
        }

        private bool TerminaisPadraoPresentes()
        {
            var terminais = ObterTerminaisInternos();

            return terminais.Count == 2 &&
                terminais[0].Id == TERMINAL_PRIMARIO &&
                terminais[1].Id == TERMINAL_SECUNDARIO;
        }

        private void AtualizarTerminal(string id, Point posicao, double largura, double altura)
        {
            var terminal = ObterTerminaisInternos().First(t => t.Id == id);
            terminal.Barra = Barra;
            terminal.DefinirPosicaoLocal(posicao, largura, altura);
        }

        public override Elemento Clonar()
        {
            var clone = new Transformador();
            CopiarEquipamentoPara(clone);
            return clone;
        }
    }
}