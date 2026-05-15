using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Araci.Core.Commands;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Services
{
    public static class ClipboardService
    {
        // =========================
        // BUFFER
        // =========================

        private static readonly List<Elemento> _buffer = new();

        // =========================
        // OFFSET
        // =========================

        private const double OFFSET_X = 30;

        private const double OFFSET_Y = 30;

        // =========================
        // COPIAR
        // =========================

        public static void CopiarSelecionados()
        {
            _buffer.Clear();

            foreach (var item in SelectionService.Selecionados)
            {
                var clone = ClonarModelo(item.Modelo);

                if (clone != null)
                {
                    _buffer.Add(clone);
                }
            }
        }

        // =========================
        // COLAR
        // =========================

        public static void Colar()
        {
            if (_buffer.Count == 0)
                return;

            var novos = new List<ElementoViewModel>();

            using var transaction = AppServices.BeginTransaction();

            foreach (var modelo in _buffer)
            {
                var cloneModelo = ClonarModelo(modelo);

                if (cloneModelo == null)
                    continue;

                cloneModelo.PosicaoX += OFFSET_X;
                cloneModelo.PosicaoY += OFFSET_Y;

                if (cloneModelo is Cabo cabo)
                {
                    cabo.PosicaoX2 += OFFSET_X;
                    cabo.PosicaoY2 += OFFSET_Y;
                }

                var vm = CriarViewModel(cloneModelo);

                if (vm == null)
                    continue;

                novos.Add(vm);

                transaction.Add(
                    new AddElementoCommand(vm));
            }

            transaction.Commit();

            SelectionService.Limpar();

            foreach (var item in novos)
            {
                SelectionService.Selecionar(item, true);
            }

            _buffer.Clear();

            foreach (var item in novos)
            {
                var clone = ClonarModelo(item.Modelo);

                if (clone != null)
                {
                    _buffer.Add(clone);
                }
            }
        }

        // =========================
        // CLONAR MODELO
        // =========================

        private static Elemento? ClonarModelo(Elemento modelo)
        {
            // =========================
            // CABO
            // =========================

            if (modelo is Cabo cabo)
            {
                return new Cabo
                {
                    Id = System.Guid.NewGuid(),

                    Nome = cabo.Nome,

                    PosicaoX = cabo.PosicaoX,
                    PosicaoY = cabo.PosicaoY,

                    PosicaoX2 = cabo.PosicaoX2,
                    PosicaoY2 = cabo.PosicaoY2,

                    BarraOrigem = cabo.BarraOrigem,
                    BarraDestino = cabo.BarraDestino,

                    Comprimento = cabo.Comprimento,

                    TipoCabo = cabo.TipoCabo,

                    Resistencia = cabo.Resistencia,
                    Reatancia = cabo.Reatancia,
                    Capacitancia = cabo.Capacitancia,
                    Ampacidade = cabo.Ampacidade,

                    Fases = cabo.Fases,
                    Neutro = cabo.Neutro,

                    Categoria = cabo.Categoria,
                    Familia = cabo.Familia,

                    Rotacao = cabo.Rotacao,
                    Escala = cabo.Escala
                };
            }

            // =========================
            // CARGA
            // =========================

            if (modelo is Carga carga)
            {
                return new Carga
                {
                    Id = System.Guid.NewGuid(),

                    Nome = carga.Nome,

                    PosicaoX = carga.PosicaoX,
                    PosicaoY = carga.PosicaoY,

                    Barra = carga.Barra,
                    Alimentador = carga.Alimentador,

                    PotenciaAtivaKW = carga.PotenciaAtivaKW,
                    PotenciaReativaKvar = carga.PotenciaReativaKvar,

                    ModeloCarga = carga.ModeloCarga,
                    Conexao = carga.Conexao,

                    TensaoKV = carga.TensaoKV,
                    Fases = carga.Fases,

                    FatorPotencia = carga.FatorPotencia,

                    Categoria = carga.Categoria,
                    Familia = carga.Familia,

                    Rotacao = carga.Rotacao,
                    Escala = carga.Escala
                };
            }

            // =========================
            // GERADOR
            // =========================

            if (modelo is Gerador gerador)
            {
                return new Gerador
                {
                    Id = System.Guid.NewGuid(),

                    Nome = gerador.Nome,

                    PosicaoX = gerador.PosicaoX,
                    PosicaoY = gerador.PosicaoY,

                    Barra = gerador.Barra,
                    Alimentador = gerador.Alimentador,

                    PotenciaAtivaKW = gerador.PotenciaAtivaKW,
                    FatorPotencia = gerador.FatorPotencia,

                    TipoGerador = gerador.TipoGerador,
                    Fabricante = gerador.Fabricante,
                    Modelo = gerador.Modelo,

                    PotenciaNominalKW = gerador.PotenciaNominalKW,

                    TensaoKV = gerador.TensaoKV,
                    Fases = gerador.Fases,

                    Categoria = gerador.Categoria,
                    Familia = gerador.Familia,

                    Rotacao = gerador.Rotacao,
                    Escala = gerador.Escala
                };
            }

            return null;
        }

        // =========================
        // VIEWMODEL
        // =========================

        private static ElementoViewModel? CriarViewModel(Elemento modelo)
        {
            if (modelo is Cabo cabo)
            {
                return new CaboViewModel(cabo);
            }

            if (modelo is Carga carga)
            {
                return new CargaViewModel(carga);
            }

            if (modelo is Gerador gerador)
            {
                return new GeradorViewModel(gerador);
            }

            return null;
        }
    }
}