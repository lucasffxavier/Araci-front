using System;
using System.Linq;
using System.Windows;
using Araci.Core.Commands;
using Araci.Core.Rendering;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Services
{
    public class BarraResizeService
    {
        private const double HandleTolerance = 12;
        private readonly EditorContext _context;
        private BarraViewModel? _vm;
        private ResizeBarraHandle _handle;
        private Point _startMouse;
        private Point _startTopWorld;
        private Point _startBottomWorld;
        private double _startX;
        private double _startY;
        private double _startHeight;

        public BarraResizeService(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public bool IsResizing { get; private set; }

        public bool TryBegin(Point position)
        {
            if (_context.Selection.Selecionados.Count != 1)
                return false;
            if (_context.Selection.Selecionados.FirstOrDefault() is not BarraViewModel barraVm)
                return false;
            ResizeBarraHandle handle = HitTestHandle(barraVm, position);
            if (handle == ResizeBarraHandle.None)
                return false;
            _vm = barraVm;
            _handle = handle;
            _startMouse = position;
            _startX = barraVm.Barra.PosicaoX;
            _startY = barraVm.Barra.PosicaoY;
            _startHeight = barraVm.Barra.Altura;
            _startTopWorld = GetTopHandleWorld(barraVm.Barra);
            _startBottomWorld = GetBottomHandleWorld(barraVm.Barra);
            IsResizing = true;
            return true;
        }

        public void Update(Point position)
        {
            if (!IsResizing || _vm == null)
                return;
            Vector eixoLocal = GetLocalVerticalAxis(_vm.Barra.Rotacao);
            Vector mouseDelta = position - _startMouse;
            double delta = Dot(mouseDelta, eixoLocal);
            double novaAltura = _handle == ResizeBarraHandle.Top ? _startHeight - delta : _startHeight + delta;
            novaAltura = Barra.NormalizarAltura(novaAltura);
            Point novoTopoEsquerdo = _handle == ResizeBarraHandle.Top
                ? CalcularTopoEsquerdoMantendoBottom(_vm.Barra, novaAltura, _startBottomWorld)
                : CalcularTopoEsquerdoMantendoTop(_vm.Barra, novaAltura, _startTopWorld);
            _vm.Barra.PosicaoX = novoTopoEsquerdo.X;
            _vm.Barra.PosicaoY = novoTopoEsquerdo.Y;
            _context.GeometryUpdates.AplicarAlturaBarra(_vm.Barra, novaAltura);
            _vm.NotificarPropriedades(nameof(BarraViewModel.Altura), nameof(BarraViewModel.X), nameof(BarraViewModel.Y), nameof(BarraViewModel.Bounds));
        }

        public void End()
        {
            if (!IsResizing || _vm == null)
            {
                Cancel();
                return;
            }
            double alturaDepois = _vm.Barra.Altura;
            double xDepois = _vm.Barra.PosicaoX;
            double yDepois = _vm.Barra.PosicaoY;
            bool mudou = Math.Abs(_startHeight - alturaDepois) > 0.0001 || Math.Abs(_startX - xDepois) > 0.0001 || Math.Abs(_startY - yDepois) > 0.0001;
            if (mudou)
            {
                _context.Commands.Execute(new ResizeBarraCommand(_vm.Barra, _startHeight, _startX, _startY, alturaDepois, xDepois, yDepois, _context.GeometryUpdates));
            }
            Limpar();
        }

        public void Cancel()
        {
            if (_vm != null)
            {
                _vm.Barra.PosicaoX = _startX;
                _vm.Barra.PosicaoY = _startY;
                _context.GeometryUpdates.AplicarAlturaBarra(_vm.Barra, _startHeight);
            }
            Limpar();
        }

        private void Limpar()
        {
            IsResizing = false;
            _vm = null;
            _handle = ResizeBarraHandle.None;
        }

        private static ResizeBarraHandle HitTestHandle(BarraViewModel vm, Point position)
        {
            Point top = GetTopHandleWorld(vm.Barra);
            Point bottom = GetBottomHandleWorld(vm.Barra);
            if (Distance(position, top) <= HandleTolerance)
                return ResizeBarraHandle.Top;
            if (Distance(position, bottom) <= HandleTolerance)
                return ResizeBarraHandle.Bottom;
            return ResizeBarraHandle.None;
        }

        private static Point GetTopHandleWorld(Barra barra)
        {
            return RotateAround(new Point(barra.PosicaoX + ElementGeometryDefaults.BarraLargura / 2, barra.PosicaoY), GetCenter(barra), barra.Rotacao);
        }

        private static Point GetBottomHandleWorld(Barra barra)
        {
            return RotateAround(new Point(barra.PosicaoX + ElementGeometryDefaults.BarraLargura / 2, barra.PosicaoY + barra.Altura), GetCenter(barra), barra.Rotacao);
        }

        private static Point GetCenter(Barra barra)
        {
            return new Point(barra.PosicaoX + ElementGeometryDefaults.BarraLargura / 2, barra.PosicaoY + barra.Altura / 2);
        }

        private static Point CalcularTopoEsquerdoMantendoTop(Barra barra, double novaAltura, Point topWorld)
        {
            Vector offsetCentroParaTop = RotateVector(new Vector(0, -novaAltura / 2), barra.Rotacao);
            Point novoCentro = topWorld - offsetCentroParaTop;
            return new Point(novoCentro.X - ElementGeometryDefaults.BarraLargura / 2, novoCentro.Y - novaAltura / 2);
        }

        private static Point CalcularTopoEsquerdoMantendoBottom(Barra barra, double novaAltura, Point bottomWorld)
        {
            Vector offsetCentroParaBottom = RotateVector(new Vector(0, novaAltura / 2), barra.Rotacao);
            Point novoCentro = bottomWorld - offsetCentroParaBottom;
            return new Point(novoCentro.X - ElementGeometryDefaults.BarraLargura / 2, novoCentro.Y - novaAltura / 2);
        }

        private static Vector GetLocalVerticalAxis(double rotacao)
        {
            return RotateVector(new Vector(0, 1), rotacao);
        }

        private static Vector RotateVector(Vector vector, double angle)
        {
            double radians = angle * Math.PI / 180.0;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);
            return new Vector(vector.X * cos - vector.Y * sin, vector.X * sin + vector.Y * cos);
        }

        private static Point RotateAround(Point point, Point center, double angle)
        {
            Vector rotated = RotateVector(point - center, angle);
            return center + rotated;
        }

        private static double Dot(Vector a, Vector b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        private static double Distance(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}