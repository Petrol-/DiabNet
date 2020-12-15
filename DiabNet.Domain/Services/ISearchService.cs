using System.Threading.Tasks;

namespace DiabNet.Domain.Services
{
    public interface ISearchService
    {
        Task InsertSgvPoint(Sgv sgv);
    }
}
