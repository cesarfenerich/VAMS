using Bogus;
using Domain.Shared;
using FluentAssertions;

namespace Tests.Domain;

public class AuctionsTests
{   
    private readonly IAuctionsService _auctionsService;
    private readonly IVehiclesService _vehiclesService;
    private readonly Faker<StartAuction> _startAuctionFaker;

    public AuctionsTests()
    {                
        _vehiclesService = VehiclesServiceFactory.CreateVehiclesService();
        _auctionsService = AuctionsServiceFactory.CreateAuctionsService(_vehiclesService);

        var _addVehicleFaker = new Faker<AddVehicle>()
            .RuleFor(x => x.Type, f => f.PickRandom<VehicleTypes>())
            .RuleFor(x => x.Manufacturer, f => f.Vehicle.Manufacturer())
            .RuleFor(x => x.Model, f => f.Vehicle.Model())
            .RuleFor(x => x.Year, f => f.Random.Number(1990, 2025))
            .RuleFor(x => x.StartingBid, f => f.Random.Decimal(10000))
            .RuleFor(x => x.NumberOfDoors, f => f.Random.Number(2, 5))
            .RuleFor(x => x.NumberOfSeats, f => f.Random.Number(1, 7))
            .RuleFor(x => x.LoadCapacity, f => f.Random.Double(500, 1500));

        var addedCars = _addVehicleFaker.Generate(10).Select(x => _vehiclesService.AddVehicle(x)).ToList();       

        _startAuctionFaker = new Faker<StartAuction>()
            .RuleFor(x => x.VehicleIds, f => [.. addedCars.Select(x => x.Id)])
            .RuleFor(x => x.EndDate, f => f.Date.Future());
    }

    [Fact]
    public void Aunction_ShouldHaveBasicProperties()
    {
        // Arrange       
        var command = _startAuctionFaker.Generate();

        // Act
        var auction = _auctionsService.StartAuction(command);

        auction.Should().NotBeNull();
        auction.Id.Should().Be(auction.Id);
        auction.Vehicles.Should().HaveCount(command.VehicleIds.Count);
        auction.EndDate.Should().Be(command.EndDate);       
        auction.Status.Should().Be(AuctionStatuses.Open);
    }


    [Fact]
    public void StartAuction_ShouldInitializeVehiclesAuction()
    {
       
    }

    [Fact]
    public void StartAuction_ShouldThrowException_WhenAnyVehicleDoesNotExist()
    {
       
    }

    [Fact]
    public void StartAuction_ShouldThrowException_WhenAnotherAuctionIsActive()
    {
       
    }

    [Fact]
    public void PlaceBid_ShouldPlaceBid_WhenBidIsHigherThanCurrentBid()
    {        
    }

    [Fact]
    public void PlaceBid_ShouldThrowException_WhenBidIsLowerThanCurrentBid()
    {
        
    }
}
