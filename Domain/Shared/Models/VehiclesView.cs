namespace Domain.Shared;

public class VehiclesView(List<VehicleInfo> vehicles)
{
    public List<VehicleInfo> Vehicles { get; set; } = vehicles;
}
