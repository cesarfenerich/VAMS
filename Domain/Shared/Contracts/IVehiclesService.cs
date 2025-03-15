namespace Domain.Shared;

public interface IVehiclesService
{    
    VehicleInfo AddVehicle(AddVehicle command);
    VehiclesView GetAllVehicles();
    VehicleInfo GetVehicleById(long id);
    VehiclesView SearchVehicles(Dictionary<VehicleSearchFields, dynamic> search);
    VehiclesView UpdateVehiclesStatus(UpdateVehiclesStatus command);
}