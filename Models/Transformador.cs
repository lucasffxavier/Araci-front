using System.Linq;
using System.Windows;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Transformador : ElementoEquipamento
    {
        public const string TERMINAL_PRIMARIO = "PRIMARIO";
        public const string TERMINAL_SECUNDARIO = "SECUNDARIO";

        public TipoTransformador TipoTransformador => (TipoTransformador)Tipo!;

        public Transformador()
        {
            Nome = "TR-001";
            Barra = "TR-001";
            Alimentador = 1;
            TensaoLinha = "13.8";
            TensaoFaseA = "7.97";
            TensaoFaseB = "7.97";
            TensaoFaseC = "7.97";
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
                terminais.Add(new Terminal(this, new Point(), TERMINAL_PRIMARIO));
                terminais.Add(new Terminal(this, new Point(), TERMINAL_SECUNDARIO));
            }

            AtualizarTerminal(TERMINAL_PRIMARIO, new Point(PosicaoX + largura / 2, PosicaoY));
            AtualizarTerminal(TERMINAL_SECUNDARIO, new Point(PosicaoX + largura / 2, PosicaoY + altura));
        }

        private bool TerminaisPadraoPresentes()
        {
            var terminais = ObterTerminaisInternos();

            return terminais.Count == 2 &&
                terminais[0].Id == TERMINAL_PRIMARIO &&
                terminais[1].Id == TERMINAL_SECUNDARIO;
        }

        private void AtualizarTerminal(string id, Point posicao)
        {
            var terminal = ObterTerminaisInternos().First(t => t.Id == id);
            terminal.Barra = Barra;
            terminal.Posicao = posicao;
        }

        public override Elemento Clonar()
        {
            var clone = new Transformador();

            CopiarEquipamentoPara(clone);

            return clone;
        }
    }
}
