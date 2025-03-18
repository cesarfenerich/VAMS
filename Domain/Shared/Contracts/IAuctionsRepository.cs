using Domain.Auctions;

namespace Domain.Shared;

public interface IAuctionsRepository
{
    internal void Add(Auction auction);

    internal void Remove(Auction vehicle);

    internal long GetLastId();

    internal Auction? GetById(long id);

    internal List<Auction> GetAll();
}
