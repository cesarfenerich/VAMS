namespace Domain.Shared;

public interface IVehiclesQueryService
{   
    VehiclesView GetAvailableVehicles();
    VehicleInfo GetVehicleById(long id);
    VehiclesView SearchVehicles(Dictionary<VehicleSearchFields, dynamic> search);    
}