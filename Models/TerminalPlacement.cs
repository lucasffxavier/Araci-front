using System;
using System.Windows;

namespace Araci.Models
{
    public static class TerminalPlacement
    {
        public static Point ToWorld(Elemento owner, Point local)
        {
            return ToWorld(owner, local, Size.Empty);
        }

        public static Point ToWorld(Elemento owner, Point local, double width, double height)
        {
            return ToWorld(owner, local, new Size(width, height));
        }

        public static Point ToWorld(Elemento owner, Point local, Size size)
        {
            double scale = owner.Escala == 0 ? 1 : owner.Escala;
            Point pivot = ObterPivo(size);
            double x = (local.X - pivot.X) * scale;
            double y = (local.Y - pivot.Y) * scale;

            if (Math.Abs(owner.Rotacao) > 0.000001)
            {
                double radians = owner.Rotacao * Math.PI / 180.0;
                double cos = Math.Cos(radians);
                double sin = Math.Sin(radians);
                double rotatedX = x * cos - y * sin;
                double rotatedY = x * sin + y * cos;

                x = rotatedX;
                y = rotatedY;
            }

            return new Point(
                owner.PosicaoX + pivot.X + x,
                owner.PosicaoY + pivot.Y + y);
        }

        public static Point ToLocal(Elemento owner, Point world)
        {
            return ToLocal(owner, world, Size.Empty);
        }

        public static Point ToLocal(Elemento owner, Point world, double width, double height)
        {
            return ToLocal(owner, world, new Size(width, height));
        }

        public static Point ToLocal(Elemento owner, Point world, Size size)
        {
            Point pivot = ObterPivo(size);
            double x = world.X - owner.PosicaoX - pivot.X;
            double y = world.Y - owner.PosicaoY - pivot.Y;

            if (Math.Abs(owner.Rotacao) > 0.000001)
            {
                double radians = -owner.Rotacao * Math.PI / 180.0;
                double cos = Math.Cos(radians);
                double sin = Math.Sin(radians);
                double rotatedX = x * cos - y * sin;
                double rotatedY = x * sin + y * cos;

                x = rotatedX;
                y = rotatedY;
            }

            double scale = owner.Escala == 0 ? 1 : owner.Escala;
            return new Point(
                pivot.X + x / scale,
                pivot.Y + y / scale);
        }

        private static Point ObterPivo(Size size)
        {
            if (size.IsEmpty ||
                size.Width <= 0 ||
                size.Height <= 0)
            {
                return new Point();
            }

            return new Point(size.Width / 2, size.Height / 2);
        }
    }
}
