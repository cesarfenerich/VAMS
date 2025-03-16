using Domain.Shared;

namespace Domain.Vehicles;

internal class VehiclesService : IVehiclesService
{
    private List<Vehicle> _inventory = [];

    IAuctionsService _auctionsService;

    public VehiclesService(IAuctionsService auctionsService)
    {
        _auctionsService = auctionsService;
    }

    public VehiclesView GetAvailableVehicles() 
        => new([.. _inventory.Where(x => x.Status == VehicleStatuses.Available).Select(x => x.AsModel())]);    

    public VehicleInfo AddVehicle(AddVehicle command)
    {
        var lastId = _inventory.Count != 0 ? _inventory.Max(vhc => vhc.Id) : 1;

        var vehicle =  new Vehicle(lastId,                             
                                   command.Type,
                                   command.Manufacturer,
                                   command.Model,
                                   command.Year,
                                   command.StartingBid,
                                   command.NumberOfDoors,
                                   command.NumberOfSeats,
                                   command.LoadCapacity);
        _inventory.Add(vehicle);

        return vehicle.AsModel();
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

        return new VehiclesView([..result.Select(y => y.AsModel())]);
    }   

    public void UpdateInventoryByAuction(long auctionId)
    {
        var auction = _auctionsService.GetAuctionById(auctionId);       

        if (auction.Status == AuctionStatuses.Closed)
            throw new VehiclesException($"Cannot update because the auction ({auction.Id}) is closed.");

        var vhcIds = auction.Vehicles.Select(x => x.Id).ToList();
        var vhcs = _inventory.Where(x => vhcIds.Contains(x.Id)).ToList();

        vhcs.ForEach(vhc => 
        {
            var status = auction.Vehicles.First(x => x.Id == vhc.Id).Status;

            _inventory.Remove(vhc);

            if (status == VehicleStatuses.InAuction)
                vhc.Reserve();
            else if (status == VehicleStatuses.Sold)
                vhc.Sell();
            else
                vhc.Release();           

            _inventory.Add(vhc);                   
        });       
    }
}