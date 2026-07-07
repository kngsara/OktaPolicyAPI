using GalleryBackend.Data;
using GalleryBackend.Models;
using GalleryBackend.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GalleryBackend.Services
{
    public interface IGallerySourceService
    {
        Task<List<GalleryItem>> GetGalleryItemsAsync();
    }

    public class GallerySourceService : IGallerySourceService
    {
        private readonly AppDbContext _context;
        private readonly IOktaPolicyExternalService _oktaPolicyService;
        private readonly ApiSettings _settings;

        public GallerySourceService(
            AppDbContext context,
            IOktaPolicyExternalService oktaPolicyService,
            IOptions<ApiSettings> options)
        {
            _context = context;
            _oktaPolicyService = oktaPolicyService;
            _settings = options.Value;
        }

        public async Task<List<GalleryItem>> GetGalleryItemsAsync()
        {
            if (_settings.UseCustomApi)
            {
                return await _context.GalleryItems
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();
            }

            return await _oktaPolicyService.GetPoliciesAsync();
        }
    }
}