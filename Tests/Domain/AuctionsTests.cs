using Bogus;
using Bogus.DataSets;
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
    public void StartAuction_ShouldInitializeVehiclesAuction()
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
    public void StartAuction_ShouldThrowException_WhenAnyVehicleDoesNotExist()
    {
        // Arrange       
        var command = _startAuctionFaker.Generate();

        command.VehicleIds = [9999];

        // Act
        _auctionsService.Invoking(x => x.StartAuction(command))
                        .Should().Throw<VehiclesException>()
                        .WithMessage($"Vehicle with id {9999} not found.");       
    }

    [Fact]
    public void StartAuction_ShouldThrowException_WhenVehicleIsNotAvailable()
    {
        // Arrange       
        var command = _startAuctionFaker.Generate();

        var id = 9999;

        // Act
        _auctionsService.Invoking(x => x.StartAuction(command))
                        .Should().Throw<AuctionsException>()
                        .WithMessage($"Vehicle with id ({id}) is not available for auction.");
    }      

    [Fact]
    public void EndAuction_ShouldCloseAuction()
    {
        // Arrange       
        var command = _startAuctionFaker.Generate();

        var auction = _auctionsService.StartAuction(command);

        auction = _auctionsService.EndAuction(new EndAuction { AuctionId = auction.Id });

        auction.Should().NotBeNull();
        auction.Status.Should().Be(AuctionStatuses.Closed);          
    }

    [Fact]
    public void EndAuction_ShouldThrowException_WhenAuctionDoesNotExists()
    {
        // Arrange       
        var command = _startAuctionFaker.Generate();

        //Act
        var auction = _auctionsService.StartAuction(command);

        //Assert
        _auctionsService.Invoking(x => x.EndAuction(new EndAuction { AuctionId = auction.Id }))
                        .Should().Throw<AuctionsException>()
                        .WithMessage($"Auction {auction.Id} was not found.");
    }

    [Fact]
    public void EndAuction_ShouldThrowException_WhenItsNotEndTimeYet()
    {
        // Arrange       
        var command = _startAuctionFaker.Generate();

        //Act
        var auction = _auctionsService.StartAuction(command);

        //Assert
        _auctionsService.Invoking(x => x.EndAuction(new EndAuction { AuctionId = auction.Id }))
                        .Should().Throw<AuctionsException>()
                        .WithMessage("Cannot close the auction before the end date.");   
    }

    [Fact]
    public void EndAuction_ShouldThrowException_WhenAuctionAlreadyEnded()
    {
        // Arrange       
        var command = _startAuctionFaker.Generate();

        //Act
        var auction = _auctionsService.StartAuction(command);

        //Assert
        _auctionsService.Invoking(x => x.EndAuction(new EndAuction { AuctionId = auction.Id }))
                        .Should().Throw<AuctionsException>()
                        .WithMessage("Auction was already closed.");
    }
}
