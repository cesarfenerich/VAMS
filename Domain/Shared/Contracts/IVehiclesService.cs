namespace Domain.Shared;

public interface IVehiclesService
{    
    VehicleInfo AddVehicle(AddVehicle command);
    VehiclesView GetAllVehicles();
    VehicleInfo? GetVehicleById(long id);
    VehiclesView SearchVehicles(Dictionary<VehicleSearchFields, dynamic> search);

    //NICE_TO_HAVE: Implement the following methods
    //Task<Vehicle> UpdateVehicle(Vehicle vehicle);
    //Task DeleteVehicle(int id);
}
