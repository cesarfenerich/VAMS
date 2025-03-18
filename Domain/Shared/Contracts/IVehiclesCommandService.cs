namespace Domain.Shared;

public interface IVehiclesCommandService
{       
    void AddVehicle(AddVehicle command);
    void UpdateInventoryByAuction(UpdateVehiclesByAuction command);
}
