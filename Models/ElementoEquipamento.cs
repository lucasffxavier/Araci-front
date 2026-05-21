using System.Collections.Generic;
using System.Windows;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public abstract class ElementoEquipamento : Elemento, ITerminalOwner
    {
        public const string PARAM_BARRA = "Barra";
        public const string PARAM_ALIMENTADOR = "Alimentador";
        public const string PARAM_POTENCIA_KW = "PotenciaAtivaKW";

        private readonly List<Terminal> _terminais = new();

        public IReadOnlyList<Terminal> Terminais => _terminais;

        protected ElementoEquipamento()
        {
            DefinirParametro(new Parameter<string>(PARAM_NOME, string.Empty));
            DefinirParametro(new Parameter<string>(PARAM_BARRA, string.Empty));
            DefinirParametro(new Parameter<string>(PARAM_ALIMENTADOR, string.Empty));
            DefinirParametro(new Parameter<double>(PARAM_POTENCIA_KW, 0));

            CriarTerminalInicial();
        }

        private void CriarTerminalInicial()
        {
            _terminais.Clear();
            _terminais.Add(new Terminal(this, new Point(PosicaoX, PosicaoY)));
        }

        // 🔥 NOVO: acesso controlado para classes derivadas
        protected List<Terminal> ObterTerminaisInternos()
        {
            return _terminais;
        }

        public string Barra
        {
            get => Obter<string>(PARAM_BARRA);
            set => Definir(PARAM_BARRA, value);
        }

        public string Alimentador
        {
            get => Obter<string>(PARAM_ALIMENTADOR);
            set => Definir(PARAM_ALIMENTADOR, value);
        }

        public double PotenciaAtivaKW
        {
            get => Obter<double>(PARAM_POTENCIA_KW);
            set => Definir(PARAM_POTENCIA_KW, value);
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
    }
}