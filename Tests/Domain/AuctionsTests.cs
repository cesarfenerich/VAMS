using Bogus;
using Domain.Shared;
using FluentAssertions;
using Moq; //TODO: Replace to use Nsusbsitute instead

namespace Tests.Domain;

public class AuctionsTests
{
    private readonly Mock<IVehiclesService> _mockVehiclesService;
    private readonly IAuctionsService _auctionsService;
    private readonly Faker<VehicleInfo> _vehiclesFaker;   

    public AuctionsTests()
    {
        _mockVehiclesService = new Mock<IVehiclesService>();
        _auctionsService = AuctionsServiceFactory.CreateAuctionsService(_mockVehiclesService.Object);

        _vehiclesFaker = new Faker<VehicleInfo>()
            .RuleFor(x => x.Id, f => f.Random.Number(1, 1000))
            .RuleFor(x => x.Type, f => f.PickRandom<VehicleTypes>())
            .RuleFor(x => x.Manufacturer, f => f.Vehicle.Manufacturer())
            .RuleFor(x => x.Model, f => f.Vehicle.Model())
            .RuleFor(x => x.Year, f => f.Random.Number(1990, 2025))
            .RuleFor(x => x.StartingBid, f => f.Random.Decimal(1000, 10000));          
    }

    private List<VehicleInfo> GenerateVehiclesByStatus(int count, VehicleStatuses status)
    {
        var faker = _vehiclesFaker.RuleFor(x => x.Status, f => status);
        return faker.Generate(count);
    }  
    [Fact]
    public void StartAuction_ShouldOpenVehiclesAuction()
    {
        // Arrange       
        var vehicles = GenerateVehiclesByStatus(10, VehicleStatuses.Available);

        var command = new StartAuction {
            VehicleIds = [.. vehicles.Select(x => x.Id)],
            EndDate = new Faker().Date.Future()
        };    

        _mockVehiclesService.Setup(x => x.GetAvailableVehicles())
                            .Returns(new VehiclesView(vehicles));  
        // Act
        var auction = _auctionsService.StartAuction(command);

        _mockVehiclesService.Verify(x => x.UpdateInventoryByAuction(auction.Id), Times.Once);

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
        var vehicles = GenerateVehiclesByStatus(10, VehicleStatuses.Available);

        var command = new StartAuction
        {
            VehicleIds = [.. vehicles.Select(x => x.Id)]
        };

        _mockVehiclesService.Setup(x => x.GetAvailableVehicles())
                            .Returns(new VehiclesView(vehicles));
     
        // Act
        _auctionsService.Invoking(x => x.StartAuction(command))
                        .Should().Throw<AuctionsException>()
                        .WithMessage("Invalid auction end date.");
    }
    [Fact]
    public void StartAuction_ShouldThrowException_WhenThereIsNoVehiclesToBeAuctioned()
    {
        // Arrange            
        var command = new StartAuction
        {
            VehicleIds = [],
            EndDate = new Faker().Date.Future()
        };

        _mockVehiclesService.Setup(x => x.GetAvailableVehicles())
                            .Returns(new VehiclesView([]));    
        // Act
        _auctionsService.Invoking(x => x.StartAuction(command))
                        .Should().Throw<AuctionsException>()
                        .WithMessage("There is no vehicle to be auctioned.");       
    }
    [Fact]
    public void StartAuction_ShouldThrowException_WhenOneOrMoreVehiclesCannotBeAuctioned()
    {
        // Arrange       
        var vehicles = GenerateVehiclesByStatus(10, VehicleStatuses.Available);

        var command = new StartAuction
        {
            VehicleIds = [.. vehicles.Select(x => x.Id)],
            EndDate = new Faker().Date.Future()
        };

        vehicles.RemoveAt(0);
        vehicles.RemoveAt(1);      

        _mockVehiclesService.Setup(x => x.GetAvailableVehicles())
                            .Returns(new VehiclesView(vehicles));
        // Act
        _auctionsService.Invoking(x => x.StartAuction(command))
                        .Should().Throw<AuctionsException>()
                        .WithMessage("One or more vehicles are not available to be auctioned.");
    }
    [Fact]
    public void PlaceBid_ShouldPlaceABidOnVehicle()
    {
        // Arrange
        var vehicles = GenerateVehiclesByStatus(10, VehicleStatuses.Available);

        var command = new StartAuction
        {
            VehicleIds = [.. vehicles.Select(x => x.Id)],
            EndDate = new Faker().Date.Future()
        };

        _mockVehiclesService.Setup(x => x.GetAvailableVehicles())
                           .Returns(new VehiclesView(vehicles));
        // Act
        var startedAuction = _auctionsService.StartAuction(command);
        var vehicle = startedAuction.Vehicles.First();

        startedAuction = _auctionsService.PlaceBid(new PlaceBid
        {
            AuctionId = startedAuction.Id,
            VehicleId = vehicle.Id,
            Amount = vehicle.StartingBid + 1
        });

        //Assert
        startedAuction.Vehicles.First().Bids.Should().HaveCount(1);
        startedAuction.Vehicles.First().Bids.First().Amount.Should().Be(vehicle.StartingBid + 1);
    }
    [Fact]
    public void PlaceBid_ShouldThrowException_WhenABidIsNegative()
    {
        // Arrange
        var vehicles = GenerateVehiclesByStatus(10, VehicleStatuses.Available);

        var command = new StartAuction
        {
            VehicleIds = [.. vehicles.Select(x => x.Id)],
            EndDate = new Faker().Date.Future()
        };

        _mockVehiclesService.Setup(x => x.GetAvailableVehicles())
                            .Returns(new VehiclesView(vehicles));
        // Act
        var startedAuction = _auctionsService.StartAuction(command);
        var vehicle = startedAuction.Vehicles.First();

        var bidCommand = new PlaceBid
        {
            AuctionId = startedAuction.Id,
            VehicleId = vehicle.Id,
            Amount = -1
        };
        
        // Assert
        _auctionsService.Invoking(x => x.PlaceBid(bidCommand))
                        .Should().Throw<AuctionsException>()
                        .WithMessage($"Bid amount ({bidCommand.Amount}) cannot be negative.");
    }
    [Fact]
    public void PlaceBid_ShouldThrowException_WhenABidIsLowerThanStartingBid()
    {
        // Arrange
        var vehicles = GenerateVehiclesByStatus(10, VehicleStatuses.Available);

        var command = new StartAuction
        {
            VehicleIds = [.. vehicles.Select(x => x.Id)],
            EndDate = new Faker().Date.Future()
        };

        _mockVehiclesService.Setup(x => x.GetAvailableVehicles())
                            .Returns(new VehiclesView(vehicles));
        // Act
        var startedAuction = _auctionsService.StartAuction(command);
        var vehicle = startedAuction.Vehicles.First();

        var bidCommand = new PlaceBid
        {
            AuctionId = startedAuction.Id,
            VehicleId = vehicle.Id,
            Amount = vehicle.StartingBid -1
        };

        // Assert
        _auctionsService.Invoking(x => x.PlaceBid(bidCommand))
                        .Should().Throw<AuctionsException>()
                        .WithMessage($"Bid amount ({bidCommand.Amount}) must be greater than the vehicle starting bid ({vehicle.StartingBid}).");
    }
    [Fact]
    public void PlaceBid_ShouldThrowException_WhenABidIsLowerThanCurrentBid()
    {
        // Arrange
        var vehicles = GenerateVehiclesByStatus(10, VehicleStatuses.Available);

        var command = new StartAuction
        {
            VehicleIds = [.. vehicles.Select(x => x.Id)],
            EndDate = new Faker().Date.Future()
        };

        _mockVehiclesService.Setup(x => x.GetAvailableVehicles())
                            .Returns(new VehiclesView(vehicles));
        // Act
        var startedAuction = _auctionsService.StartAuction(command);
        var vehicle = startedAuction.Vehicles.First();

        var bid1 = new PlaceBid
        {
            AuctionId = startedAuction.Id,
            VehicleId = vehicle.Id,
            Amount = 999999999
        };

        startedAuction = _auctionsService.PlaceBid(bid1);

        var bid2 = new PlaceBid
        {
            AuctionId = startedAuction.Id,
            VehicleId = vehicle.Id,
            Amount = 2
        };

        // Assert
        _auctionsService.Invoking(x => x.PlaceBid(bid2))
                        .Should().Throw<AuctionsException>()
                        .WithMessage($"Bid amount ({bid2.Amount}) must be greater than the current highest bid ({bid1.Amount}).");
    }
    [Fact]
    public void PlaceBid_ShouldThrowException_WhenAuctionDoesNotExists()
    {
        // Arrange
        var vehicle = _vehiclesFaker.Generate();

        var command = new PlaceBid
        {
            AuctionId = 9999,
            VehicleId = vehicle.Id,
            Amount = vehicle.StartingBid + 1
        };

        // Act & Assert
        _auctionsService.Invoking(x => x.PlaceBid(command))
                        .Should().Throw<AuctionsException>()
                        .WithMessage($"Auction {command.AuctionId} was not found.");
    }
    [Fact]
    public void PlaceBid_ShouldThrowException_WhenAuctionIsNotOpenForBids()
    {
        // Arrange
        var vehicles = GenerateVehiclesByStatus(10, VehicleStatuses.Available);

        var auctionCommand = new StartAuction
        {
            VehicleIds = [.. vehicles.Select(x => x.Id)],
            EndDate = DateTime.UtcNow.AddSeconds(1)
        };

        _mockVehiclesService.Setup(x => x.GetAvailableVehicles())
                           .Returns(new VehiclesView(vehicles));
        // Act
        var startedAuction = _auctionsService.StartAuction(auctionCommand);

        Thread.Sleep(2000);

        _auctionsService.EndAuction(startedAuction.Id);

        var vehicle = startedAuction.Vehicles.First();

        var command = new PlaceBid
        {
            AuctionId = startedAuction.Id,
            VehicleId = vehicle.Id,
            Amount = vehicle.StartingBid + 1
        };

        // Act
        _auctionsService.Invoking(x => x.PlaceBid(command))
                        .Should().Throw<AuctionsException>()
                        .WithMessage("Auction is not open for bids.");
    }
    [Fact]
    public void PlaceBid_ShouldThrowException_WhenVehicleDoesNotExists()
    {
        // Arrange
        var vehicles = GenerateVehiclesByStatus(10, VehicleStatuses.Available);

        var auctionCommand = new StartAuction
        {
            VehicleIds = [.. vehicles.Select(x => x.Id)],
            EndDate = DateTime.UtcNow.AddSeconds(1)
        };

        _mockVehiclesService.Setup(x => x.GetAvailableVehicles())
                            .Returns(new VehiclesView(vehicles));
        // Act
        var startedAuction = _auctionsService.StartAuction(auctionCommand);

        Thread.Sleep(2000);

        _auctionsService.EndAuction(startedAuction.Id);

        var vehicle = startedAuction.Vehicles.First();

        var command = new PlaceBid
        {
            AuctionId = startedAuction.Id,
            VehicleId = 9999,
            Amount = 9999
        };

        // Act
        _auctionsService.Invoking(x => x.PlaceBid(command))
                        .Should().Throw<AuctionsException>()
                        .WithMessage("Vehicle not found.");
    }
    [Fact]
    public void EndAuction_ShouldCloseAuction()
    {
        // Arrange       
        var vehicles = GenerateVehiclesByStatus(10, VehicleStatuses.Available);

        var command = new StartAuction
        {
            VehicleIds = [.. vehicles.Select(x => x.Id)],
            EndDate = DateTime.UtcNow.AddSeconds(1)
        };

        _mockVehiclesService.Setup(x => x.GetAvailableVehicles())
                            .Returns(new VehiclesView(vehicles));
        // Act
        var auction = _auctionsService.StartAuction(command);

        Thread.Sleep(2000);

        auction = _auctionsService.EndAuction(auction.Id);

        _mockVehiclesService.Verify(x => x.UpdateInventoryByAuction(auction.Id), Times.Exactly(2));        

        auction.Should().NotBeNull();
        auction.Status.Should().Be(AuctionStatuses.Closed);              
    }
    [Fact]
    public void EndAuction_ShouldThrowException_WhenAuctionDoesNotExists()
    {            
        //Assert
        _auctionsService.Invoking(x => x.EndAuction(1))
                        .Should().Throw<AuctionsException>()
                        .WithMessage($"Auction {1} was not found.");
    }
    [Fact]
    public void EndAuction_ShouldThrowException_WhenIsNotEndDateYet()
    {
        // Arrange       
        var vehicles = GenerateVehiclesByStatus(10, VehicleStatuses.Available);

        var command = new StartAuction
        {
            VehicleIds = [.. vehicles.Select(x => x.Id)],
            EndDate = new Faker().Date.Future()
        };

        _mockVehiclesService.Setup(x => x.GetAvailableVehicles())
                            .Returns(new VehiclesView(vehicles));
        // Act
        var auction = _auctionsService.StartAuction(command);      

        //Assert
        _auctionsService.Invoking(x => x.EndAuction(auction.Id))
                        .Should().Throw<AuctionsException>()
                        .WithMessage("Cannot close the auction before the end date.");   
    }
    [Fact]
    public void EndAuction_ShouldThrowException_WhenAuctionIsAlreadyClosed()
    {
        // Arrange       
        var vehicles = GenerateVehiclesByStatus(10, VehicleStatuses.Available);

        var command = new StartAuction
        {
            VehicleIds = [.. vehicles.Select(x => x.Id)],
            EndDate = DateTime.UtcNow.AddSeconds(1)
        };

        _mockVehiclesService.Setup(x => x.GetAvailableVehicles())
                            .Returns(new VehiclesView(vehicles));
        // Act
        var auction = _auctionsService.StartAuction(command);

        Thread.Sleep(2000);      

        auction = _auctionsService.EndAuction(auction.Id);

        //Assert
        _auctionsService.Invoking(x => x.EndAuction(auction.Id))
                        .Should().Throw<AuctionsException>()
                        .WithMessage("Auction was already closed.");
    }
    [Fact]
    public void GetAuctionById_ShouldReturnAuctionInfo()
    {
        var vehicles = GenerateVehiclesByStatus(10, VehicleStatuses.Available);

        var command = new StartAuction
        {
            VehicleIds = [.. vehicles.Select(x => x.Id)],
            EndDate = DateTime.UtcNow.AddSeconds(1)
        };

        _mockVehiclesService.Setup(x => x.GetAvailableVehicles())
                            .Returns(new VehiclesView(vehicles));
        // Act
        var startedAuction = _auctionsService.StartAuction(command);
        var retrievedAuction = _auctionsService.GetAuctionById(startedAuction.Id);

        retrievedAuction.Should()
                        .NotBeNull()
                        .And.BeEquivalentTo(startedAuction);
    }
    [Fact]
    public void GetAuctionById_ShouldThrowException_WhenAuctionIsNotFound()
    {
        _auctionsService.Invoking(x => x.GetAuctionById(99999))
                       .Should().Throw<AuctionsException>()
                       .WithMessage("Auction with id 99999 not found.");

    }
}
