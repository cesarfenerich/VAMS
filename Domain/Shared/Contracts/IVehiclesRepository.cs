using Domain.Vehicles;

namespace Domain.Shared;

public interface IVehiclesRepository
{
    internal void Add(Vehicle vehicle);

    internal void Remove(Vehicle vehicle);

    internal long GetLastId();

    internal List<Vehicle> GetByStatus(VehicleStatuses status);

    internal Vehicle? GetById(long id);

    internal List<Vehicle> GetByIds(List<long> ids);

    internal List<Vehicle> GetByCriteria(Dictionary<VehicleSearchFields, dynamic> search);
}
