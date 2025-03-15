using Domain.Shared;

namespace Domain.Vehicles;

internal class VehiclesService : IVehiclesService
{
    private List<Vehicle> _inventory = [];

    public VehiclesView GetAllVehicles() => new(_inventory.Select(x => x.AsModel()));    

    public VehicleInfo AddVehicle(AddVehicle command)
    {        
        return new Vehicle(_inventory,                             
                           command.Type,
                           command.Manufacturer,
                           command.Model,
                           command.Year,
                           command.StartingBid,
                           command.NumberOfDoors,
                           command.NumberOfSeats,
                           command.LoadCapacity).AsModel();
    }

    public VehicleInfo GetVehicleById(long id)
    {
        var vhc = _inventory.FirstOrDefault(vehicle => vehicle.Id == id)?.AsModel();

        return vhc ?? throw new VehiclesException($"Vehicle with id {id} not found.");
    }

    public VehiclesView SearchVehicles(Dictionary<VehicleSearchFields, dynamic> search)
    {    
        if(search.Count == 0)
            throw new VehiclesException("At least one field is required to Search a vehicle.");                  

        var result = _inventory.Where(vehicle =>
        {
            bool match = false;

            foreach (var searchField in search)
            {
                var property = typeof(Vehicle).GetProperty(searchField.Key.ToString());

                if(property is not null)
                {
                    var propertyValue = Convert.ChangeType(property?.GetValue(vehicle), property?.PropertyType);
                    var fieldValue = Convert.ChangeType(searchField.Value, property?.PropertyType);

                    if (!match)
                        match = (propertyValue?.Equals(fieldValue));
                }
                
            }

            return match;

        }).ToList();      

        return new VehiclesView(result.Select(y => y.AsModel()));
    }

    public VehiclesView UpdateVehiclesStatus(UpdateVehiclesStatus command)
    {
        if (command.VehicleIds is null || command.VehicleIds.Count == 0)
            throw new VehiclesException("No vehicles were identified.");

        if(!Enum.IsDefined(command.Status))
            throw new VehiclesException("Invalid vehicle status.");

        var vehicles = _inventory.Where(x => command.VehicleIds.Contains(x.Id)).ToList();

        foreach (var vhc in vehicles)
        {
            vhc.UpdateStatus(ref _inventory, command.Status);
        }

        return new VehiclesView(vehicles.Select(y => y.AsModel()));
    }
}