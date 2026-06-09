using System;
using Microsoft.EntityFrameworkCore;

namespace Abp.EntityFrameworkCore.Configuration;

public interface IAbpEfCoreConfiguration
{
    public bool UseAbpQueryCompiler { get; set; }

    /// <summary>
    /// When enabled, soft-deleting an entity (<see cref="Abp.Domain.Entities.ISoftDelete"/>) also
    /// cascades the deletion to its dependent (child) entities that are configured with a cascade
    /// delete behavior. Soft-deletable children are soft-deleted, other children are hard-deleted.
    /// Default: false (preserves the historical behavior where children are left untouched).
    /// </summary>
    public bool EnableSoftDeleteCascade { get; set; }

    void AddDbContext<TDbContext>(Action<AbpDbContextConfiguration<TDbContext>> action)
        where TDbContext : DbContext;
}

public class NullAbpEfCoreConfiguration : IAbpEfCoreConfiguration
{
    /// <summary>
    /// Gets single instance of <see cref="NullAbpEfCoreConfiguration"/> class.
    /// </summary>
    public static NullAbpEfCoreConfiguration Instance { get; } = new NullAbpEfCoreConfiguration();

    public bool UseAbpQueryCompiler { get; set; }

    public bool EnableSoftDeleteCascade { get; set; }

    public void AddDbContext<TDbContext>(Action<AbpDbContextConfiguration<TDbContext>> action) where TDbContext : DbContext
    {

    }
}