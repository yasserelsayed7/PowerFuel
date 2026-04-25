namespace PowerFuel.Application.Media;

public interface IMediaUrlGenerator
{
    string? ProductImageUrl(string? imageFileName);

    string? EquipmentImageUrl(string? imageFileName);

    string? CoachProfileImageUrl(string? imageFileName);

    string? TestimonialImageUrl(string? imageFileName);
}
