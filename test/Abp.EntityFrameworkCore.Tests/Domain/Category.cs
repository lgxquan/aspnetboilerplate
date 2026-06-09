using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Abp.EntityFrameworkCore.Tests.Domain;

// Soft-deletable parent used to exercise the soft-delete cascade prototype.
public class Category : FullAuditedEntity
{
    public string Name { get; set; }

    public ICollection<Product> Products { get; set; }

    public Category()
    {
        Products = new List<Product>();
    }
}

// Soft-deletable child with a required relationship to Category (=> Cascade delete behavior).
public class Product : FullAuditedEntity
{
    public string Name { get; set; }

    [Required]
    public Category Category { get; set; }

    public int CategoryId { get; set; }

    // Hard-delete child (plain Entity) with a required relationship to Product (=> Cascade).
    public ICollection<ProductTag> Tags { get; set; }

    public Product()
    {
        Tags = new List<ProductTag>();
    }
}

public class ProductTag : Entity
{
    public string Name { get; set; }

    [Required]
    public Product Product { get; set; }

    public int ProductId { get; set; }
}
