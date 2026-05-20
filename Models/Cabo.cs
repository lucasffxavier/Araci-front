using System.Collections.ObjectModel;
using System.Windows;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Cabo
        : ElementoLinear
    {
        public const string PARAM_BARRA_ORIGEM =
            "BarraOrigem";

        public const string PARAM_BARRA_DESTINO =
            "BarraDestino";

        public const string PARAM_COMPRIMENTO =
            "Comprimento";

        public ObservableCollection<Point>
            Vertices
        { get; }
            = new();

        public Point? PreviewPonto
        { get; set; }

        public TipoCabo TipoCabo =>
            (TipoCabo)Tipo!;

        public string BarraOrigem
        {
            get => Obter<string>(
                PARAM_BARRA_ORIGEM);

            set => Definir(
                PARAM_BARRA_ORIGEM,
                value);
        }

        public string BarraDestino
        {
            get => Obter<string>(
                PARAM_BARRA_DESTINO);

            set => Definir(
                PARAM_BARRA_DESTINO,
                value);
        }

        public new double Comprimento
        {
            get => Obter<double>(
                PARAM_COMPRIMENTO);

            set => Definir(
                PARAM_COMPRIMENTO,
                value);
        }

        public Cabo()
        {
            Nome = "CB-01";

            DefinirParametro(
                new Parameter<string>(
                    PARAM_BARRA_ORIGEM,
                    "BUS-01"));

            DefinirParametro(
                new Parameter<string>(
                    PARAM_BARRA_DESTINO,
                    "BUS-02"));

            DefinirParametro(
                new Parameter<double>(
                    PARAM_COMPRIMENTO,
                    120));
        }

        public override Elemento Clonar()
        {
            var clone = new Cabo();

            CopiarLinearPara(clone);

            CopiarBasePara(clone);

            foreach (Point p in Vertices)
            {
                clone.Vertices.Add(p);
            }

            clone.PreviewPonto =
                PreviewPonto;

            return clone;
        }
    }
}