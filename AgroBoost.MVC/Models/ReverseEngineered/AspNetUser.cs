using System;
using System.Collections.Generic;

namespace AgroBoost.MVC.Models.ReverseEngineered;

public partial class AspNetUser
{
    public string Id { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string UserType { get; set; } = null!;

    public string? OrganizationName { get; set; }

    public string? OrganizationType { get; set; }

    public string? Address { get; set; }

    public string? District { get; set; }

    public string? State { get; set; }

    public string? Gstnumber { get; set; }

    public string? RegistrationNumber { get; set; }

    public bool IsVerified { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public string? UserName { get; set; }

    public string? NormalizedUserName { get; set; }

    public string? Email { get; set; }

    public string? NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? PasswordHash { get; set; }

    public string? SecurityStamp { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; } = new List<AspNetUserClaim>();

    public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; } = new List<AspNetUserLogin>();

    public virtual ICollection<AspNetUserToken> AspNetUserTokens { get; set; } = new List<AspNetUserToken>();

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<Order> OrderBuyers { get; set; } = new List<Order>();

    public virtual ICollection<Order> OrderSellers { get; set; } = new List<Order>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<AspNetRole> Roles { get; set; } = new List<AspNetRole>();
}
