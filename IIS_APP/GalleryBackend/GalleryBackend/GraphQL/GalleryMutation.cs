using GalleryBackend.Data;
using GalleryBackend.Models;

namespace GalleryBackend.GraphQL
{
    public class GalleryMutation
    {
        public async Task<GalleryItem> CreateGalleryItem(
            CreateGalleryItemInput input,
            [Service] AppDbContext context)
        {
            var item = new GalleryItem
            {
                ImgurId = input.ImgurId,
                Title = input.Title,
                Link = input.Link,
                Type = input.Type,
                Views = input.Views,
                Ups = input.Ups,
                Downs = input.Downs,
                Score = input.Score,
                IsAlbum = input.IsAlbum,
                CreatedAt = input.CreatedAt == default
                    ? DateTime.UtcNow
                    : input.CreatedAt
            };

            context.GalleryItems.Add(item);
            await context.SaveChangesAsync();

            return item;
        }

        public async Task<bool> DeleteGalleryItem(
            int id,
            [Service] AppDbContext context)
        {
            var item = await context.GalleryItems.FindAsync(id);

            if (item == null)
            {
                return false;
            }

            context.GalleryItems.Remove(item);
            await context.SaveChangesAsync();

            return true;
        }
    }
}