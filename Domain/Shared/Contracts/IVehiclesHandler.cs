namespace Domain.Shared;

public interface IVehiclesHandler
{    
    void Handle(AddVehicle command);
    void Handle(UpdateVehiclesByAuction command);
}