namespace Domain.Shared;

public interface IVehiclesService
{    
    VehicleInfo AddVehicle(AddVehicle command);
    VehiclesView GetAvailableVehicles();
    VehicleInfo GetVehicleById(long id);
    VehiclesView SearchVehicles(Dictionary<VehicleSearchFields, dynamic> search);
    void UpdateInventoryByAuction(long auctionId);
}