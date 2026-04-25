using Microsoft.EntityFrameworkCore;
using PowerFuel.Application.DTOs.Products;
using PowerFuel.Application.Interfaces;
using PowerFuel.Application.Media;
using PowerFuel.Domain.Entities;
using PowerFuel.Infrastructure.Data;

namespace PowerFuel.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IMediaUrlGenerator _mediaUrls;

    public ProductService(ApplicationDbContext context, IMediaUrlGenerator mediaUrls)
    {
        _context = context;
        _mediaUrls = mediaUrls;
    }

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var p = await _context.Products.AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return p == null ? null : MapToDto(p);
    }

    public async Task<IReadOnlyList<ProductListDto>> GetBestSellersAsync(int count = 8, CancellationToken cancellationToken = default)
    {
        var products = await _context.Products.AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.IsBestSeller)
            .OrderByDescending(x => x.ReviewCount)
            .Take(count)
            .ToListAsync(cancellationToken); // ← جيب من DB الأول

        return products.Select(p => new ProductListDto(
            p.Id,
            p.Name,
            p.ShortDescription,
            p.Price,
            p.OriginalPrice,
            p.IsOnSale,
            _mediaUrls.ProductImageUrl(p.ImageUrl), // ← اعمل mapping في الـ memory
            p.AverageRating,
            p.ReviewCount,
            p.Category?.Name
        )).ToList();
    }

    public async Task<IReadOnlyList<ProductListDto>> ListAsync(string? categorySlug = null, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Products.AsNoTracking().Include(x => x.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(categorySlug))
            query = query.Where(p => p.Category != null && p.Category.Slug == categorySlug);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || (p.ShortDescription != null && p.ShortDescription.Contains(search)));

        var products = await query.ToListAsync(cancellationToken); // ← جيب من DB الأول

        return products.Select(p => new ProductListDto(
            p.Id,
            p.Name,
            p.ShortDescription,
            p.Price,
            p.OriginalPrice,
            p.IsOnSale,
            _mediaUrls.ProductImageUrl(p.ImageUrl), // ← اعمل mapping في الـ memory
            p.AverageRating,
            p.ReviewCount,
            p.Category?.Name
        )).ToList();
    }

    private ProductDto MapToDto(Product p) => new(
        p.Id,
        p.Name,
        p.ShortDescription,
        p.LongDescription,
        p.AdditionalInfo,
        p.Price,
        p.OriginalPrice,
        p.IsOnSale,
        _mediaUrls.ProductImageUrl(p.ImageUrl),
        p.StockQuantity,
        p.CategoryId,
        p.Category?.Name,
        p.AverageRating,
        p.ReviewCount,
        p.IsBestSeller
    );
}
