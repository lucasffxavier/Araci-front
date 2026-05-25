using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Cabo : ElementoLinear, ITerminalOwner
    {
        public const string PARAM_ORIGEM_ID = "OrigemId";
        public const string PARAM_DESTINO_ID = "DestinoId";
        public const string PARAM_BARRA_ORIGEM = "BarraOrigem";
        public const string PARAM_BARRA_DESTINO = "BarraDestino";
        public const string PARAM_COMPRIMENTO = "Comprimento";
        public const string PARAM_AMPACIDADE = "Ampacidade";
        public const string PARAM_TENSAO_LINHA = "TensaoLinha";
        public const string PARAM_TENSAO_FASE_A = "TensaoFaseA";
        public const string PARAM_TENSAO_FASE_B = "TensaoFaseB";
        public const string PARAM_TENSAO_FASE_C = "TensaoFaseC";
        public const string PARAM_CORRENTE_LINHA = "CorrenteLinha";
        public const string PARAM_CORRENTE_FASE_A = "CorrenteFaseA";
        public const string PARAM_CORRENTE_FASE_B = "CorrenteFaseB";
        public const string PARAM_CORRENTE_FASE_C = "CorrenteFaseC";

        private readonly List<Terminal> _terminais = new();

        public Cabo()
        {
            DefinirParametro(new Parameter<string>(PARAM_ORIGEM_ID, string.Empty));
            DefinirParametro(new Parameter<string>(PARAM_DESTINO_ID, string.Empty));
            DefinirParametro(new Parameter<string>(PARAM_BARRA_ORIGEM, "GERADOR-001"));
            DefinirParametro(new Parameter<string>(PARAM_BARRA_DESTINO, "CARGA-001"));
            DefinirParametro(new Parameter<double>(PARAM_COMPRIMENTO, 1));
            DefinirParametro(new Parameter<double>(PARAM_AMPACIDADE, 520));
            DefinirParametro(new Parameter<string>(PARAM_TENSAO_LINHA, "12.47∠0°"));
            DefinirParametro(new Parameter<string>(PARAM_TENSAO_FASE_A, "7.2∠0°"));
            DefinirParametro(new Parameter<string>(PARAM_TENSAO_FASE_B, "7.2∠-120°"));
            DefinirParametro(new Parameter<string>(PARAM_TENSAO_FASE_C, "7.2∠120°"));
            DefinirParametro(new Parameter<string>(PARAM_CORRENTE_LINHA, "0∠0°"));
            DefinirParametro(new Parameter<string>(PARAM_CORRENTE_FASE_A, "0∠0°"));
            DefinirParametro(new Parameter<string>(PARAM_CORRENTE_FASE_B, "0∠-120°"));
            DefinirParametro(new Parameter<string>(PARAM_CORRENTE_FASE_C, "0∠120°"));

            Nome = "L1";
        }

        public IReadOnlyList<Terminal> Terminais => _terminais;

        public ObservableCollection<Point> Vertices { get; } = new();

        public Point? PreviewPonto { get; set; }

        public TipoCabo TipoCabo => (TipoCabo)Tipo!;

        public Terminal? Origem => _terminais.Count > 0 ? _terminais[0] : null;

        public Terminal? Destino => _terminais.Count > 1 ? _terminais[1] : null;

        public string OrigemId
        {
            get => Obter<string>(PARAM_ORIGEM_ID);
            set => Definir(PARAM_ORIGEM_ID, value);
        }

        public string DestinoId
        {
            get => Obter<string>(PARAM_DESTINO_ID);
            set => Definir(PARAM_DESTINO_ID, value);
        }

        public string BarraOrigem
        {
            get => Obter<string>(PARAM_BARRA_ORIGEM);
            set => Definir(PARAM_BARRA_ORIGEM, value);
        }

        public string BarraDestino
        {
            get => Obter<string>(PARAM_BARRA_DESTINO);
            set => Definir(PARAM_BARRA_DESTINO, value);
        }

        public new double Comprimento
        {
            get => Obter<double>(PARAM_COMPRIMENTO);
            set => Definir(PARAM_COMPRIMENTO, value);
        }

        public double Ampacidade
        {
            get => Obter<double>(PARAM_AMPACIDADE);
            set => Definir(PARAM_AMPACIDADE, value);
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

        public void DefinirOrigem(Point p)
        {
            if (_terminais.Count == 0)
                _terminais.Add(new Terminal(this, p) { Barra = BarraOrigem });
            else
                _terminais[0].Posicao = p;
        }

        public void DefinirDestino(Point p)
        {
            if (_terminais.Count < 2)
                _terminais.Add(new Terminal(this, p) { Barra = BarraDestino });
            else
                _terminais[1].Posicao = p;
        }

        public override Elemento Clonar()
        {
            var clone = new Cabo();

            CopiarLinearPara(clone);

            foreach (Point p in Vertices)
                clone.Vertices.Add(p);

            clone.PreviewPonto = PreviewPonto;
            clone._terminais.Clear();

            foreach (Terminal t in _terminais)
            {
                clone._terminais.Add(
                    new Terminal(clone, t.Posicao)
                    {
                        Barra = t.Barra
                    });
            }

            return clone;
        }
    }
}
