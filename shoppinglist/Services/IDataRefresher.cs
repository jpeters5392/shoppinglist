using System;
using System.Threading.Tasks;

namespace shoppinglist.Services
{
    public interface IDataRefresher
    {
        Task RefreshAll();
    }
}
