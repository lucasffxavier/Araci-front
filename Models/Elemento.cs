using System;

using Araci.Models.Interfaces;

namespace Araci.Models
{
    public abstract class Elemento : IElementoClonavel
    {
        // =========================
        // POSIÇÃO
        // =========================

        public double PosicaoX { get; set; }

        public double PosicaoY { get; set; }

        // =========================
        // IDENTIFICAÇÃO
        // =========================

        public string Nome { get; set; }
            = string.Empty;

        public Guid Id { get; set; }
            = Guid.NewGuid();

        // =========================
        // TRANSFORMAÇÃO
        // =========================

        public double Rotacao { get; set; }

        public double Escala { get; set; }
            = 1;

        // =========================
        // BIM
        // =========================

        public string Familia { get; set; }
            = string.Empty;

        public string Categoria { get; set; }
            = string.Empty;

        // =========================
        // CLONAGEM
        // =========================

        public abstract Elemento Clonar();

        protected void CopiarBasePara(Elemento destino)
        {
            destino.Id = Guid.NewGuid();

            destino.Nome = Nome;

            destino.PosicaoX = PosicaoX;
            destino.PosicaoY = PosicaoY;

            destino.Rotacao = Rotacao;
            destino.Escala = Escala;

            destino.Familia = Familia;
            destino.Categoria = Categoria;
        }
    }
}