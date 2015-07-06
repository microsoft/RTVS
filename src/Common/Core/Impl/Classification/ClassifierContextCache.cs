using System;
using System.Globalization;

namespace Microsoft.Languages.Core.Classification
{
    public abstract class ClassifierContextCache<TContextType, TContextClass> where TContextClass : ClassifierContext<TContextType>
    {
        static IClassifierContext[] _cachedContexts;

        protected ClassifierContextCache()
        {
        }

        private static void Create()
        {
            if (_cachedContexts == null)
            {
                Array values = Enum.GetValues(typeof(TContextType));
                int count = values.Length;
                _cachedContexts = new IClassifierContext[count];
            }
        }

        public static IClassifierContext FromTypeEnum(TContextType contextType)
        {
            Create();

            int index = Convert.ToInt32(contextType, CultureInfo.InvariantCulture);

            if (_cachedContexts[index] == null)
            {
                TContextClass c = Activator.CreateInstance<TContextClass>();
                c.ContextType = contextType;

                _cachedContexts[index] = c;
            }

            return _cachedContexts[index];
        }
    }

}
