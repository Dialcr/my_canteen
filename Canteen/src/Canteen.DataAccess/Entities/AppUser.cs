using System;
using Microsoft.AspNetCore.Identity;

namespace Canteen.DataAccess.Entities;

public class AppUser : IdentityUser<int>
{
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public int? EstablishmentId { get; set; }
    public Establishment? Establishment { get; set; }
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<CanteenRequest> CanteenRequests { get; set; } = [];



    // public File? Image { get; set; }
    // public UserPreference? UserPreference { get; set; }
    // public virtual ICollection<ConfirmationCode> ConfirmationCodes { get; set; }
    // public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    // public IEnumerable<VerificationToken>? VerificationTokens { get; set; }

    // public AppUser()
    // {
    //     StatusBaseEntity = StatusEntityType.Active;
    //     ConfirmationCodes = new HashSet<ConfirmationCode>();
    //     RefreshTokens = new HashSet<RefreshToken>();
    // }
}

