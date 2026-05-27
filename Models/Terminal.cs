using System.Windows;

namespace Araci.Models
{
    public class Terminal
    {
        public Elemento Dono { get; }
        public string Id { get; }
        public Point Posicao { get; set; }
        public Point PosicaoLocal { get; private set; }
        public string? Barra { get; set; }
        public TerminalKind Kind { get; set; }
        public TerminalDirection Direction { get; set; }

        public Terminal(
            Elemento dono,
            Point posicao,
            string? id = null,
            TerminalKind kind = TerminalKind.Electrical,
            TerminalDirection direction = TerminalDirection.None)
        {
            Dono = dono;
            Id = string.IsNullOrWhiteSpace(id)
                ? System.Guid.NewGuid().ToString("N")
                : id;
            Kind = kind;
            Direction = direction;
            Posicao = posicao;
            PosicaoLocal = TerminalPlacement.ToLocal(dono, posicao);
        }

        public TerminalEndpoint Endpoint =>
            TerminalEndpoint.FromTerminal(this);

        public void DefinirPosicaoLocal(Point local)
        {
            PosicaoLocal = local;
            Posicao = TerminalPlacement.ToWorld(Dono, local);
        }

        public void DefinirPosicaoVisual(Point world)
        {
            Posicao = world;
            PosicaoLocal = TerminalPlacement.ToLocal(Dono, world);
        }
    }
}
