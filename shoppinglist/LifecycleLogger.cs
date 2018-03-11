using System;
using System.Diagnostics;

namespace shoppinglist
{
    public class LifecycleLogger : IDisposable
    {
        private string _name;

        public LifecycleLogger(Type type)
        {
            _name = type.FullName;
            Debug.WriteLine($"Activating {_name}");
        }

        public void Dispose()
        {
            Debug.WriteLine($"Deactivating {_name}");
        }
    }
}
