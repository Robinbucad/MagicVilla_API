using MagicVilla_VillaAPI.Models;

namespace MagicVilla_VillaAPI.Repository.IRepository
{
    public interface IVillaNumberRepository : IRepository<VillaNumber>
    {
        public Task<VillaNumber> UpdateAsync(VillaNumber villaNumber);
    }
}
