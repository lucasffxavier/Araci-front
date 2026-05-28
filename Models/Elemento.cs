using System;
using System.Collections.Generic;
using Araci.Models.Interfaces;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public abstract class Elemento : IElementoClonavel
    {
        public const string PARAM_NOME = "Nome";

        private readonly Dictionary<string, Parameter> _parametros = new();

        protected Elemento()
        {
            DefinirParametro(new Parameter<string>(PARAM_NOME, string.Empty));
        }

        public double PosicaoX { get; set; }
        public double PosicaoY { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public double Rotacao { get; set; }
        public double Escala { get; set; } = 1;
        public TipoElemento? Tipo { get; set; }
        public virtual ElementoDomainRole DomainRole => ElementoDomainRole.Grafico;
        public bool ParticipaDoGrafoEletrico => DomainRole == ElementoDomainRole.EletricoTopologico;

        public string Nome
        {
            get => Obter<string>(PARAM_NOME);
            set => Definir(PARAM_NOME, value);
        }

        public IReadOnlyDictionary<string, Parameter> Parametros => _parametros;

        protected void DefinirParametro(Parameter parameter)
        {
            _parametros[parameter.Nome] = parameter;
        }

        public bool PossuiParametro(string nome)
        {
            return _parametros.ContainsKey(nome);
        }

        public T Obter<T>(string nome)
        {
            if (!_parametros.TryGetValue(nome, out var parameter))
                throw new InvalidOperationException($"Parâmetro '{nome}' não encontrado.");

            if (parameter is not Parameter<T> typed)
                throw new InvalidCastException($"Parâmetro '{nome}' não é do tipo {typeof(T).Name}.");

            return typed.Valor;
        }

        public void Definir<T>(string nome, T valor)
        {
            if (!_parametros.TryGetValue(nome, out var parameter))
                throw new InvalidOperationException($"Parâmetro '{nome}' não encontrado.");

            if (parameter is not Parameter<T> typed)
                throw new InvalidCastException($"Parâmetro '{nome}' não é do tipo {typeof(T).Name}.");

            typed.Valor = valor!;
        }

        public abstract Elemento Clonar();

        protected void CopiarBasePara(Elemento destino)
        {
            destino.Id = Guid.NewGuid();
            destino.PosicaoX = PosicaoX;
            destino.PosicaoY = PosicaoY;
            destino.Rotacao = Rotacao;
            destino.Escala = Escala;
            destino.Tipo = Tipo;

            foreach (var kv in _parametros)
                destino._parametros[kv.Key] = kv.Value.Clonar();
        }
    }
}
