using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;

namespace shoppinglist.Services
{
    public interface IDataRefresher : IDisposable
    {
        ReactiveCommand<Unit, long> RefreshAll { get; }
    }
}
