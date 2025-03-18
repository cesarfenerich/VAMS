using Bogus;
using Domain.Shared;
using FluentAssertions;
using NSubstitute;

namespace Tests.Domain;

public class VehiclesTests
{
    private readonly IVehiclesHandler _vehiclesHandler;
    private readonly IVehiclesQueryService _vehiclesQueryService;  
   
    private readonly IAuctionsQueryService _auctionsQueryService;

    private readonly Faker<AuctionInfo> _auctionFaker;

    public VehiclesTests()
    {              
        _auctionsQueryService = Substitute.For<IAuctionsQueryService>();

        var _repository = VehiclesFactory.CreateVehiclesRepository();

        var _vehiclesCommandService = VehiclesFactory.CreateVehiclesCommandService(_repository, _auctionsQueryService);

        _vehiclesHandler = VehiclesFactory.CreateVehiclesHandler(_vehiclesCommandService);
        _vehiclesQueryService = VehiclesFactory.CreateVehiclesQueryService(_repository);

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

    private AddVehicle GenerateAddVehicleCommand(VehicleTypes? type = null, 
                                                 string? manufacturer = null,
                                                 string? model = null,
                                                 int? year = null,
                                                 decimal? startingBid = null,
                                                 int? numberOfDoors = null, 
                                                 int? numberOfSeats = null, 
                                                 double? loadCapacity = null)
    {
        var faker = new Faker();    
        
        return  new(type ?? faker.PickRandom<VehicleTypes>(),
                    manufacturer ?? faker.Vehicle.Manufacturer(),
                    model ?? faker.Vehicle.Model(),
                    year ?? faker.Random.Number(1990, 2025),
                    startingBid ?? faker.Random.Decimal(10000),
                    numberOfDoors,
                    numberOfSeats,
                    loadCapacity);        
    }

    private AuctionInfo GenerateOpenAuction(VehicleStatuses statusForVehicles)
    {
        var faker = new Faker();

        var commands = new List<AddVehicle>();

        for (int i = 1; i <= 10; i++)
            commands.Add(GenerateAddVehicleCommand());

        commands.ForEach(command => _vehiclesHandler.Handle(command));

        var addedVehicles = _vehiclesQueryService.GetAvailableVehicles();
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
        var command = GenerateAddVehicleCommand();

        // Act
        _vehiclesHandler.Handle(command);       

        var vhc = _vehiclesQueryService.GetAvailableVehicles().Vehicles.First();

        // Assert
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
        var command = GenerateAddVehicleCommand(VehicleTypes.Hatchback, 
                                                numberOfDoors: new Faker().Random.Number(2, 5));
        // Act
        _vehiclesHandler.Handle(command);

        var hatch = _vehiclesQueryService.GetAvailableVehicles().Vehicles.First();

        // Assert
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
        var command = GenerateAddVehicleCommand(VehicleTypes.Sedan,
                                                numberOfDoors: new Faker().Random.Number(2, 5));
        // Act
        _vehiclesHandler.Handle(command);

        var sedan = _vehiclesQueryService.GetAvailableVehicles().Vehicles.First();

        // Assert
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
        var command = GenerateAddVehicleCommand(VehicleTypes.SUV,
                                                numberOfSeats: new Faker().Random.Number(1, 7));
        // Act
        _vehiclesHandler.Handle(command);

        var suv = _vehiclesQueryService.GetAvailableVehicles().Vehicles.First();

        //Assert
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
        var command = GenerateAddVehicleCommand(VehicleTypes.Truck,
                                                loadCapacity: new Faker().Random.Double(500, 1500));
        // Act 
        _vehiclesHandler.Handle(command);

        var truck = _vehiclesQueryService.GetAvailableVehicles().Vehicles.First();

        // Assert
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
        var command = GenerateAddVehicleCommand();

        // Act
        _vehiclesHandler.Handle(command);

        var vehicle = _vehiclesQueryService.GetAvailableVehicles().Vehicles.First();

        vehicle.Should()
               .NotBeNull()
               .And.BeEquivalentTo(_vehiclesQueryService.GetVehicleById(vehicle.Id));     
    }
    [Fact]
    public void AddVehicle_ShouldThrowException_WhenAnyBasicInfoIsBad()
    {
        // Arrange             
        var command1 = GenerateAddVehicleCommand(manufacturer: string.Empty);
        var command2 = GenerateAddVehicleCommand(model: string.Empty);
        var command3 = GenerateAddVehicleCommand(year: 0);
        var command4 = GenerateAddVehicleCommand(startingBid: 0);     

        // Act & Assert
        _vehiclesHandler.Invoking(x => x.Handle(command1))
                        .Should().Throw<VehiclesException>()
                        .WithMessage("Manufacturer is required.");

        _vehiclesHandler.Invoking(x => x.Handle(command2))
                        .Should().Throw<VehiclesException>()
                        .WithMessage("Model is required.");

        _vehiclesHandler.Invoking(x => x.Handle(command3))
                        .Should().Throw<VehiclesException>()
                        .WithMessage("Year is required.");

        _vehiclesHandler.Invoking(x => x.Handle(command4))
                        .Should().Throw<VehiclesException>()
                        .WithMessage("StartingBid is required.");       
    }
    [Fact]
    public void AddVehicle_ShouldThrowException_WhenVehicleTypeIsBad()
    {
        // Arrange
        var command = GenerateAddVehicleCommand((VehicleTypes)99);

        // Act
        _vehiclesHandler.Invoking(x => x.Handle(command))      
                        .Should().Throw<VehiclesException>()
                        .WithMessage("Invalid vehicle type.");
    }
    [Fact]
    public void AddVehicle_ShouldThrowException_WhenNumberOfDoorsIsBad()
    {
        // Arrange
        var command1 = GenerateAddVehicleCommand(VehicleTypes.Hatchback,
                                                numberOfDoors: -1);

        var command2 = GenerateAddVehicleCommand(VehicleTypes.Sedan,
                                                numberOfDoors: 0);
        // Act & Assert        
        _vehiclesHandler.Invoking(x => x.Handle(command1))
                        .Should().Throw<VehiclesException>()
                        .WithMessage($"Door count is ({command1.NumberOfDoors}) and should be between 2 and 5.");

        _vehiclesHandler.Invoking(x => x.Handle(command2))
                        .Should().Throw<VehiclesException>()
                        .WithMessage($"Door count is ({command2.NumberOfDoors}) and should be between 2 and 5.");       
    }
    [Fact]
    public void AddVehicle_ShouldThrowException_WhenNumberOfSeatsIsBad()
    {
        // Arrange
        var command = GenerateAddVehicleCommand(VehicleTypes.SUV, numberOfSeats: 0);
        
        // Act & Assert
        _vehiclesHandler.Invoking(x => x.Handle(command))
                        .Should().Throw<VehiclesException>()
                        .WithMessage($"Seat count is ({command.NumberOfSeats}) and must be at least 1.");       
    }
    [Fact]
    public void AddVehicle_ShouldThrowException_WhenLoadCapacityIsBad()
    {
        // Arrange
        var command = GenerateAddVehicleCommand(VehicleTypes.Truck, loadCapacity: -1);       

        // Act & Assert
        _vehiclesHandler.Invoking(x => x.Handle(command))
                        .Should().Throw<VehiclesException>()
                        .WithMessage($"Capacity cannot be negative ({command.LoadCapacity}).");      
    }   
    [Fact]
    public void SearchVehicles_ShouldReturnOnlyOneMatchingVehicle()
    {
        // Arrange
        var command1 = GenerateAddVehicleCommand();   
        var command2 = GenerateAddVehicleCommand();

        _vehiclesHandler.Handle(command1);
        _vehiclesHandler.Handle(command2);
        
        var result = _vehiclesQueryService.GetAvailableVehicles();

        var vhc1 = result.Vehicles.First();
        var vhc2 = result.Vehicles.Last();

        var search = new Dictionary<VehicleSearchFields, dynamic>()
        {
            { VehicleSearchFields.Type, vhc1.Type },
            { VehicleSearchFields.Manufacturer, vhc1.Manufacturer },
            { VehicleSearchFields.Model, vhc1.Model },
            { VehicleSearchFields.Year, vhc1.Year }
        };

        // Act & Assert
        var results = _vehiclesQueryService.SearchVehicles(search);

        results.Should()
               .NotBeNull()
               .And.BeOfType<VehiclesView>();

        results.Vehicles.Should()
                        .OnlyContain(x => x.Id == vhc1.Id);
    }
    [Fact]
    public void SearchVehicles_ShouldReturnVehiclesFilteredByType()
    {
        // Arrange
        for (int i = 1; i <= 100; i++)
            _vehiclesHandler.Handle(GenerateAddVehicleCommand());
       
        var type = new Faker().PickRandom<VehicleTypes>();

        var search = new Dictionary<VehicleSearchFields, dynamic>()
        {
            { VehicleSearchFields.Type, type },          
        };

        // Act
        var results = _vehiclesQueryService.SearchVehicles(search);

        // Assert
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
        var vhc = new Faker().Vehicle;
        var manufacturer = vhc.Manufacturer();
        var model = vhc.Model();

        for (int i = 1; i <= 2; i++)
            _vehiclesHandler.Handle(GenerateAddVehicleCommand(manufacturer: manufacturer, model: model));

        for (int i = 1; i <= 100; i++)
            _vehiclesHandler.Handle(GenerateAddVehicleCommand());

        var search = new Dictionary<VehicleSearchFields, dynamic>()
        {
            { VehicleSearchFields.Manufacturer, manufacturer },
            { VehicleSearchFields.Model, model },
        };

        // Act
        var results = _vehiclesQueryService.SearchVehicles(search);

        // Assert
        results.Should()
               .NotBeNull()
               .And.BeOfType<VehiclesView>();

        results.Vehicles.Should()
                        .NotBeNullOrEmpty()
                        .And.OnlyContain(x => x.Manufacturer == manufacturer && x.Model == model);                      
    }
    [Fact]
    public void SearchVehicles_ShouldReturnVehiclesFilteredByYear()
    {
        // Arrange
        for (int i = 1; i <= 500; i++)
            _vehiclesHandler.Handle(GenerateAddVehicleCommand());

        var year = new Faker().Random.Number(1990, 2025);

        var search = new Dictionary<VehicleSearchFields, dynamic>()
        {
            { VehicleSearchFields.Year, year },
        };

        // Act & Assert
        var results = _vehiclesQueryService.SearchVehicles(search);

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
        var command = GenerateAddVehicleCommand(VehicleTypes.SUV, numberOfSeats: 4);

        _vehiclesHandler.Handle(command);        

        var search = new Dictionary<VehicleSearchFields, dynamic>()
        {
            { VehicleSearchFields.Type, VehicleTypes.Truck },
        };

        // Act & Assert
        var results = _vehiclesQueryService.SearchVehicles(search);

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
        _vehiclesQueryService.Invoking(x => x.SearchVehicles(search))
                        .Should().Throw<VehiclesException>()
                        .WithMessage($"At least one field is required to Search a vehicle.");
    }
    [Fact]
    public void UpdateInventoryByAuction_ShouldUpdateVehicleStatusProperly()
    {
        //Arrange    
        var auction = GenerateOpenAuction(new Faker().PickRandom<VehicleStatuses>());

        _auctionsQueryService.GetAuctionById(auction.Id).Returns(auction);

        var command = new UpdateVehiclesByAuction(auction.Id);

        //Act
        _vehiclesHandler.Handle(command);

        //Assert
        _auctionsQueryService.Received().GetAuctionById(auction.Id);

        foreach (var vhc in auction.Vehicles)
        {
            var vehicle = _vehiclesQueryService.GetVehicleById(vhc.Id);          

            vehicle.Should().NotBeNull();
            vehicle.Status.Should().Be(vhc.Status);
        }
    }       
    [Fact]
    public void GetAvailableVehicles_ShouldReturnOnlyAvailableVehicles()
    {
        //Arrange
        for (int i = 1; i <= 10; i++)
            _vehiclesHandler.Handle(GenerateAddVehicleCommand());

        //Act
        var availableVehicles = _vehiclesQueryService.GetAvailableVehicles();

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

        _auctionsQueryService.GetAuctionById(auction.Id).Returns(auction);

        //Act
        var command = new UpdateVehiclesByAuction(auction.Id);

        _vehiclesHandler.Handle(command); 

        var availableVehicles = _vehiclesQueryService.GetAvailableVehicles();

        //Assert
        _auctionsQueryService.Received().GetAuctionById(auction.Id);
        availableVehicles.Should().NotBeNull();
        availableVehicles.Vehicles.Should().NotBeNull();
        availableVehicles.Vehicles.Should().HaveCount(0);
    }
    [Fact]
    public void GetVehicleById_ShouldReturnVehicleInfo()
    {
        // Arrange & Act
        var command = GenerateAddVehicleCommand();        

        _vehiclesHandler.Handle(GenerateAddVehicleCommand());

        var addedVehicle = _vehiclesQueryService.GetAvailableVehicles().Vehicles.First();
        var returedVehicle = _vehiclesQueryService.GetVehicleById(addedVehicle.Id);

        // Assert
        returedVehicle.Should()
                      .NotBeNull()
                      .And.BeEquivalentTo(addedVehicle);
    }
    [Fact]
    public void GetVehicleById_ShouldThrowException_WhenVehicleIsNotFound()
    {
        _vehiclesQueryService.Invoking(x => x.GetVehicleById(99999))
                        .Should().Throw<VehiclesException>()
                        .WithMessage("Vehicle with id 99999 not found.");
    }    
}