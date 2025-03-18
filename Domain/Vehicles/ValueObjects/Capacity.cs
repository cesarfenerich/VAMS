using Domain.Shared;

namespace Domain.Vehicles;

internal struct Capacity
{
    public double Value { get; }

    public Capacity(VehicleTypes type, double? capacity)
    {
        if (type != VehicleTypes.Truck)
            throw new VehiclesException($"Capacity is only applicable to {VehicleTypes.Truck} vehicles.");

        if (capacity < 0)
            throw new VehiclesException($"Capacity cannot be negative ({capacity}).");

        Value = capacity.GetValueOrDefault();
    }
}