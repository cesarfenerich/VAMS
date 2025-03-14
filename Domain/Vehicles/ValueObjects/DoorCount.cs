using Domain.Shared;

namespace Domain.Vehicles;

internal readonly struct DoorCount
{
    public int Value { get; }

    public DoorCount(VehicleTypes type, int? numberOfDoors)
    {        
        if(type != VehicleTypes.Hatchback && type != VehicleTypes.Sedan)
            throw new VehiclesException($"Door count is only applicable to {VehicleTypes.Hatchback} and {VehicleTypes.Sedan} vehicles.");

        if (numberOfDoors < 2 || numberOfDoors > 5)
            throw new VehiclesException($"Door count is ({numberOfDoors}) and should be between 2 and 5.");      

        Value = numberOfDoors.GetValueOrDefault();
    }
}