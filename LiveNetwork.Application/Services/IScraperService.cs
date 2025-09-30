using System.Threading.Tasks;

namespace LiveNetwork.Infrastructure.Services
{
    public interface IScraperService
    {
        Task<int> ScrapeAsync();
    }


}
