using Bogus;
using Domain.Shared;
using FluentAssertions;

namespace Tests.Domain;

public class VehiclesTests
{
    private readonly IVehiclesService _vehiclesService;
    private readonly Faker<AddVehicle> _addVehicleFaker;

    public VehiclesTests()
    {        
        _vehiclesService = VehiclesServiceFactory.CreateVehiclesService();

        _addVehicleFaker = new Faker<AddVehicle>()
            .RuleFor(x => x.Type, f => f.PickRandom<VehicleTypes>())
            .RuleFor(x => x.Manufacturer, f => f.Vehicle.Manufacturer())
            .RuleFor(x => x.Model, f => f.Vehicle.Model())
            .RuleFor(x => x.Year, f => f.Random.Number(1990,2025))
            .RuleFor(x => x.StartingBid, f => f.Random.Decimal(10000))
            .RuleFor(x => x.NumberOfDoors, f => f.Random.Number(2,5))
            .RuleFor(x => x.NumberOfSeats, f => f.Random.Number(1,7))
            .RuleFor(x => x.LoadCapacity, f => f.Random.Double(500,1500));
    }

    //TODO: Add Tests for base vehicle properties

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
        _addVehicleFaker.Generate(100)
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
}