using GalleryBackend.Data;
using GalleryBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace GalleryBackend.GraphQL
{
    public class GalleryQuery
    {
        public async Task<List<GalleryItem>> GetGalleryItems([Service] AppDbContext context)
        {
            return await context.GalleryItems
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<GalleryItem?> GetGalleryItemById(
            int id,
            [Service] AppDbContext context)
        {
            return await context.GalleryItems.FindAsync(id);
        }
    }
}