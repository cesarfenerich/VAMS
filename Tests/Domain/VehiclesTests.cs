using Bogus;
using Domain.Shared;
using FluentAssertions;
using Moq;

namespace Tests.Domain;

public class VehiclesTests
{
    private readonly Mock<IAuctionsService> _mockAuctionsService;
    private readonly IVehiclesService _vehiclesService;
    private readonly Faker<AddVehicle> _addVehicleFaker;   
    private readonly Faker<AuctionInfo> _auctionFaker;

    public VehiclesTests()
    {
        _mockAuctionsService = new Mock<IAuctionsService>();
        _vehiclesService = VehiclesServiceFactory.CreateVehiclesService(_mockAuctionsService.Object);       

        _addVehicleFaker = new Faker<AddVehicle>()
            .RuleFor(x => x.Type, f => f.PickRandom<VehicleTypes>())
            .RuleFor(x => x.Manufacturer, f => f.Vehicle.Manufacturer())
            .RuleFor(x => x.Model, f => f.Vehicle.Model())
            .RuleFor(x => x.Year, f => f.Random.Number(1990,2025))
            .RuleFor(x => x.StartingBid, f => f.Random.Decimal(10000))
            .RuleFor(x => x.NumberOfDoors, f => f.Random.Number(2,5))
            .RuleFor(x => x.NumberOfSeats, f => f.Random.Number(1,7))
            .RuleFor(x => x.LoadCapacity, f => f.Random.Double(500,1500));

        var _bidFaker = new Faker<BidInfo>()
            .RuleFor(x => x.Amount, f => f.Random.Decimal(1, 100000))
            .RuleFor(x => x.IsWinningBid, f => f.Random.Bool());

        var _auctionVehicleFaker = new Faker<AuctionVehicleInfo>()
            .RuleFor(x => x.Id, f => f.Random.Number(1, 1000))
            .RuleFor(x => x.Type, f => f.PickRandom<VehicleTypes>())
            .RuleFor(x => x.Manufacturer, f => f.Vehicle.Manufacturer())
            .RuleFor(x => x.Model, f => f.Vehicle.Model())
            .RuleFor(x => x.Year, f => f.Random.Number(1990, 2025))
            .RuleFor(x => x.StartingBid, f => f.Random.Decimal(10000))
            .RuleFor(x => x.WinnerBid, f => f.Random.Decimal(10000))
            .RuleFor(x => x.Bids, f => _bidFaker.Generate(5))
            .RuleFor(x => x.Status, f => f.PickRandom<VehicleStatuses>());

        _auctionFaker = new Faker<AuctionInfo>()
            .RuleFor(x => x.Id, f => f.Random.Number(1, 1000))
            .RuleFor(x => x.Vehicles, f => _auctionVehicleFaker.Generate(10))
            .RuleFor(x => x.Start, f => f.Date.Past())
            .RuleFor(x => x.End, f => f.Date.Future())
            .RuleFor(x => x.Status, f => f.PickRandom<AuctionStatuses>());
    }

    private AuctionInfo GenerateOpenAuction(VehicleStatuses statusForVehicles)
    {
        var commands = _addVehicleFaker.Generate(10);
        commands.ForEach(c => _vehiclesService.AddVehicle(c));

        var addedVehicles = _vehiclesService.GetAvailableVehicles();
        var addedVehicleIds = addedVehicles.Vehicles.Select(x => x.Id).ToList();
        var auction = _auctionFaker.Generate();

        foreach (var vehicle in auction.Vehicles)
        {
            vehicle.Id = addedVehicleIds.Last();
            vehicle.Status = statusForVehicles;
            addedVehicleIds.Remove(addedVehicleIds.Last());
        }

        auction.Status = AuctionStatuses.Open;

        return auction;
    }

    [Fact]
    public void Vehicle_ShouldHaveBasicProperties()
    {
        // Arrange       
        var command = _addVehicleFaker.Generate();        

        // Act
        var vhc = _vehiclesService.AddVehicle(command);

        vhc.Should().NotBeNull();
        vhc.Id.Should().Be(vhc.Id);
        vhc.Type.Should().Be(command.Type);
        vhc.Manufacturer.Should().Be(command.Manufacturer);
        vhc.Model.Should().Be(command.Model);
        vhc.Year.Should().Be(command.Year);
        vhc.StartingBid.Should().Be(command.StartingBid);
        vhc.Status.Should().Be(VehicleStatuses.Available);
    }
    [Fact]
    public void Vehicle_ShouldBehaveAsHatchback()
    {
        // Arrange       
        var command = _addVehicleFaker.Generate();

        command.Type = VehicleTypes.Hatchback;
        command.NumberOfDoors = new Faker().Random.Number(2, 5);     

        // Act
        var hatch = _vehiclesService.AddVehicle(command);

        hatch.Should().NotBeNull();
        hatch.Type.Should().Be(VehicleTypes.Hatchback);
        hatch.NumberOfDoors.Should().Be(command.NumberOfDoors);
        hatch.NumberOfSeats.Should().BeNull();
        hatch.LoadCapacity.Should().BeNull();       
    }
    [Fact]
    public void Vehicle_ShouldBehaveAsSedan()
    {
        // Arrange      
        var command = _addVehicleFaker.Generate();       

        command.Type = VehicleTypes.Sedan;
        command.NumberOfDoors = new Faker().Random.Number(2, 5);      

        // Act     
        var sedan = _vehiclesService.AddVehicle(command);

        sedan.Should().NotBeNull();
        sedan.Type.Should().Be(VehicleTypes.Sedan);
        sedan.NumberOfDoors.Should().Be(command.NumberOfDoors);
        sedan.NumberOfSeats.Should().BeNull();
        sedan.LoadCapacity.Should().BeNull();       
    }
    [Fact]
    public void Vehicle_ShouldBehaveAsSUV()
    {
        // Arrange       
        var command = _addVehicleFaker.Generate();   

        command.Type = VehicleTypes.SUV;
        command.NumberOfSeats = new Faker().Random.Number(1, 7);        

        // Act      
        var suv = _vehiclesService.AddVehicle(command);

        suv.Should().NotBeNull();
        suv.Type.Should().Be(VehicleTypes.SUV);
        suv.NumberOfSeats.Should().Be(command.NumberOfSeats);
        suv.NumberOfDoors.Should().BeNull();
        suv.LoadCapacity.Should().BeNull();       
    }
    [Fact]
    public void Vehicle_ShouldBehaveAsTruck()
    {
        // Arrange      
        var command = _addVehicleFaker.Generate();        

        command.Type = VehicleTypes.Truck;
        command.LoadCapacity = new Faker().Random.Double(500,1500);

        // Act 
        var truck = _vehiclesService.AddVehicle(command);

        truck.Should().NotBeNull();
        truck.Type.Should().Be(VehicleTypes.Truck);
        truck.LoadCapacity.Should().Be(command.LoadCapacity);
        truck.NumberOfDoors.Should().BeNull();
        truck.NumberOfSeats.Should().BeNull();
    }
    [Fact]
    public void AddVehicle_ShouldIncrementInventory()
    {
        // Arrange
        var command = _addVehicleFaker.Generate();

        // Act
        var vehicle = _vehiclesService.AddVehicle(command);
        
        vehicle.Should()
               .NotBeNull()
               .And.BeEquivalentTo(_vehiclesService.GetVehicleById(vehicle.Id));     
    }
    [Fact]
    public void AddVehicle_ShouldThrowException_WhenAnyBasicInfoIsBad()
    {
        // Arrange       
        var commands = _addVehicleFaker.Generate(4);

        commands[0].Manufacturer = string.Empty;
        commands[1].Model = string.Empty;
        commands[2].Year = 0;
        commands[3].StartingBid = 0;     

        // Act & Assert
        _vehiclesService.Invoking(x => x.AddVehicle(commands[0]))
                        .Should().Throw<VehiclesException>()
                        .WithMessage("Manufacturer is required.");

        _vehiclesService.Invoking(x => x.AddVehicle(commands[1]))
                        .Should().Throw<VehiclesException>()
                        .WithMessage("Model is required.");

        _vehiclesService.Invoking(x => x.AddVehicle(commands[2]))
                        .Should().Throw<VehiclesException>()
                        .WithMessage("Year is required.");

        _vehiclesService.Invoking(x => x.AddVehicle(commands[3]))
                        .Should().Throw<VehiclesException>()
                        .WithMessage("StartingBid is required.");
    }
    [Fact]
    public void AddVehicle_ShouldThrowException_WhenVehicleTypeIsBad()
    {
        // Arrange
        var command = _addVehicleFaker.Generate();        

        command.Type = (VehicleTypes)99;     

        // Act & Assert
        _vehiclesService.Invoking(x => x.AddVehicle(command))
                        .Should().Throw<VehiclesException>()
                        .WithMessage("Invalid vehicle type.");
    }
    [Fact]
    public void AddVehicle_ShouldThrowException_WhenNumberOfDoorsIsBad()
    {
        // Arrange
        var commands = _addVehicleFaker.Generate(3);

        commands[0].Type = VehicleTypes.Hatchback;
        commands[0].NumberOfDoors = -1;

        commands[1].Type = VehicleTypes.Sedan;
        commands[1].NumberOfDoors = 0;      

        // Act & Assert        
        _vehiclesService.Invoking(x => x.AddVehicle(commands[0]))
                        .Should().Throw<VehiclesException>()
                        .WithMessage($"Door count is ({commands[0].NumberOfDoors}) and should be between 2 and 5.");

        _vehiclesService.Invoking(x => x.AddVehicle(commands[1]))
                        .Should().Throw<VehiclesException>()
                        .WithMessage($"Door count is ({commands[1].NumberOfDoors}) and should be between 2 and 5.");       
    }
    [Fact]
    public void AddVehicle_ShouldThrowException_WhenNumberOfSeatsIsBad()
    {
        // Arrange
        var command = _addVehicleFaker.Generate();

        command.Type = VehicleTypes.SUV;
        command.NumberOfSeats = 0;
        
        // Act & Assert
        _vehiclesService.Invoking(x => x.AddVehicle(command))
                        .Should().Throw<VehiclesException>()
                        .WithMessage($"Seat count is ({command.NumberOfSeats}) and must be at least 1.");       
    }
    [Fact]
    public void AddVehicle_ShouldThrowException_WhenLoadCapacityIsBad()
    {
        // Arrange
        var command = _addVehicleFaker.Generate();

        command.Type = VehicleTypes.Truck;
        command.LoadCapacity = -1;

        // Act & Assert
        _vehiclesService.Invoking(x => x.AddVehicle(command))
                        .Should().Throw<VehiclesException>()
                        .WithMessage($"Capacity cannot be negative ({command.LoadCapacity}).");      
    }   
    [Fact]
    public void SearchVehicles_ShouldReturnOnlyOneMatchingVehicle()
    {
        // Arrange
        var command = _addVehicleFaker.Generate();     

        var vhc = _vehiclesService.AddVehicle(command);

        var search = new Dictionary<VehicleSearchFields, dynamic>()
        {
            { VehicleSearchFields.Type, vhc.Type },
            { VehicleSearchFields.Manufacturer, vhc.Manufacturer },
            { VehicleSearchFields.Model, vhc.Model },
            { VehicleSearchFields.Year, vhc.Year }
        };

        // Act & Assert
        var results = _vehiclesService.SearchVehicles(search);

        results.Should()
               .NotBeNull()
               .And.BeOfType<VehiclesView>();

        results.Vehicles.Should()
                        .OnlyContain(x => x.Id == vhc.Id);
    }
    [Fact]
    public void SearchVehicles_ShouldReturnVehiclesFilteredByType()
    {
        // Arrange
        _addVehicleFaker.Generate(100)
                        .ForEach(c => _vehiclesService.AddVehicle(c));  

        var type = new Faker().PickRandom<VehicleTypes>();

        var search = new Dictionary<VehicleSearchFields, dynamic>()
        {
            { VehicleSearchFields.Type, type },          
        };

        // Act & Assert
        var results = _vehiclesService.SearchVehicles(search);

        results.Should()
               .NotBeNull()
               .And.BeOfType<VehiclesView>();

        results.Vehicles.Should()
                        .NotBeNullOrEmpty()
                        .And.OnlyContain(x => x.Type == type)
                        .And.HaveCountGreaterThan(1);     
    }
    [Fact]
    public void SearchVehicles_ShouldReturnVehiclesFilteredByManufacturerOrModel()
    {
        // Arrange   
        _addVehicleFaker.Generate(100)
                        .ForEach(c => _vehiclesService.AddVehicle(c));

        var vhc = new Faker().Vehicle;
        var manufacturer = vhc.Manufacturer();
        var model = vhc.Model();

        var search = new Dictionary<VehicleSearchFields, dynamic>()
        {
            { VehicleSearchFields.Manufacturer, manufacturer },
            { VehicleSearchFields.Model, model },
        };

        // Act & Assert
        var results = _vehiclesService.SearchVehicles(search);

        results.Should()
               .NotBeNull()
               .And.BeOfType<VehiclesView>();

        results.Vehicles.Should()
                        .NotBeNullOrEmpty()
                        .And.OnlyContain(x => x.Manufacturer == manufacturer || x.Model == model);                      
    }
    [Fact]
    public void SearchVehicles_ShouldReturnVehiclesFilteredByYear()
    {
        // Arrange
        _addVehicleFaker.Generate(1000)
                        .ForEach(c => _vehiclesService.AddVehicle(c));

        var year = new Faker().Random.Number(1990, 2025);

        var search = new Dictionary<VehicleSearchFields, dynamic>()
        {
            { VehicleSearchFields.Year, year },
        };

        // Act & Assert
        var results = _vehiclesService.SearchVehicles(search);

        results.Should()
               .NotBeNull()
               .And.BeOfType<VehiclesView>();

        results.Vehicles.Should()
                        .NotBeNullOrEmpty()
                        .And.OnlyContain(x => x.Year == year);
    }
    [Fact]
    public void SearchVehicles_ShouldNotReturnVehicles()
    {
        // Arrange
        _addVehicleFaker.Generate(100)
                        .ForEach(c => 
                        {
                            c.Type = VehicleTypes.SUV;
                            c.NumberOfSeats = 1;

                            _vehiclesService.AddVehicle(c); 
                        });          

        var search = new Dictionary<VehicleSearchFields, dynamic>()
        {
            { VehicleSearchFields.Type, VehicleTypes.Truck },
        };

        // Act & Assert
        var results = _vehiclesService.SearchVehicles(search);

        results.Should()
               .NotBeNull()
               .And.BeOfType<VehiclesView>();

        results.Vehicles.Should().BeEmpty();
    }
    [Fact]
    public void SearchVehicles_ShouldThrowException_WhenThereIsNoSearchCriteria()
    {
        // Arrange
        var search = new Dictionary<VehicleSearchFields, dynamic>();

        // Act & Assert
        _vehiclesService.Invoking(x => x.SearchVehicles(search))
                        .Should().Throw<VehiclesException>()
                        .WithMessage($"At least one field is required to Search a vehicle.");
    }
    [Fact]
    public void UpdateInventoryByAuction_ShouldUpdateVehicleStatusProperly()
    {
        //Arrange    
        var auction = GenerateOpenAuction(new Faker().PickRandom<VehicleStatuses>());

        _mockAuctionsService.Setup(x => x.GetAuctionById(auction.Id))
                            .Returns(auction);
        //Act
        _vehiclesService.UpdateInventoryByAuction(auction.Id);

        //Assert
        _mockAuctionsService.Verify(x => x.GetAuctionById(auction.Id), Times.Once);       

        foreach (var vhc in auction.Vehicles)
        {
            var vehicle = _vehiclesService.GetVehicleById(vhc.Id);          

            vehicle.Should().NotBeNull();
            vehicle.Status.Should().Be(vhc.Status);
        }
    }   
    [Fact]
    public void UpdateInventoryByAuction_ShouldThrowException_WhenAuctionIsClosed()
    {
        var auction = _auctionFaker.Generate();
        auction.Status = AuctionStatuses.Closed;

        _mockAuctionsService.Setup(x => x.GetAuctionById(auction.Id))
                            .Returns(auction);      

        _vehiclesService.Invoking(x => x.UpdateInventoryByAuction(auction.Id))
                            .Should().Throw<VehiclesException>()
                            .WithMessage($"Cannot update because the auction ({auction.Id}) is closed.");
    }    
    [Fact]
    public void GetAvailableVehicles_ShouldReturnOnlyAvailableVehicles()
    {
        //Arrange
        var commands = _addVehicleFaker.Generate(10);
        commands.ForEach(c => _vehiclesService.AddVehicle(c));

        //Act
        var availableVehicles = _vehiclesService.GetAvailableVehicles();

        //Assert
        availableVehicles.Should().NotBeNull();
        availableVehicles.Vehicles.Should().NotBeNull();
        availableVehicles.Vehicles.Should().HaveCount(10);
        availableVehicles.Vehicles.Should().OnlyContain(x => x.Status == VehicleStatuses.Available);
    }
    [Fact]
    public void GetAvailableVehicles_ShouldReturnEmpty()
    {
        //Arrange
        var auction = GenerateOpenAuction(VehicleStatuses.Sold);

        _mockAuctionsService.Setup(x => x.GetAuctionById(auction.Id))
                            .Returns(auction);
        //Act
        _vehiclesService.UpdateInventoryByAuction(auction.Id);

        var availableVehicles = _vehiclesService.GetAvailableVehicles();

        //Assert
        availableVehicles.Should().NotBeNull();
        availableVehicles.Vehicles.Should().NotBeNull();
        availableVehicles.Vehicles.Should().HaveCount(0);
    }
    [Fact]
    public void GetVehicleById_ShouldReturnVehicleInfo()
    {
        var command = _addVehicleFaker.Generate();
        var addedVehicle = _vehiclesService.AddVehicle(command);
        var returedVehicle = _vehiclesService.GetVehicleById(addedVehicle.Id);

        returedVehicle.Should()
                      .NotBeNull()
                      .And.BeEquivalentTo(addedVehicle);
    }
    [Fact]
    public void GetVehicleById_ShouldThrowException_WhenVehicleIsNotFound()
    {
        _vehiclesService.Invoking(x => x.GetVehicleById(99999))
                        .Should().Throw<VehiclesException>()
                        .WithMessage("Vehicle with id 99999 not found.");
    }    
}