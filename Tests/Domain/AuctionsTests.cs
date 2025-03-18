using Bogus;
using Domain.Shared;
using FluentAssertions;
using NSubstitute;

namespace Tests.Domain;

public class AuctionsTests
{
    private readonly IAuctionsQueryService _auctionsQueryService;
    private readonly IAuctionsHandler _auctionsHandler;   

    private readonly IVehiclesHandler _vehiclesHandler;
    private readonly IVehiclesQueryService _vehiclesQueryService;
    private readonly Faker<VehicleInfo> _vehiclesFaker;   

    public AuctionsTests()
    {
        _vehiclesHandler = Substitute.For<IVehiclesHandler>();
        _vehiclesQueryService = Substitute.For<IVehiclesQueryService>();

        var _repository = AuctionsFactory.CreateAuctionsRepository();     

        _auctionsHandler = AuctionsFactory.CreateAuctionsHandler(_repository, _vehiclesHandler, _vehiclesQueryService);
        _auctionsQueryService = AuctionsFactory.CreateAuctionsQueryService(_repository);

        _vehiclesFaker = new Faker<VehicleInfo>()
            .RuleFor(x => x.Id, f => f.Random.Number(1, 1000))
            .RuleFor(x => x.Type, f => f.PickRandom<VehicleTypes>())
            .RuleFor(x => x.Manufacturer, f => f.Vehicle.Manufacturer())
            .RuleFor(x => x.Model, f => f.Vehicle.Model())
            .RuleFor(x => x.Year, f => f.Random.Number(1990, 2025))
            .RuleFor(x => x.StartingBid, f => f.Random.Decimal(1000, 10000));          
    }
    
    private StartAuction GenerateStartAuctionCommand(int vehiclesToAdd,
                                                     VehicleStatuses addedVehicleStatuses,
                                                     DateTime endDate)
    {
        var faker = _vehiclesFaker.RuleFor(x => x.Status, f => addedVehicleStatuses);
        var vehicles = faker.Generate(vehiclesToAdd);

        var auctionCommand = new StartAuction(vehicles.Select(x => x.Id).ToList(), endDate);

        _vehiclesQueryService.GetAvailableVehicles().Returns(new VehiclesView(vehicles));

        return auctionCommand;
    }
    [Fact]
    public void StartAuction_ShouldOpenVehiclesAuction()
    {
        // Arrange       
        StartAuction command = GenerateStartAuctionCommand(10,
                                                           VehicleStatuses.Available,
                                                           new Faker().Date.Future());
        // Act
        _auctionsHandler.Handle(command);

        var auction = _auctionsQueryService.GetAuctions().Auctions.First();

        //Assert
        _vehiclesQueryService.Received().GetAvailableVehicles();      

        auction.Should().NotBeNull();
        auction.Id.Should().Be(auction.Id);
        auction.Vehicles.Should().HaveCount(auction.Vehicles.Count);
        auction.End.Should().Be(auction.End);       
        auction.Status.Should().Be(AuctionStatuses.Open);
    }
    [Fact]
    public void StartAuction_ShouldThrowException_WhenEndTimeIsBad()
    {
        // Arrange            
        StartAuction command = GenerateStartAuctionCommand(10,
                                                           VehicleStatuses.Available,
                                                           default);
        // Act        
        _auctionsHandler.Invoking(x => x.Handle(command))
                        .Should().Throw<AuctionsException>()
                        .WithMessage("Invalid auction end date.");
    }
    [Fact]
    public void StartAuction_ShouldThrowException_WhenThereIsNoVehiclesToBeAuctioned()
    {
        // Arrange            
        StartAuction command = GenerateStartAuctionCommand(0,
                                                           VehicleStatuses.Available,
                                                           new Faker().Date.Future());  
        // Act & Assert
        _auctionsHandler.Invoking(x => x.Handle(command))
                        .Should().Throw<AuctionsException>()
                        .WithMessage("There is no vehicle to be auctioned.");       
    }
    [Fact]
    public void StartAuction_ShouldThrowException_WhenOneOrMoreVehiclesCannotBeAuctioned()
    {
        // Arrange       
        var faker = _vehiclesFaker.RuleFor(x => x.Status, f => VehicleStatuses.Available);
        var vehicles = faker.Generate(10);

        var command = new StartAuction(vehicles.Select(x => x.Id).ToList(),
                                      new Faker().Date.Future());
        vehicles.RemoveAt(0);
        vehicles.RemoveAt(1);      

        _vehiclesQueryService.GetAvailableVehicles().Returns(new VehiclesView(vehicles));       

        // Act & Assert
        _auctionsHandler.Invoking(x => x.Handle(command))
                        .Should().Throw<AuctionsException>()
                        .WithMessage("One or more vehicles are not available to be auctioned.");
    }
    [Fact]
    public void PlaceBid_ShouldPlaceABidOnVehicle()
    {        
        // Arrange            
        StartAuction command = GenerateStartAuctionCommand(10,
                                                           VehicleStatuses.Available,
                                                           new Faker().Date.Future());       
        // Act
        _auctionsHandler.Handle(command);       

        var startedAuction = _auctionsQueryService.GetAuctions().Auctions.First();        
        var vehicle = startedAuction.Vehicles.First();

        _auctionsHandler.Handle(new PlaceBid(startedAuction.Id,
                                             vehicle.Id,
                                             vehicle.StartingBid + 1));       

        startedAuction = _auctionsQueryService.GetAuctionById(startedAuction.Id);

        //Assert
        _vehiclesQueryService.Received().GetAvailableVehicles();     

        startedAuction.Vehicles.First().Bids.Should().HaveCount(1);
        startedAuction.Vehicles.First().Bids.First().Amount.Should().Be(vehicle.StartingBid + 1);
    }
    [Fact]
    public void PlaceBid_ShouldThrowException_WhenABidIsNegative()
    {
        // Arrange
        StartAuction command = GenerateStartAuctionCommand(10,
                                                           VehicleStatuses.Available,
                                                           new Faker().Date.Future());
        // Act
        _auctionsHandler.Handle(command);      

        var startedAuction = _auctionsQueryService.GetAuctions().Auctions.First();
        var vehicle = startedAuction.Vehicles.Last();

        var bidCommand = new PlaceBid(startedAuction.Id, vehicle.Id, -1);        
        
        // Assert
        _auctionsHandler.Invoking(x => _auctionsHandler.Handle(bidCommand))
                        .Should().Throw<AuctionsException>()
                        .WithMessage($"Bid amount ({bidCommand.Amount}) cannot be negative.");
    }
    [Fact]
    public void PlaceBid_ShouldThrowException_WhenABidIsLowerThanStartingBid()
    {
        // Arrange
        StartAuction command = GenerateStartAuctionCommand(10,
                                                           VehicleStatuses.Available,
                                                           new Faker().Date.Future());
        // Act
        _auctionsHandler.Handle(command);      

        var startedAuction = _auctionsQueryService.GetAuctions().Auctions.First();
        var vehicle = startedAuction.Vehicles.First();

        var bidCommand = new PlaceBid(startedAuction.Id, 
                                      vehicle.Id, 
                                      vehicle.StartingBid - 1);
        // Assert
        _auctionsHandler.Invoking(x => x.Handle(bidCommand))
                        .Should().Throw<AuctionsException>()
                        .WithMessage($"Bid amount ({bidCommand.Amount}) must be greater than the vehicle starting bid ({vehicle.StartingBid}).");
    }
    [Fact]
    public void PlaceBid_ShouldThrowException_WhenABidIsLowerThanCurrentBid()
    {
        // Arrange
        StartAuction command = GenerateStartAuctionCommand(10,
                                                           VehicleStatuses.Available,
                                                           new Faker().Date.Future());
        // Act
        _auctionsHandler.Handle(command);       

        var startedAuction = _auctionsQueryService.GetAuctions().Auctions.First();
        var vehicle = startedAuction.Vehicles.First();

        var bid1 = new PlaceBid(startedAuction.Id, 
                                vehicle.Id, 
                                999999999);

        _auctionsHandler.Handle(bid1);

        var bid2 = new PlaceBid(startedAuction.Id,
                                vehicle.Id,
                                2);
        // Assert
        _auctionsHandler.Invoking(x => _auctionsHandler.Handle(bid2))
                        .Should().Throw<AuctionsException>()
                        .WithMessage($"Bid amount ({bid2.Amount}) must be greater than the current highest bid ({bid1.Amount}).");
    }
    [Fact]
    public void PlaceBid_ShouldThrowException_WhenAuctionDoesNotExists()
    {
        // Arrange
        var vehicle = _vehiclesFaker.Generate();        

        var bidCommand = new PlaceBid(9999,
                                      vehicle.Id,
                                      999999999);
        // Act & Assert
        _auctionsHandler.Invoking(x => x.Handle(bidCommand))
                        .Should().Throw<AuctionsException>()
                        .WithMessage($"Auction {bidCommand.AuctionId} was not found.");      
    }
    [Fact]
    public void PlaceBid_ShouldThrowException_WhenAuctionIsNotOpenForBids()
    {
        // Arrange
        StartAuction command = GenerateStartAuctionCommand(10,
                                                           VehicleStatuses.Available,
                                                           DateTime.UtcNow.AddSeconds(1));
        // Act
        _auctionsHandler.Handle(command);        

        Thread.Sleep(2000);

        var startedAuction = _auctionsQueryService.GetAuctions().Auctions.First();

        _auctionsHandler.Handle(new EndAuction(startedAuction.Id));  

        var vehicle = startedAuction.Vehicles.First();      

        var bid = new PlaceBid(startedAuction.Id,
                               vehicle.Id,
                               vehicle.StartingBid + 1);  
        //Assert
        _vehiclesQueryService.Received().GetAvailableVehicles();

        _auctionsHandler.Invoking(x => _auctionsHandler.Handle(bid))
                        .Should().Throw<AuctionsException>()
                        .WithMessage("Auction is not open for bids.");
    }
    [Fact]
    public void PlaceBid_ShouldThrowException_WhenVehicleDoesNotExists()
    {
        // Arrange
        StartAuction auctionCommand = GenerateStartAuctionCommand(10,
                                                                  VehicleStatuses.Available,
                                                                  DateTime.UtcNow.AddSeconds(1));
        // Act
        _auctionsHandler.Handle(auctionCommand);

        Thread.Sleep(2000);

        var startedAuction = _auctionsQueryService.GetAuctions().Auctions.First();

        _auctionsHandler.Handle(new EndAuction(startedAuction.Id));       

        var vehicle = startedAuction.Vehicles.First();
       
        var bidCommand = new PlaceBid(startedAuction.Id,
                                      9999,
                                      9999);
        // Act
        _auctionsHandler.Invoking(x => x.Handle(bidCommand))
                        .Should().Throw<AuctionsException>()
                        .WithMessage("Vehicle not found.");
    }    
    [Fact]
    public void EndAuction_ShouldCloseAuction()
    {
        // Arrange       
        StartAuction command = GenerateStartAuctionCommand(10,
                                                           VehicleStatuses.Available,
                                                           DateTime.UtcNow.AddSeconds(1));
        // Act
        _auctionsHandler.Handle(command);

        Thread.Sleep(2000);

        var startedAuction = _auctionsQueryService.GetAuctions().Auctions.First();

        _auctionsHandler.Handle(new EndAuction(startedAuction.Id));        

        startedAuction = _auctionsQueryService.GetAuctionById(startedAuction.Id);

        //Assert       
        startedAuction.Should().NotBeNull();
        startedAuction.Status.Should().Be(AuctionStatuses.Closed);              
    }
    [Fact]
    public void EndAuction_ShouldThrowException_WhenAuctionDoesNotExists()
    {
        //Arrange
        var command = new EndAuction(1);

        //Assert
        _auctionsHandler.Invoking(x => x.Handle(command))
                        .Should().Throw<AuctionsException>()
                        .WithMessage($"Auction {1} was not found.");
    }
    [Fact]
    public void EndAuction_ShouldThrowException_WhenIsNotEndDateYet()
    {
        // Arrange       
        StartAuction command = GenerateStartAuctionCommand(10,
                                                           VehicleStatuses.Available,
                                                           new Faker().Date.Future());
        // Act
        _auctionsHandler.Handle(command);

        var startedAuction = _auctionsQueryService.GetAuctions().Auctions.First();

        var endCommand = new EndAuction(startedAuction.Id);

        //Assert
        _vehiclesQueryService.Received().GetAvailableVehicles();      

        _auctionsHandler.Invoking(x => x.Handle(endCommand))
                        .Should().Throw<AuctionsException>()
                        .WithMessage("Cannot close the auction before the end date.");   
    }
    [Fact]
    public void EndAuction_ShouldThrowException_WhenAuctionIsAlreadyClosed()
    {
        // Arrange       
        StartAuction command = GenerateStartAuctionCommand(10,
                                                           VehicleStatuses.Available,
                                                           DateTime.UtcNow.AddSeconds(1));
        // Act
        _auctionsHandler.Handle(command);
      
        var startedAuction = _auctionsQueryService.GetAuctions().Auctions.First();   

        Thread.Sleep(2000);      

        var endCommand = new EndAuction(startedAuction.Id);

        _auctionsHandler.Handle(endCommand);

        //Assert
        _vehiclesQueryService.Received().GetAvailableVehicles();       

        _auctionsHandler.Invoking(x => x.Handle(endCommand))
                        .Should().Throw<AuctionsException>()
                        .WithMessage("Auction was already closed.");
    }
    [Fact]
    public void GetAuctionById_ShouldReturnAuctionInfo()
    {
        // Arrange
        StartAuction command = GenerateStartAuctionCommand(10,
                                                           VehicleStatuses.Available,
                                                           DateTime.UtcNow.AddSeconds(1));
        // Act
        _auctionsHandler.Handle(command);

        var startedAuction = _auctionsQueryService.GetAuctions().Auctions.First();
        var retrievedAuction = _auctionsQueryService.GetAuctionById(startedAuction.Id);

        //Assert
        _vehiclesQueryService.Received().GetAvailableVehicles();        

        retrievedAuction.Should()
                        .NotBeNull()
                        .And.BeEquivalentTo(startedAuction);
    }
    [Fact]
    public void GetAuctionById_ShouldThrowException_WhenAuctionIsNotFound()
    {
        _auctionsQueryService.Invoking(x => x.GetAuctionById(99999))
                        .Should().Throw<AuctionsException>()
                        .WithMessage("Auction with id 99999 not found.");

    }
}
