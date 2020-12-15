using System.Threading.Tasks;

namespace DiabNet.Domain.Services
{
    public interface ISgvSyncService
    {
        Task Synchronize(DateRange range);
    }
}
