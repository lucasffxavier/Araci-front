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
        public const string PARAM_ORIGEM_TERMINAL_ID = "OrigemTerminalId";
        public const string PARAM_DESTINO_TERMINAL_ID = "DestinoTerminalId";
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
            DefinirParametro(new Parameter<string>(PARAM_ORIGEM_TERMINAL_ID, string.Empty));
            DefinirParametro(new Parameter<string>(PARAM_DESTINO_TERMINAL_ID, string.Empty));
            DefinirParametro(new Parameter<string>(PARAM_BARRA_ORIGEM, "GERADOR-001"));
            DefinirParametro(new Parameter<string>(PARAM_BARRA_DESTINO, "CARGA-001"));
            DefinirParametro(new Parameter<double>(PARAM_COMPRIMENTO, 1));
            DefinirParametro(new Parameter<double>(PARAM_AMPACIDADE, 520));
            DefinirParametro(new Parameter<string>(PARAM_TENSAO_LINHA, "12,47∠0°"));
            DefinirParametro(new Parameter<string>(PARAM_TENSAO_FASE_A, "7,2∠0°"));
            DefinirParametro(new Parameter<string>(PARAM_TENSAO_FASE_B, "7,2∠-120°"));
            DefinirParametro(new Parameter<string>(PARAM_TENSAO_FASE_C, "7,2∠120°"));
            DefinirParametro(new Parameter<string>(PARAM_CORRENTE_LINHA, "0∠0°"));
            DefinirParametro(new Parameter<string>(PARAM_CORRENTE_FASE_A, "0∠0°"));
            DefinirParametro(new Parameter<string>(PARAM_CORRENTE_FASE_B, "0∠-120°"));
            DefinirParametro(new Parameter<string>(PARAM_CORRENTE_FASE_C, "0∠120°"));
            Nome = "L1";
        }

        public IReadOnlyList<Terminal> Terminais => _terminais;
        public override ElementoDomainRole DomainRole => ElementoDomainRole.EletricoTopologico;
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

        public string OrigemTerminalId
        {
            get => Obter<string>(PARAM_ORIGEM_TERMINAL_ID);
            set => Definir(PARAM_ORIGEM_TERMINAL_ID, value);
        }

        public string DestinoTerminalId
        {
            get => Obter<string>(PARAM_DESTINO_TERMINAL_ID);
            set => Definir(PARAM_DESTINO_TERMINAL_ID, value);
        }

        public TerminalEndpoint OrigemEndpoint => new(OrigemId, OrigemTerminalId);
        public TerminalEndpoint DestinoEndpoint => new(DestinoId, DestinoTerminalId);

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

        public bool PossuiOrigemConectada => OrigemEndpoint.IsComplete;
        public bool PossuiDestinoConectado => DestinoEndpoint.IsComplete;
        public bool PossuiDuasPontasConectadas => PossuiOrigemConectada && PossuiDestinoConectado;
        public bool PossuiVerticesIntermediarios => Vertices.Count > 2;

        public bool MoverPreservandoAncoras(Vector delta)
        {
            if (delta.X == 0 && delta.Y == 0 || Vertices.Count == 0)
                return false;

            int inicio = PossuiOrigemConectada ? 1 : 0;
            int fimExclusivo = PossuiDestinoConectado ? Vertices.Count - 1 : Vertices.Count;

            if (inicio >= fimExclusivo)
                return false;

            for (int i = inicio; i < fimExclusivo; i++)
            {
                Point p = Vertices[i];
                Vertices[i] = new Point(p.X + delta.X, p.Y + delta.Y);
            }

            if (PreviewPonto.HasValue && !PossuiDestinoConectado)
            {
                Point preview = PreviewPonto.Value;
                PreviewPonto = new Point(preview.X + delta.X, preview.Y + delta.Y);
            }

            AtualizarTerminaisPelasPontas();
            return true;
        }

        public void AtualizarTerminaisPelasPontas()
        {
            if (Vertices.Count > 0)
                DefinirOrigem(Vertices[0]);

            if (Vertices.Count > 1)
                DefinirDestino(Vertices[^1]);
        }

        public void DefinirOrigem(Point p)
        {
            if (_terminais.Count == 0)
            {
                _terminais.Add(new Terminal(this, p, "ORIGEM", TerminalKind.CableEnd, TerminalDirection.West) { Barra = BarraOrigem });
            }
            else
            {
                _terminais[0].DefinirPosicaoVisual(p);
                _terminais[0].Barra = BarraOrigem;
            }
        }

        public void DefinirDestino(Point p)
        {
            if (_terminais.Count < 2)
            {
                _terminais.Add(new Terminal(this, p, "DESTINO", TerminalKind.CableEnd, TerminalDirection.East) { Barra = BarraDestino });
            }
            else
            {
                _terminais[1].DefinirPosicaoVisual(p);
                _terminais[1].Barra = BarraDestino;
            }
        }

        public override Elemento Clonar()
        {
            var clone = new Cabo();
            CopiarLinearPara(clone);
            clone.OrigemId = OrigemId;
            clone.DestinoId = DestinoId;
            clone.OrigemTerminalId = OrigemTerminalId;
            clone.DestinoTerminalId = DestinoTerminalId;
            clone.BarraOrigem = BarraOrigem;
            clone.BarraDestino = BarraDestino;
            clone.Comprimento = Comprimento;
            clone.Ampacidade = Ampacidade;
            clone.TensaoLinha = TensaoLinha;
            clone.TensaoFaseA = TensaoFaseA;
            clone.TensaoFaseB = TensaoFaseB;
            clone.TensaoFaseC = TensaoFaseC;
            clone.CorrenteLinha = CorrenteLinha;
            clone.CorrenteFaseA = CorrenteFaseA;
            clone.CorrenteFaseB = CorrenteFaseB;
            clone.CorrenteFaseC = CorrenteFaseC;

            foreach (Point p in Vertices)
                clone.Vertices.Add(p);

            clone.PreviewPonto = PreviewPonto;
            clone._terminais.Clear();

            foreach (Terminal t in _terminais)
            {
                clone._terminais.Add(new Terminal(clone, t.Posicao, t.Id, t.Kind, t.Direction)
                {
                    Barra = t.Barra
                });
            }

            return clone;
        }
    }
}