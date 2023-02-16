using CocRfidReader.Model;

namespace CocRfidReader.Services
{
    public interface ICocReader
    {
        Task<Coc> GetAsync(string epc, CancellationToken cancellationToken);
    }
}