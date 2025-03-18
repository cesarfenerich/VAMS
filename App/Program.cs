using Bogus;
using Domain.Shared;
using Microsoft.Extensions.DependencyInjection;

internal class Program(IVehiclesHandler vehiclesHandler,
                       IVehiclesQueryService vehiclesQueryService,
                       IAuctionsHandler auctionsHandler,
                       IAuctionsQueryService auctionsQueryService)
{
    readonly IVehiclesHandler _vehiclesHandler = vehiclesHandler;
    readonly IVehiclesQueryService _vehiclesQueryService = vehiclesQueryService;

    readonly IAuctionsHandler _auctionsHandler = auctionsHandler;
    readonly IAuctionsQueryService _auctionsQueryService = auctionsQueryService;   

    private static void Main(string[] args)
    {
        var auctionsRepository = AuctionsFactory.CreateAuctionsRepository();
        var auctionsQueryService = AuctionsFactory.CreateAuctionsQueryService(auctionsRepository);     
        var vehiclesRepository = VehiclesFactory.CreateVehiclesRepository();
        var vehiclesCommandService = VehiclesFactory.CreateVehiclesCommandService(vehiclesRepository, auctionsQueryService);
        var vehiclesHandler = VehiclesFactory.CreateVehiclesHandler(vehiclesCommandService);
        var vehiclesQueryService = VehiclesFactory.CreateVehiclesQueryService(vehiclesRepository);
        var auctionsCommandService = AuctionsFactory.CreateAuctionsCommandService(auctionsRepository, vehiclesHandler, vehiclesQueryService);
        var auctionsHandler = AuctionsFactory.CreateAuctionsHandler(auctionsCommandService);

        var serviceProvider = new ServiceCollection()
          .AddSingleton(vehiclesHandler)
          .AddSingleton(vehiclesQueryService)
          .AddSingleton(auctionsHandler)
          .AddSingleton(auctionsQueryService)
          .BuildServiceProvider();
        
        ActivatorUtilities.CreateInstance<Program>(serviceProvider).Run();
    }

    public void Run() 
    {   
        bool running = true;

        while (running)
        {
            Console.WriteLine("--------Welcome to VAMS!-------");
            Console.WriteLine("-------------------------------");
            Console.WriteLine("Select an operation:");
            Console.WriteLine("0 - Run Happy Path");
            Console.WriteLine("1 - Add New Vehicle");
            Console.WriteLine("2 - Retrieve Available Vehicles");
            Console.WriteLine("3 - Search Vehicles by Type");
            Console.WriteLine("4 - Start an Auction");
            Console.WriteLine("5 - Place a Bid");
            Console.WriteLine("6 - End an Auction");
            Console.WriteLine("7 - List Auctions");
            Console.WriteLine("8 - Exit");

            var input = Console.ReadLine();

            if (int.TryParse(input?.ToString(), out int choice))
            {
                try
                {
                    switch (choice)
                    {
                        case 0:
                            HappyPath();
                            break;
                        case 1:
                            AddVehicle();
                            break;
                        case 2:
                            ListVehicles();
                            break;
                        case 3:
                            SearchVehiclesByType();
                            break;
                        case 4:
                            StartAuction();
                            break;
                        case 5:
                            PlaceBid();
                            break;
                        case 6:
                            EndAuction();
                            break;
                        case 7:
                            ListAuctions();
                            break;
                        case 8:
                            running = false;
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
                catch (VehiclesException vhcEx)
                {
                    Console.WriteLine($"Error from Vehicles Domain: {vhcEx.Message}" );
                }
                catch (AuctionsException actEx)
                {
                    Console.WriteLine($"Error from Auctions Domain: {actEx.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unknown error: {ex.Message}\n{ex.StackTrace}");
                }

                if (running)
                {
                    Console.WriteLine();
                    Console.Write("Run another operation? (y/n): ");
                    input = Console.ReadLine();
                    running = input?.Trim().ToLower() == "y";
                }
            }

            Console.Clear();
        }
    }

    #region Operations

    private void HappyPath()
    {
        Console.Clear();
        Console.WriteLine("");
        _vehiclesHandler.Handle(GetFakeVehicle());
        _vehiclesHandler.Handle(GetFakeVehicle());
        _vehiclesHandler.Handle(GetFakeVehicle());
        _vehiclesHandler.Handle(GetFakeVehicle());

        var vehicles = _vehiclesQueryService.GetAvailableVehicles().Vehicles.OrderByDescending(x => x.Id).Take(4).ToList();

        Console.WriteLine($"{vehicles.Count} vehicles were added.");

        _auctionsHandler.Handle(GetFakeStartAuction(vehicles));        
        var auction = _auctionsQueryService.GetAuctions().Auctions.LastOrDefault();
        Console.WriteLine($"Auction ({auction?.Id}) started.");

        _auctionsHandler.Handle(GetFakeBid(auction?.Id, vehicles.First()));       
        Console.WriteLine($"Auction ({auction?.Id}) received a bid for the first vehicle.");

        _auctionsHandler.Handle(GetFakeBid(auction?.Id, vehicles.Last()));
        Console.WriteLine($"Auction ({auction?.Id}) received a bid for the last vehicle.");

        Console.WriteLine("Waiting for the time to end auction (2s)...");
        Thread.Sleep(2000);

        _auctionsHandler.Handle(new EndAuction(auction?.Id ?? 0));
        Console.WriteLine($"Auction ({auction?.Id}) ended.");

        auction = _auctionsQueryService.GetAuctionById(auction?.Id ?? 0);      
        PrintAuction(auction);
    }

    private void AddVehicle()
    {
        Console.WriteLine("");
        _vehiclesHandler.Handle(GetFakeVehicle());

        var vhc = _vehiclesQueryService.GetAvailableVehicles().Vehicles.Last();

        Console.WriteLine($"New Vehicle Successfully Added!");
        PrintVehicle(vhc);
    }

    private void ListVehicles()
    {
        Console.WriteLine("");
        Console.WriteLine("Available Vehicles:");

        _vehiclesQueryService.GetAvailableVehicles().Vehicles.ForEach(vhc => PrintVehicle(vhc));         
    }

    private void SearchVehiclesByType()
    {
        Console.WriteLine("");        

        var search = new Dictionary<VehicleSearchFields, dynamic>()
        {
            { VehicleSearchFields.Type, new Faker().PickRandom<VehicleTypes>() }
        };

        var result = vehiclesQueryService.SearchVehicles(search).Vehicles;

        if (result.Count == 0)
            Console.WriteLine("No vehicles were found.");
        else
        {
            Console.WriteLine("Found Vehicles:");

            result.ForEach(vhc => PrintVehicle(vhc));    
        }
    }

    private void StartAuction()
    {
        Console.WriteLine("");

        var vehicles = _vehiclesQueryService.GetAvailableVehicles().Vehicles;

        _auctionsHandler.Handle(GetFakeStartAuction(vehicles));

        var act = _auctionsQueryService.GetAuctions().Auctions.Last();

        Console.WriteLine($"Auction Started Successfully!");
        PrintAuction(act);
    }

    private void PlaceBid()
    {
        Console.WriteLine("");

        var auction = _auctionsQueryService.GetAuctions().Auctions.LastOrDefault();

        var vehicleId = auction?.Vehicles.Last().Id ?? 0;
        var vehicle = _vehiclesQueryService.GetVehicleById(vehicleId);

        _auctionsHandler.Handle(GetFakeBid(auction?.Id ?? 0, vehicle));

        var act = _auctionsQueryService.GetAuctions().Auctions.Last();

        Console.WriteLine($"Bid Placed Successfully!");
        PrintAuction(act);
    }

    private void EndAuction()
    {
        Console.WriteLine("");

        var auction = _auctionsQueryService.GetAuctions().Auctions.LastOrDefault();

        _auctionsHandler.Handle(new EndAuction(auction?.Id ?? 0));

        var act = _auctionsQueryService.GetAuctions().Auctions.Last();

        Console.WriteLine($"Auction Ended Successfully!");
        PrintAuction(act);
    }

    private void ListAuctions()
    {
        Console.WriteLine("");
        Console.WriteLine("Available Auctions:");

        _auctionsQueryService.GetAuctions().Auctions.ForEach(act => PrintAuction(act));
    }

    #endregion

    #region Fakers

    private static AddVehicle GetFakeVehicle(VehicleTypes? type = null)
    {
        var faker = new Faker();

        return new AddVehicle(type ?? faker.PickRandom<VehicleTypes>(),
                              faker.Vehicle.Manufacturer(),
                              faker.Vehicle.Model(),
                              faker.Random.Number(1990, 2025),
                              faker.Random.Number(1000, 10000),
                              faker.Random.Number(2, 5),
                              faker.Random.Number(1, 7),
                              faker.Random.Double(500, 2000) );
    }

    private StartAuction GetFakeStartAuction(List<VehicleInfo> vehicles)
    {
        var vhcIds = vehicles.Select(x => x.Id).ToList();

        return new StartAuction(vhcIds, DateTime.UtcNow.AddSeconds(1));
    }

    private PlaceBid GetFakeBid(long? auctionId, VehicleInfo vehicle, bool goodBid = true)
    {
        var faker = new Faker();     

        return new PlaceBid(auctionId ?? 0, vehicle.Id, goodBid ? vehicle.StartingBid + 1 : faker.Random.Decimal(1000, 10000));
    }

    #endregion

    #region Printers

    private static void PrintVehicle(VehicleInfo vehicle)
    {
        Console.WriteLine();
        Console.WriteLine($"Vehicle ({vehicle.Id})");
        Console.WriteLine($"Type: {vehicle.Type}");
        Console.WriteLine($"Manufacturer: {vehicle.Manufacturer}");
        Console.WriteLine($"Model: {vehicle.Model}");
        Console.WriteLine($"Year: {vehicle.Year}");
        Console.WriteLine($"StartinBid: {vehicle.StartingBid}");
        Console.WriteLine($"Status: {vehicle.Status}");
        Console.WriteLine($"NumberOfDoors: {vehicle.NumberOfDoors}");
        Console.WriteLine($"NumberOfSeats: {vehicle.NumberOfSeats}");
        Console.WriteLine($"LoadCapacity: {vehicle.LoadCapacity}");
    }

    private static void PrintAuction(AuctionInfo auction)
    {
        Console.WriteLine();
        Console.WriteLine($"Auction ({auction.Id})");
        Console.WriteLine($"StartedAt: {auction.Start}");
        Console.WriteLine($"EndsAt: {auction.End}");
        Console.WriteLine($"Status: {auction.Status}");
        PrintAuctionVehicles(auction.Vehicles);
    }

    private static void PrintAuctionVehicles(List<AuctionVehicleInfo> auctionVehicles)
    {
        Console.WriteLine("Vehicles:");       
        foreach (var vehicle in auctionVehicles)
        {
            Console.WriteLine();
            Console.WriteLine($"Vehicle ({vehicle.Id})");
            Console.WriteLine($"Type: {vehicle.Type}");          
            Console.WriteLine($"StartingBid: {vehicle.StartingBid}");
            Console.WriteLine($"WinnerBid: {vehicle.WinnerBid}");
            Console.WriteLine($"Status: {vehicle.Status}");
            PrintVehicleBids(vehicle.Bids);           
        }       
    }

    private static void PrintVehicleBids(List<BidInfo> bids)
    {        
        var count = 1;
        foreach (var bid in bids)
        {
            Console.WriteLine($"Bid {(count)}: {bid.Amount}");

            count++;
        }
    }

    #endregion
}