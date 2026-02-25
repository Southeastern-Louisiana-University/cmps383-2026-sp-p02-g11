using Selu383.SP26.Api.Features.Users; // Add this using

namespace Selu383.SP26.Api.Features.Locations;

public class Location
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Required, Max 120
    public string Address { get; set; } = string.Empty; // Required
    public int TableCount { get; set; } // Required, Min 1

    // New for Phase 2
    public int? ManagerId { get; set; } // Nullable
    public virtual User? Manager { get; set; } // Optional
}