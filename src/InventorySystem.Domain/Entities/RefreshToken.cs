using InventorySystem.Domain.Common;
namespace InventorySystem.Domain.Entities;
public class RefreshToken : BaseEntity { public string UserId { get; set; } = string.Empty; public string Token { get; set; } = string.Empty; public DateTime Expires { get; set; } public bool Revoked { get; set; } public AppUser User { get; set; } = null!; }
