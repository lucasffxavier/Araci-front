// Models/Cabo.cs

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Cabo : ElementoLinear, ITerminalOwner
    {
        public const string PARAM_BARRA_ORIGEM =
            "BarraOrigem";

        public const string PARAM_BARRA_DESTINO =
            "BarraDestino";

        public const string PARAM_COMPRIMENTO =
            "Comprimento";

        private readonly List<Terminal> _terminais = new();

        public IReadOnlyList<Terminal> Terminais =>
            _terminais;

        public ObservableCollection<Point> Vertices { get; } =
            new();

        public Point? PreviewPonto { get; set; }

        public TipoCabo TipoCabo =>
            (TipoCabo)Tipo!;

        public Terminal? Origem =>
            _terminais.Count > 0
                ? _terminais[0]
                : null;

        public Terminal? Destino =>
            _terminais.Count > 1
                ? _terminais[1]
                : null;

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

        public Cabo()
        {
            DefinirParametro(
                new Parameter<string>(
                    PARAM_BARRA_ORIGEM,
                    string.Empty));

            DefinirParametro(
                new Parameter<string>(
                    PARAM_BARRA_DESTINO,
                    string.Empty));

            DefinirParametro(
                new Parameter<double>(
                    PARAM_COMPRIMENTO,
                    120));
        }

        public void DefinirOrigem(Point p)
        {
            if (_terminais.Count == 0)
                _terminais.Add(
                    new Terminal(this, p));
            else
                _terminais[0].Posicao = p;
        }

        public void DefinirDestino(Point p)
        {
            if (_terminais.Count < 2)
                _terminais.Add(
                    new Terminal(this, p));
            else
                _terminais[1].Posicao = p;
        }

        public override Elemento Clonar()
        {
            var clone = new Cabo();

            CopiarLinearPara(clone);

            CopiarBasePara(clone);

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