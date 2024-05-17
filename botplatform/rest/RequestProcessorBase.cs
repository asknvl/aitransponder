using System.Net;
using System.Threading.Tasks;

namespace botplatform.rest
{
    public interface IRequestProcessor
    {
        Task<(HttpStatusCode, string)> ProcessRequestData(string data);
    }
}
