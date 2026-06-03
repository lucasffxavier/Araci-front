namespace Araci.Models
{
    public abstract class ElementoAnotativo : Elemento
    {
        public override ElementoDomainRole DomainRole => ElementoDomainRole.Anotacao;
    }
}
