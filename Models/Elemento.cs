using System;
using System.Collections.Generic;

using Araci.Models.Interfaces;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public abstract class Elemento
        : IElementoClonavel
    {
        // =========================
        // PARÂMETROS BASE
        // =========================

        public const string PARAM_NOME =
            "Nome";

        // =========================
        // CAMPOS
        // =========================

        private readonly Dictionary<string, Parameter>
            _parametros = new();

        // =========================
        // CONSTRUTOR
        // =========================

        protected Elemento()
        {
            DefinirParametro(
                new Parameter<string>(
                    PARAM_NOME,
                    string.Empty));
        }

        // =========================
        // POSIÇÃO
        // =========================

        public double PosicaoX { get; set; }

        public double PosicaoY { get; set; }

        // =========================
        // IDENTIFICAÇÃO
        // =========================

        public Guid Id { get; set; }
            = Guid.NewGuid();

        // =========================
        // TRANSFORMAÇÃO
        // =========================

        public double Rotacao { get; set; }

        public double Escala { get; set; }
            = 1;

        // =========================
        // TIPO
        // =========================

        public TipoElemento? Tipo { get; set; }

        // =========================
        // WRAPPERS BIM BASE
        // =========================

        public string Nome
        {
            get => Obter<string>(PARAM_NOME);

            set => Definir(
                PARAM_NOME,
                value);
        }

        // =========================
        // PARÂMETROS
        // =========================

        public IReadOnlyDictionary<string, Parameter>
            Parametros =>
            _parametros;

        protected void DefinirParametro(
            Parameter parameter)
        {
            _parametros[parameter.Nome] =
                parameter;
        }

        public bool PossuiParametro(
            string nome)
        {
            return _parametros.ContainsKey(nome);
        }

        public T Obter<T>(
            string nome)
        {
            if (!_parametros.TryGetValue(
                    nome,
                    out var parameter))
            {
                throw new InvalidOperationException(
                    $"Parâmetro '{nome}' não encontrado.");
            }

            return ((Parameter<T>)parameter)
                .Valor;
        }

        public void Definir<T>(
            string nome,
            T valor)
        {
            if (!_parametros.TryGetValue(
                    nome,
                    out var parameter))
            {
                throw new InvalidOperationException(
                    $"Parâmetro '{nome}' não encontrado.");
            }

            ((Parameter<T>)parameter).Valor =
                valor!;
        }

        // =========================
        // CLONAGEM
        // =========================

        public abstract Elemento Clonar();

        protected void CopiarBasePara(
            Elemento destino)
        {
            destino.Id =
                Guid.NewGuid();

            destino.PosicaoX =
                PosicaoX;

            destino.PosicaoY =
                PosicaoY;

            destino.Rotacao =
                Rotacao;

            destino.Escala =
                Escala;

            destino.Tipo =
                Tipo;

            foreach (var kv in _parametros)
            {
                destino._parametros[kv.Key] =  kv.Value.Clonar();
            }
        }
    }
}