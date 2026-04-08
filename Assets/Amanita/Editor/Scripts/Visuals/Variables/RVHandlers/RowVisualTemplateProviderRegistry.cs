using System;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public static class RowVisualTemplateProviderRegistry
    {
        private static IRowVisualTemplateProvider _current = new CachedRowVisualTemplateProvider();

        public static IRowVisualTemplateProvider Current
        {
            get => _current;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _current = value;
            }
        }
    }
}