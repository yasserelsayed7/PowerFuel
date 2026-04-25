using Microsoft.EntityFrameworkCore;
using PowerFuel.Application.DTOs.Equipment;
using PowerFuel.Application.Interfaces;
using PowerFuel.Application.Media;
using PowerFuel.Infrastructure.Data;

namespace PowerFuel.Infrastructure.Services;

public class EquipmentService : IEquipmentService
{
    private readonly ApplicationDbContext _context;
    private readonly IMediaUrlGenerator _mediaUrls;

    public EquipmentService(ApplicationDbContext context, IMediaUrlGenerator mediaUrls)
    {
        _context = context;
        _mediaUrls = mediaUrls;
    }

    public async Task<EquipmentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var e = await _context.Equipments.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.FeaturedCoach)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (e == null) return null;
        var coachName = e.FeaturedCoach != null ? $"{e.FeaturedCoach.FirstName} {e.FeaturedCoach.LastName}" : null;
        return new EquipmentDto(
            e.Id,
            e.Name,
            e.ShortDescription,
            e.LongDescription,
            e.AdditionalInfo,
            e.Price,
            e.OriginalPrice,
            e.IsOnSale,
            _mediaUrls.EquipmentImageUrl(e.ImageUrl),
            e.StockQuantity,
            e.CategoryId,
            e.Category?.Name,
            e.AverageRating,
            e.ReviewCount,
            e.FeaturedCoachId,
            coachName
        );
    }

    public async Task<IReadOnlyList<EquipmentListDto>> ListAsync(string? categorySlug = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Equipments.AsNoTracking().Include(x => x.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(categorySlug))
            query = query.Where(e => e.Category != null && e.Category.Slug == categorySlug);

        var equipments = await query.ToListAsync(cancellationToken); // ← جيب من DB الأول

        return equipments.Select(e => new EquipmentListDto(
            e.Id,
            e.Name,
            e.ShortDescription,
            e.Price,
            e.OriginalPrice,
            e.IsOnSale,
            _mediaUrls.EquipmentImageUrl(e.ImageUrl), // ← اعمل mapping في الـ memory
            e.Category?.Name,
            e.FeaturedCoachId
        )).ToList();
    }

}
