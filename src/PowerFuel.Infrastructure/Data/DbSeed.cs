using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PowerFuel.Domain.Entities;

namespace PowerFuel.Infrastructure.Data;

public static class DbSeed
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger, string? configuredAssetsRoot = null)
    {
        try
        {
            var assetsRoot = ResolveAssetsRoot(configuredAssetsRoot);

            if (!await context.Categories.AnyAsync())
                await SeedBaseDataAsync(context, logger);

            // Always update image URLs to match the actual assets folder (works for existing DBs).
            await ApplyRealImageMappingsAsync(context, assetsRoot, logger);

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task SeedBaseDataAsync(ApplicationDbContext context, ILogger logger)
    {
        var productCategories = new[]
        {
            new Category { Name = "Supplements", Slug = "supplements", Description = "Premium supplements", Kind = "Product" },
            new Category { Name = "Equipment", Slug = "equipment", Description = "Gym equipment", Kind = "Product" },
            new Category { Name = "Workout Programs", Slug = "workout-programs", Description = "Expert programs", Kind = "Product" }
        };
        var equipmentCategories = new[]
        {
            new Category { Name = "Chest", Slug = "chest", Description = "Chest equipment", Kind = "Equipment" },
            new Category { Name = "Back", Slug = "back", Description = "Back equipment", Kind = "Equipment" },
            new Category { Name = "Shoulder", Slug = "shoulder", Description = "Shoulder equipment", Kind = "Equipment" },
            new Category { Name = "Leg", Slug = "leg", Description = "Leg equipment", Kind = "Equipment" }
        };

        await context.Categories.AddRangeAsync(productCategories);
        await context.Categories.AddRangeAsync(equipmentCategories);
        await context.SaveChangesAsync();

        var categoriesBySlug = await context.Categories.AsNoTracking().ToDictionaryAsync(c => c.Slug);
        int supplementsId = categoriesBySlug["supplements"].Id;
        int chestId = categoriesBySlug["chest"].Id;
        int backId = categoriesBySlug["back"].Id;
        int shoulderId = categoriesBySlug["shoulder"].Id;
        int legId = categoriesBySlug["leg"].Id;

        var coach = new Coach
        {
            FirstName = "Ahmed",
            LastName = "Mohamed",
            Title = "Professional Bodybuilding Coach",
            Specialization = "Bodybuilding",
            AboutDescription = "Ahmed is a professional bodybuilding coach with over 8 years of experience. He has won multiple championships and specializes in helping athletes achieve their peak physical condition through customized training programs and nutrition plans.",
            YearsExperience = 8,
            HappyClientsCount = 150,
            CertificationsCount = 5,
            PhoneNumber = "+20 123 456 7890",
            Email = "ahmed.mohamed@example.com",
            HourlyRate = 50m,
            CreatedAt = DateTime.UtcNow
        };
        context.Coaches.Add(coach);
        await context.SaveChangesAsync();

        var availabilities = new[]
        {
            new CoachAvailability { CoachId = coach.Id, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0) },
            new CoachAvailability { CoachId = coach.Id, DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0) },
            new CoachAvailability { CoachId = coach.Id, DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0) },
            new CoachAvailability { CoachId = coach.Id, DayOfWeek = DayOfWeek.Thursday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0) },
            new CoachAvailability { CoachId = coach.Id, DayOfWeek = DayOfWeek.Friday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(13, 0) }
        };
        await context.CoachAvailabilities.AddRangeAsync(availabilities);

        var products = new[]
        {
            new Product { Name = "Quantum Whey Isolate", ShortDescription = "Ultra-filtered whey protein with 25g protein per serving", Price = 59.99m, OriginalPrice = 74.99m, IsOnSale = true, CategoryId = supplementsId, StockQuantity = 100, AverageRating = 4.9m, ReviewCount = 24, IsBestSeller = true, CreatedAt = DateTime.UtcNow },
            new Product { Name = "NeuroFocus Pre-Workout", ShortDescription = "Mental clarity and explosive energy formula", Price = 39.99m, CategoryId = supplementsId, StockQuantity = 80, AverageRating = 4.8m, ReviewCount = 18, IsBestSeller = true, CreatedAt = DateTime.UtcNow },
            new Product { Name = "RecoverElite BCAA", ShortDescription = "2:1:1 BCAA ratio with electrolytes and vitamins", Price = 32.99m, CategoryId = supplementsId, StockQuantity = 120, AverageRating = 4.7m, ReviewCount = 31, IsBestSeller = true, CreatedAt = DateTime.UtcNow }
        };
        await context.Products.AddRangeAsync(products);

        var equipments = new[]
        {
            new Equipment { Name = "Chest Press Machine", ShortDescription = "Certified trainer with 8 years of experience in bodybuilding and fitness.", Price = 499.99m, OriginalPrice = 599.99m, IsOnSale = true, CategoryId = chestId, StockQuantity = 15, FeaturedCoachId = coach.Id, CreatedAt = DateTime.UtcNow },
            new Equipment { Name = "Dual Lat Pulldown / Low Row Machine", ShortDescription = "Certified fitness and functional training specialist.", Price = 799.99m, CategoryId = backId, StockQuantity = 10, CreatedAt = DateTime.UtcNow },
            new Equipment { Name = "Shoulder Press Machine", ShortDescription = "Certified nutrition specialist with 10 years of experience.", Price = 549.99m, CategoryId = shoulderId, StockQuantity = 12, CreatedAt = DateTime.UtcNow },
            new Equipment { Name = "Titan Adjustable Dumbbells", ShortDescription = "5-52.5 lbs adjustable dumbbells with quick-lock system", Price = 349.99m, OriginalPrice = 399m, IsOnSale = true, CategoryId = legId, StockQuantity = 25, AverageRating = 4.7m, ReviewCount = 15, CreatedAt = DateTime.UtcNow }
        };
        await context.Equipments.AddRangeAsync(equipments);
        await context.SaveChangesAsync();

        logger.LogInformation("Base data seeded.");
    }

    private static async Task ApplyRealImageMappingsAsync(ApplicationDbContext context, string assetsRoot, ILogger logger)
    {
        if (!Directory.Exists(assetsRoot))
        {
            logger.LogWarning("Assets folder was not found at {AssetsRoot}. Image URLs were not updated.", assetsRoot);
            return;
        }

        var allImages = Directory.EnumerateFiles(assetsRoot, "*.*", SearchOption.AllDirectories)
            .Where(IsImageFile)
            .ToList();

        string ToRelativeAssetUrl(string fullPath) => "/assets/" + Path.GetRelativePath(assetsRoot, fullPath).Replace("\\", "/");

        bool changed = false;

        var products = await context.Products.ToListAsync();
        foreach (var p in products)
        {
            var match = MatchBestImage(allImages, p.Name, "product", "products", "supplement", "supplements");
            var rel = match == null ? null : ToRelativeAssetUrl(match);
            if (!string.Equals(p.ImageUrl, rel, StringComparison.Ordinal))
            {
                p.ImageUrl = rel;
                changed = true;
            }
        }

        var equipments = await context.Equipments.ToListAsync();
        foreach (var e in equipments)
        {
            var match = MatchBestImage(allImages, e.Name, "equipment", "equipments", "gym");
            var rel = match == null ? null : ToRelativeAssetUrl(match);
            if (!string.Equals(e.ImageUrl, rel, StringComparison.Ordinal))
            {
                e.ImageUrl = rel;
                changed = true;
            }
        }

        var coaches = await context.Coaches.ToListAsync();
        foreach (var c in coaches)
        {
            var displayName = $"{c.FirstName} {c.LastName}";
            var match = MatchBestImage(allImages, displayName, "coach", "coaches", "trainer");
            var rel = match == null ? null : ToRelativeAssetUrl(match);
            if (!string.Equals(c.ProfilePictureUrl, rel, StringComparison.Ordinal))
            {
                c.ProfilePictureUrl = rel;
                changed = true;
            }
        }

        if (changed)
        {
            await context.SaveChangesAsync();
            logger.LogInformation("Image URL mappings updated using real assets.");
        }
        else
        {
            logger.LogInformation("Image URL mappings are already up to date.");
        }
    }

    private static string ResolveAssetsRoot(string? configuredAssetsRoot)
    {
        if (!string.IsNullOrWhiteSpace(configuredAssetsRoot))
            return configuredAssetsRoot;

        // Default: src/PowerFuel.API/assets
        return Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "PowerFuel.API", "assets"));
    }

    private static bool IsImageFile(string path) =>
        path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
        path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
        path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        path.EndsWith(".webp", StringComparison.OrdinalIgnoreCase);

    private static string? MatchBestImage(IEnumerable<string> allImages, string entityName, params string[] folderHints)
    {
        var normalizedEntity = Normalize(entityName);
        var tokens = entityName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(Normalize)
            .Where(t => t.Length > 1)
            .ToArray();
        var normalizedHints = folderHints.Select(Normalize).Where(h => h.Length > 0).ToArray();

        string? best = null;
        var bestScore = -1;

        foreach (var path in allImages)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            var normalizedFileName = Normalize(fileName);
            var normalizedPath = Normalize(path.Replace("\\", "/"));

            var score = 0;
            if (normalizedFileName == normalizedEntity) score += 100;
            if (normalizedFileName.StartsWith(normalizedEntity) || normalizedEntity.StartsWith(normalizedFileName)) score += 80;
            if (tokens.Length > 0 && tokens.All(t => normalizedFileName.Contains(t))) score += 60;
            if (tokens.Any(t => normalizedFileName.Contains(t))) score += 25;
            if (normalizedHints.Any(h => normalizedPath.Contains(h))) score += 15;

            if (score > bestScore)
            {
                bestScore = score;
                best = path;
            }
        }

        return bestScore >= 40 ? best : null;
    }

    private static string Normalize(string value) =>
        new(value.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
}
