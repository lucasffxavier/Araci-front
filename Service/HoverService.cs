using System;
using System.Windows;
using Araci.Core.SceneQueries;
using Araci.ViewModels;

namespace Araci.Services
{
    public class HoverService
    {
        private readonly ISceneQueryService _queries;
        private ElementoViewModel? _atual;

        public HoverService(ISceneQueryService queries)
        {
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        }

        public void Update(Point worldPosition)
        {
            ElementoViewModel? novo = _queries.HitTest(worldPosition)?.Elemento;

            if (ReferenceEquals(_atual, novo))
                return;

            Clear();

            if (novo == null)
                return;

            novo.IsHover = true;
            _atual = novo;
        }

        public void Clear()
        {
            if (_atual == null)
                return;

            _atual.IsHover = false;
            _atual = null;
        }
    }
}
