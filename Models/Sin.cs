using System.Linq;
using System.Windows;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Sin : ElementoEquipamento
    {
        public const string TERMINAL_NORTE = "NORTE";
        public const string TERMINAL_SUL = "SUL";
        public const string TERMINAL_LESTE = "LESTE";
        public const string TERMINAL_OESTE = "OESTE";

        public TipoSin TipoSin => (TipoSin)Tipo!;

        public Sin()
        {
            Nome = "SIN-001";
            Barra = "SIN-001";
            Alimentador = 1;
            TensaoLinha = "12.47";
            TensaoFaseA = "7.2";
            TensaoFaseB = "7.2";
            TensaoFaseC = "7.2";
            CorrenteLinha = "0";
            CorrenteFaseA = "0";
            CorrenteFaseB = "0";
            CorrenteFaseC = "0";

            PosicaoX = 200;
            PosicaoY = 160;
        }

        public void AtualizarTerminais(double largura, double altura)
        {
            var terminais = ObterTerminaisInternos();

            if (!TerminaisPadraoPresentes())
            {
                terminais.Clear();
                terminais.Add(new Terminal(this, new Point(), TERMINAL_NORTE, TerminalKind.Electrical, TerminalDirection.North));
                terminais.Add(new Terminal(this, new Point(), TERMINAL_SUL, TerminalKind.Electrical, TerminalDirection.South));
                terminais.Add(new Terminal(this, new Point(), TERMINAL_LESTE, TerminalKind.Electrical, TerminalDirection.East));
                terminais.Add(new Terminal(this, new Point(), TERMINAL_OESTE, TerminalKind.Electrical, TerminalDirection.West));
            }

            AtualizarTerminal(TERMINAL_NORTE, new Point(largura / 2, 0), largura, altura);
            AtualizarTerminal(TERMINAL_SUL, new Point(largura / 2, altura), largura, altura);
            AtualizarTerminal(TERMINAL_LESTE, new Point(largura, altura / 2), largura, altura);
            AtualizarTerminal(TERMINAL_OESTE, new Point(0, altura / 2), largura, altura);
        }

        private bool TerminaisPadraoPresentes()
        {
            var terminais = ObterTerminaisInternos();

            return terminais.Count == 4 &&
                terminais[0].Id == TERMINAL_NORTE &&
                terminais[1].Id == TERMINAL_SUL &&
                terminais[2].Id == TERMINAL_LESTE &&
                terminais[3].Id == TERMINAL_OESTE;
        }

        private void AtualizarTerminal(string id, Point posicao, double largura, double altura)
        {
            var terminal = ObterTerminaisInternos().First(t => t.Id == id);
            terminal.Barra = Barra;
            terminal.DefinirPosicaoLocal(posicao, largura, altura);
        }

        public override Elemento Clonar()
        {
            var clone = new Sin();

            CopiarEquipamentoPara(clone);

            return clone;
        }
    }
}
