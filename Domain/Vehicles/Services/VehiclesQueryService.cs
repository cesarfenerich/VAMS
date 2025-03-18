using Domain.Shared;

namespace Domain.Vehicles;

internal class VehiclesQueryService(IVehiclesRepository vehiclesRepository) : IVehiclesQueryService
{
    readonly IVehiclesRepository _vehicles = vehiclesRepository;  

    public VehiclesView GetAvailableVehicles()
        => new (_vehicles.GetByStatus(VehicleStatuses.Available).Select(x => x.AsModel()).ToList());

    public VehicleInfo GetVehicleById(long id) 
        => _vehicles.GetById(id)?.AsModel() ?? throw new VehiclesException($"Vehicle with id {id} not found.");    

    public VehiclesView SearchVehicles(Dictionary<VehicleSearchFields, dynamic> search)
    {
        if (search.Count == 0)
            throw new VehiclesException("At least one field is required to Search a vehicle.");
        try
        {           
            return new VehiclesView(_vehicles.GetByCriteria(search).Select(y => y.AsModel()).ToList());
        }       
        catch (Exception ex)
        {
            throw new VehiclesException($"Search by criteria failed.");
        }        
    }   
}