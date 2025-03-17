namespace Domain.Shared;

public interface IVehiclesService
{   
    VehiclesView GetAvailableVehicles();
    VehicleInfo GetVehicleById(long id);
    VehiclesView SearchVehicles(Dictionary<VehicleSearchFields, dynamic> search);    
}