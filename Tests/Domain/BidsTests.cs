using Bogus;
using Domain.Shared;

namespace Tests.Domain;

public class BiddingTests
{
    private readonly IAuctionsService _auctionsService;
    private readonly IVehiclesService _vehiclesService;

    private readonly IBiddingService _biddingService;

    private readonly Faker<PlaceBid> _placeBidFaker;

    public BiddingTests()
    {
        _vehiclesService = VehiclesServiceFactory.CreateVehiclesService();
        _auctionsService = AuctionsServiceFactory.CreateAuctionsService(_vehiclesService);

        var _addVehicleFaker = new Faker<AddVehicle>()
            .RuleFor(x => x.Type, f => f.PickRandom<VehicleTypes>());       
    }

    [Fact]
    public void PlaceBid_ShouldPlaceBid_WhenBidIsHigherThanCurrentBid()
    {
    }

    [Fact]
    public void PlaceBid_ShouldThrowException_WhenBidIsIncomplete()
    {

    }

    [Fact]
    public void PlaceBid_ShouldThrowException_WhenBidIsLowerThanCurrentBid()
    {

    }
}