using Domain.Shared;

namespace Domain.Vehicles;

internal record Vehicle
{
    public long Id { get; }
    public string Manufacturer { get; }
    public string Model { get; private set; }
    public int Year { get; private set; }
    public VehicleStatuses Status { get; private set; }
    public VehicleTypes Type { get; private set; }
    public DoorCount? NumberOfDoors { get; private set; } = null;
    public SeatCount? NumberOfSeats { get; private set; } = null;
    public Capacity? LoadCapacity { get; private set; } = null;

    public Vehicle(long lastId, 
                   VehicleTypes type, 
                   string manufacturer, 
                   string model, 
                   int year, 
                   int? numberOfDoors = null, 
                   int? numberOfSeats = null, 
                   double? loadCapacity = null)
    {
        //This ensure that the unique identifier is not already in use by another vehicle in the inventory
        //No need to raise an exception
        Id = ++lastId;        
        Manufacturer = manufacturer;
        Model = model;
        Year = year;
        Status = VehicleStatuses.Available;

        SetTypeAndAttribute(type, numberOfDoors, numberOfSeats, loadCapacity);
    }

    private void SetTypeAndAttribute(VehicleTypes type, 
                                    int? numberOfDoors = null, 
                                    int? numberOfSeats = null, 
                                    double? loadCapacity = null)
    {
        if (!Enum.IsDefined(type))
            throw new VehiclesException("Invalid vehicle type.");
        else
            Type = type;

        switch (Type)
        {
            case VehicleTypes.Hatchback:
                NumberOfDoors = new(Type, numberOfDoors);
                break;
            case VehicleTypes.Sedan:
                NumberOfDoors = new(Type, numberOfDoors);
                break;
            case VehicleTypes.SUV:
                NumberOfSeats = new(Type, numberOfSeats);
                break;
            case VehicleTypes.Truck:
                LoadCapacity = new(Type, loadCapacity);
                break;         
        }
    }

    public VehicleInfo AsModel()
    {
        return new VehicleInfo()
        {
            Id = Id,
            Manufacturer = Manufacturer,
            Model = Model,
            Year = Year,
            Status = Status,
            Type = Type,
            NumberOfDoors = NumberOfDoors.HasValue ? NumberOfDoors.Value.Value : null,
            NumberOfSeats = NumberOfSeats.HasValue ? NumberOfSeats.Value.Value : null, 
            LoadCapacity = LoadCapacity.HasValue ? LoadCapacity.Value.Value : null,
        };
    }   
}