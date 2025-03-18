using Domain.Shared;

namespace Domain.Auctions;

internal class AuctionsRepository : IAuctionsRepository
{
    public List<Auction> _inventory = [];

    public void Add(Auction auction)
        => _inventory.Add(auction);

    public void Remove(Auction auction)
        => _inventory.Remove(auction);

    public long GetLastId()
        => _inventory.Count == 0 ? 0 : _inventory.Max(x => x.Id);

    public Auction? GetById(long id)
        => _inventory.FirstOrDefault(x => x.Id == id);

    public List<Auction> GetAll() => _inventory; 
   
}