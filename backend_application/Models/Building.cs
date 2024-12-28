﻿namespace API.Models;

public class Building
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int PostalCode { get; set; }
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public List<User> Users { get; set; } = [];
    public List<Room> Rooms { get; set; } = [];
}