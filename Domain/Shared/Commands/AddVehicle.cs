namespace Domain.Shared;

public class AddVehicle(VehicleTypes type,
                        string manufacturer,
                        string model,
                        int year,
                        decimal startingBid,
                        int? numberOfDoors = null,
                        int? numberOfSeats = null,
                        double? loadCapacity = null) : Command
{      
    public VehicleTypes Type { get; } = type;
    public string Manufacturer { get; } = manufacturer;
    public string Model { get; } = model;
    public int Year { get; } = year;
    public decimal StartingBid { get; } = startingBid;
    public int? NumberOfDoors { get; } = numberOfDoors;
    public int? NumberOfSeats { get; } = numberOfSeats;
    public double? LoadCapacity { get; } = loadCapacity;

    public override bool IsValid()
    {
        //NICE_TO_HAVE: Implement command validations
        return true;
    }
}
