using System;
using System.Collections.Generic;

namespace Araci.Models.Tipos
{
    public abstract class TipoElemento
    {
        public const string PARAM_NOME_TIPO = "NomeTipo";
        public const string PARAM_FAMILIA = "Familia";
        public const string PARAM_CATEGORIA = "Categoria";

        private readonly Dictionary<string, Parameter> _parametros = new();

        protected TipoElemento()
        {
            DefinirParametro(new Parameter<string>(PARAM_NOME_TIPO, string.Empty));
            DefinirParametro(new Parameter<string>(PARAM_FAMILIA, string.Empty));
            DefinirParametro(new Parameter<string>(PARAM_CATEGORIA, string.Empty));
        }

        public string NomeTipo
        {
            get => Obter<string>(PARAM_NOME_TIPO);
            set => Definir(PARAM_NOME_TIPO, value);
        }

        public string Familia
        {
            get => Obter<string>(PARAM_FAMILIA);
            set => Definir(PARAM_FAMILIA, value);
        }

        public string Categoria
        {
            get => Obter<string>(PARAM_CATEGORIA);
            set => Definir(PARAM_CATEGORIA, value);
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
                throw new InvalidOperationException($"Parâmetro de tipo '{nome}' não encontrado.");

            return ((Parameter<T>)parameter).Valor;
        }

        public void Definir<T>(string nome, T valor)
        {
            if (!_parametros.TryGetValue(nome, out var parameter))
                throw new InvalidOperationException($"Parâmetro de tipo '{nome}' não encontrado.");

            ((Parameter<T>)parameter).Valor = valor!;
        }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(NomeTipo) ? GetType().Name : NomeTipo;
        }
    }
}