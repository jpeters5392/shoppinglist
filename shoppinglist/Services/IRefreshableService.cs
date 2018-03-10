using System;
using System.Reactive;
using ReactiveUI;

namespace shoppinglist.Services
{
    public interface IRefreshableService
    {
        ReactiveCommand<Unit, long> Refresh { get; }
    }
}
