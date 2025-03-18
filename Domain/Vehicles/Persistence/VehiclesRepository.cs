using Domain.Shared;

namespace Domain.Vehicles;

internal class VehiclesRepository : IVehiclesRepository
{
    private readonly List<Vehicle> _inventory = [];

    public void Add(Vehicle vehicle)
        => _inventory.Add(vehicle);

    public void Remove(Vehicle vehicle)
        => _inventory.Remove(vehicle);

    public long GetLastId() 
        => _inventory.Count == 0 ? 0 : _inventory.Max(x => x.Id);

    public List<Vehicle> GetByStatus(VehicleStatuses status) 
        => _inventory.Where(x => x.Status == status).ToList();

    public Vehicle? GetById(long id) 
        => _inventory.FirstOrDefault(x => x.Id == id);

    public List<Vehicle> GetByIds(List<long> ids)
        => _inventory.Where(x => ids.Contains(x.Id)).ToList();

    public List<Vehicle> GetByCriteria(Dictionary<VehicleSearchFields, dynamic> search)
        => _inventory.Where(vehicle =>
        {
            bool match = true; //This boolean, together with the condition below, define the search as an AND.
                               //If is set to false, and the condition is commented, it defines the search as an OR. (tests may fail)

            foreach (var searchField in search)
            {
                var property = typeof(Vehicle).GetProperty(searchField.Key.ToString());

                if (property is not null)
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    var propertyValue = Convert.ChangeType(property?.GetValue(vehicle), property.PropertyType);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    var fieldValue = Convert.ChangeType(searchField.Value, property?.PropertyType);

                    if (!match) //--> Uncomment this line to define the search as an OR
                        break;

                    match = (propertyValue?.Equals(fieldValue));
                }

            }

            return match;

        }).ToList();
}