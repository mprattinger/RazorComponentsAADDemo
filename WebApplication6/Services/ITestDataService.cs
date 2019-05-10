using System.Threading.Tasks;
using WebApplication6.Models;

namespace WebApplication6.Services
{
    public interface ITestDataService
    {
        Task<TestData> GetTestDataAsync();
    }
}