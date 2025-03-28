﻿namespace Domain.Shared;

public class UpdateVehiclesByAuction(long auctionId) : Command
{
    public long AuctionId { get; } = auctionId;

    public override bool IsValid()
    {
        //NICE_TO_HAVE: Implement command validations
        return true;
    }
}