using System;

namespace Araci.Models
{
    public abstract class Parameter
    {
        protected Parameter(string nome, Type tipo)
        {
            Nome = nome;
            Tipo = tipo;
        }

        public string Nome { get; }

        public Type Tipo { get; }

        public abstract object? ValorObjeto { get; set; }

        public abstract Parameter Clonar();
    }

    public sealed class Parameter<T> : Parameter
    {
        private T _valor;

        public Parameter(string nome, T valor)
            : base(nome, typeof(T))
        {
            _valor = valor;
        }

        public T Valor
        {
            get => _valor;
            set => _valor = value;
        }

        public override object? ValorObjeto
        {
            get => _valor;
            set
            {
                if (value is T typed)
                {
                    _valor = typed;
                    return;
                }

                if (value == null)
                {
                    _valor = default!;
                    return;
                }

                _valor = (T)Convert.ChangeType(value, typeof(T));
            }
        }

        public override Parameter Clonar()
        {
            return new Parameter<T>(Nome, _valor);
        }
    }
}