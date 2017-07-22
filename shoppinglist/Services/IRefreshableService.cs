using System;
using System.Threading.Tasks;

namespace shoppinglist.Services
{
    public interface IRefreshableService
    {
        Task Refresh();
    }
}
