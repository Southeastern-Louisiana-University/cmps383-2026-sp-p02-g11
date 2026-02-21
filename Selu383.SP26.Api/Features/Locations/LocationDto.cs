namespace Selu383.SP26.Api.Features.Locations;

public class LocationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int TableCount { get; set; }

    // New for Phase 2
    public int? ManagerId { get; set; } //
}