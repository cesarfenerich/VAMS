﻿namespace Domain.Shared;

public class StartAuction
{
    public List<long> VehicleIds { get; set; } = [];

    public DateTime EndDate { get; set; }
}
