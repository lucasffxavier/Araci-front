using System;
using Araci.Core.Documents;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetTableInstanceViewModel
    {
        public ProjectSheetTableInstanceViewModel(ProjectSheetTableInstance instance, string tableName)
        {
            ArgumentNullException.ThrowIfNull(instance);

            Id = instance.Id;
            TableId = instance.TableId;
            TableName = string.IsNullOrWhiteSpace(tableName) ? "Tabela sem nome" : tableName;
            X = instance.X;
            Y = instance.Y;
            Width = instance.Width;
            Height = instance.Height;
        }

        public Guid Id { get; }
        public Guid TableId { get; }
        public string TableName { get; }
        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }
    }
}
