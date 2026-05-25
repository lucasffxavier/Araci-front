using System.Collections.Generic;
using System.Windows;

namespace Araci.Models
{
    public abstract class ElementoEquipamento : Elemento, ITerminalOwner
    {
        public const string PARAM_BARRA = "Barra";
        public const string PARAM_BARRA_ID = "BarraId";
        public const string PARAM_ALIMENTADOR = "Alimentador";
        public const string PARAM_POTENCIA_ATIVA = "PotenciaAtiva";
        public const string PARAM_POTENCIA_REATIVA = "PotenciaReativa";
        public const string PARAM_TENSAO_LINHA = "TensaoLinha";
        public const string PARAM_TENSAO_FASE_A = "TensaoFaseA";
        public const string PARAM_TENSAO_FASE_B = "TensaoFaseB";
        public const string PARAM_TENSAO_FASE_C = "TensaoFaseC";
        public const string PARAM_CORRENTE_LINHA = "CorrenteLinha";
        public const string PARAM_CORRENTE_FASE_A = "CorrenteFaseA";
        public const string PARAM_CORRENTE_FASE_B = "CorrenteFaseB";
        public const string PARAM_CORRENTE_FASE_C = "CorrenteFaseC";

        private readonly List<Terminal> _terminais = new();

        protected ElementoEquipamento()
        {
            DefinirParametro(new Parameter<string>(PARAM_NOME, string.Empty));
            DefinirParametro(new Parameter<string>(PARAM_BARRA, string.Empty));
            DefinirParametro(new Parameter<string>(PARAM_BARRA_ID, string.Empty));
            DefinirParametro(new Parameter<int>(PARAM_ALIMENTADOR, 0));
            DefinirParametro(new Parameter<double>(PARAM_POTENCIA_ATIVA, 0));
            DefinirParametro(new Parameter<double>(PARAM_POTENCIA_REATIVA, 0));
            DefinirParametro(new Parameter<string>(PARAM_TENSAO_LINHA, string.Empty));
            DefinirParametro(new Parameter<string>(PARAM_TENSAO_FASE_A, string.Empty));
            DefinirParametro(new Parameter<string>(PARAM_TENSAO_FASE_B, string.Empty));
            DefinirParametro(new Parameter<string>(PARAM_TENSAO_FASE_C, string.Empty));
            DefinirParametro(new Parameter<string>(PARAM_CORRENTE_LINHA, string.Empty));
            DefinirParametro(new Parameter<string>(PARAM_CORRENTE_FASE_A, string.Empty));
            DefinirParametro(new Parameter<string>(PARAM_CORRENTE_FASE_B, string.Empty));
            DefinirParametro(new Parameter<string>(PARAM_CORRENTE_FASE_C, string.Empty));

            CriarTerminalInicial();
        }

        public IReadOnlyList<Terminal> Terminais => _terminais;

        public string Barra
        {
            get => Obter<string>(PARAM_BARRA);
            set
            {
                Definir(PARAM_BARRA, value);

                foreach (Terminal terminal in _terminais)
                    terminal.Barra = value;
            }
        }

        public string BarraId
        {
            get => Obter<string>(PARAM_BARRA_ID);
            set => Definir(PARAM_BARRA_ID, value);
        }

        public int Alimentador
        {
            get => Obter<int>(PARAM_ALIMENTADOR);
            set => Definir(PARAM_ALIMENTADOR, value < 0 ? 0 : value);
        }

        public double PotenciaAtiva
        {
            get => Obter<double>(PARAM_POTENCIA_ATIVA);
            set => Definir(PARAM_POTENCIA_ATIVA, value);
        }

        public double PotenciaReativa
        {
            get => Obter<double>(PARAM_POTENCIA_REATIVA);
            set => Definir(PARAM_POTENCIA_REATIVA, value);
        }

        public string TensaoLinha
        {
            get => Obter<string>(PARAM_TENSAO_LINHA);
            set => Definir(PARAM_TENSAO_LINHA, value);
        }

        public string TensaoFaseA
        {
            get => Obter<string>(PARAM_TENSAO_FASE_A);
            set => Definir(PARAM_TENSAO_FASE_A, value);
        }

        public string TensaoFaseB
        {
            get => Obter<string>(PARAM_TENSAO_FASE_B);
            set => Definir(PARAM_TENSAO_FASE_B, value);
        }

        public string TensaoFaseC
        {
            get => Obter<string>(PARAM_TENSAO_FASE_C);
            set => Definir(PARAM_TENSAO_FASE_C, value);
        }

        public string CorrenteLinha
        {
            get => Obter<string>(PARAM_CORRENTE_LINHA);
            set => Definir(PARAM_CORRENTE_LINHA, value);
        }

        public string CorrenteFaseA
        {
            get => Obter<string>(PARAM_CORRENTE_FASE_A);
            set => Definir(PARAM_CORRENTE_FASE_A, value);
        }

        public string CorrenteFaseB
        {
            get => Obter<string>(PARAM_CORRENTE_FASE_B);
            set => Definir(PARAM_CORRENTE_FASE_B, value);
        }

        public string CorrenteFaseC
        {
            get => Obter<string>(PARAM_CORRENTE_FASE_C);
            set => Definir(PARAM_CORRENTE_FASE_C, value);
        }

        protected List<Terminal> ObterTerminaisInternos()
        {
            return _terminais;
        }

        protected void CopiarEquipamentoPara(ElementoEquipamento destino)
        {
            CopiarBasePara(destino);

            destino._terminais.Clear();

            foreach (var t in _terminais)
            {
                destino._terminais.Add(
                    new Terminal(destino, t.Posicao)
                    {
                        Barra = t.Barra
                    });
            }
        }

        private void CriarTerminalInicial()
        {
            _terminais.Clear();
            _terminais.Add(new Terminal(this, new Point(PosicaoX, PosicaoY)));
        }
    }
}
