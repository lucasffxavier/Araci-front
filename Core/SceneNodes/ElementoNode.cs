using System.Windows;
using Araci.Models;

namespace Araci.Core.SceneNodes
{
    public abstract class ElementoNode
    {
        protected ElementoNode(Elemento modelo)
        {
            Modelo = modelo;
        }

        public Elemento Modelo { get; }
        public Rect Bounds { get; protected set; }
        public virtual Rect BoundsAlinhamento => Bounds;

        public Point Centro => new(
            Bounds.X + Bounds.Width / 2,
            Bounds.Y + Bounds.Height / 2);

        public virtual double Largura => Bounds.Width;
        public virtual double Altura => Bounds.Height;

        public virtual double X
        {
            get => Modelo.PosicaoX;
            set => Modelo.PosicaoX = value;
        }

        public virtual double Y
        {
            get => Modelo.PosicaoY;
            set => Modelo.PosicaoY = value;
        }

        public virtual void Mover(Vector delta)
        {
            Modelo.PosicaoX += delta.X;
            Modelo.PosicaoY += delta.Y;
            AtualizarGeometria();
        }

        public abstract void AtualizarGeometria();
    }
}