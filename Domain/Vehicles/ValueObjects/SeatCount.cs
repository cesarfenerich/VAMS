using Domain.Shared;

namespace Domain.Vehicles;

internal readonly struct SeatCount
{
    public int Value { get; }

    public SeatCount(VehicleTypes type, int? numberOfSeats)
    {
        if (type != VehicleTypes.SUV)
            throw new VehiclesException($"Seat count is only applicable to {VehicleTypes.SUV} vehicles.");

        if (numberOfSeats < 1)
            throw new VehiclesException($"Seat count is ({numberOfSeats}) and must be at least 1.");

        Value = numberOfSeats.GetValueOrDefault();
    }
}