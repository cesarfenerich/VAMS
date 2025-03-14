using Bogus;

namespace Tests.Domain;

public class AuctionsTests
{
    //private readonly IAuctionsService _auctionsService;
    //private readonly Faker<Auction> _auctionsFaker;

    //public AuctionsTests()
    //{
    //    _auctionsService = new AuctionsService();
    //    _auctionsFaker = new Faker<Auction>();
    //}

    //[Fact]
    //public void OpenAuction_ShouldOpenAuction_ForCar()
    //{
    //    // Arrange
    //    var car = new Car { Id = 1, Type = "Sedan", Manufacturer = "Toyota", Model = "Camry", Year = 2020 };
    //    _system.AddCar(car);

    //    // Act
    //    _system.OpenAuction(car.Id);

    //    // Assert
    //    Assert.True(_system.IsAuctionOpen(car.Id));
    //}

    //[Fact]
    //public void OpenAuction_ShouldThrowException_WhenCarDoesNotExist()
    //{
    //    // Act & Assert
    //    var exception = Assert.Throws<KeyNotFoundException>(() => _system.OpenAuction(99));
    //    Assert.Equal("Car not found.", exception.Message);
    //}

    //[Fact]
    //public void OpenAuction_ShouldThrowException_WhenAnotherAuctionIsActive()
    //{
    //    // Arrange
    //    var car = new Car { Id = 1, Type = "Sedan", Manufacturer = "Toyota", Model = "Camry", Year = 2020 };
    //    _system.AddCar(car);
    //    _system.OpenAuction(car.Id);

    //    // Act & Assert
    //    var exception = Assert.Throws<InvalidOperationException>(() => _system.OpenAuction(car.Id));
    //    Assert.Equal("Car has an active auction.", exception.Message);
    //}

    //[Fact]
    //public void PlaceBid_ShouldPlaceBid_WhenBidIsHigherThanCurrentBid()
    //{
    //    // Arrange
    //    var car = new Car { Id = 1, Type = "Sedan", Manufacturer = "Toyota", Model = "Camry", Year = 2020 };
    //    _system.AddCar(car);
    //    _system.OpenAuction(car.Id);
    //    _system.PlaceBid(car.Id, 1000);

    //    // Act
    //    _system.PlaceBid(car.Id, 1500);

    //    // Assert
    //    Assert.Equal(1500, _system.GetCurrentBid(car.Id));
    //}

    //[Fact]
    //public void PlaceBid_ShouldThrowException_WhenBidIsLowerThanCurrentBid()
    //{
    //    // Arrange
    //    var car = new Car { Id = 1, Type = "Sedan", Manufacturer = "Toyota", Model = "Camry", Year = 2020 };
    //    _system.AddCar(car);
    //    _system.OpenAuction(car.Id);
    //    _system.PlaceBid(car.Id, 1000);

    //    // Act & Assert
    //    var exception = Assert.Throws<InvalidOperationException>(() => _system.PlaceBid(car.Id, 500));
    //    Assert.Equal("Bid amount must be higher than the current highest bid.", exception.Message);
    //}
}
