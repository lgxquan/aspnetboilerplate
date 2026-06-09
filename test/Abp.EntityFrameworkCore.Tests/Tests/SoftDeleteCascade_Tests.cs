using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Configuration;
using Abp.EntityFrameworkCore.Tests.Domain;
using Shouldly;
using Xunit;

namespace Abp.EntityFrameworkCore.Tests.Tests;

public class SoftDeleteCascade_Tests : EntityFrameworkCoreModuleTestBase
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<ProductTag> _productTagRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IAbpEfCoreConfiguration _efCoreConfiguration;

    public SoftDeleteCascade_Tests()
    {
        _categoryRepository = Resolve<IRepository<Category>>();
        _productRepository = Resolve<IRepository<Product>>();
        _productTagRepository = Resolve<IRepository<ProductTag>>();
        _unitOfWorkManager = Resolve<IUnitOfWorkManager>();
        _efCoreConfiguration = Resolve<IAbpEfCoreConfiguration>();
    }

    private async Task<int> CreateCategoryWithChildrenAsync()
    {
        var categoryId = 0;
        await WithUnitOfWorkAsync(async () =>
        {
            var category = new Category { Name = "cat-1" };
            var product = new Product { Name = "p1" };
            product.Tags.Add(new ProductTag { Name = "tag-1" });
            category.Products.Add(product);
            category.Products.Add(new Product { Name = "p2" });

            categoryId = await _categoryRepository.InsertAndGetIdAsync(category);
        });

        return categoryId;
    }

    [Fact]
    public async Task Should_Cascade_Soft_Delete_To_Children_When_Enabled()
    {
        _efCoreConfiguration.EnableSoftDeleteCascade = true;

        var categoryId = await CreateCategoryWithChildrenAsync();

        await WithUnitOfWorkAsync(async () => { await _categoryRepository.DeleteAsync(categoryId); });

        await WithUnitOfWorkAsync(async () =>
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                // Soft-deletable parent and children are soft-deleted (kept, IsDeleted = true).
                (await _categoryRepository.GetAsync(categoryId)).IsDeleted.ShouldBeTrue();

                var products = await _productRepository.GetAllListAsync();
                products.Count.ShouldBe(2);
                products.ShouldAllBe(p => p.IsDeleted);

                // Non-soft-deletable grandchildren are hard-deleted by the cascade.
                (await _productTagRepository.GetAllListAsync()).ShouldBeEmpty();
            }
        });
    }

    [Fact]
    public async Task Should_Not_Touch_Children_When_Disabled()
    {
        _efCoreConfiguration.EnableSoftDeleteCascade = false;

        var categoryId = await CreateCategoryWithChildrenAsync();

        await WithUnitOfWorkAsync(async () => { await _categoryRepository.DeleteAsync(categoryId); });

        await WithUnitOfWorkAsync(async () =>
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                (await _categoryRepository.GetAsync(categoryId)).IsDeleted.ShouldBeTrue();

                // Historical behavior: children are left untouched (not soft-deleted).
                var products = await _productRepository.GetAllListAsync();
                products.Count.ShouldBe(2);
                products.ShouldAllBe(p => !p.IsDeleted);
            }
        });
    }
}
