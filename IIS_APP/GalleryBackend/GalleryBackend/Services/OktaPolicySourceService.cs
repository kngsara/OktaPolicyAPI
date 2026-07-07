using OktaBackend.Data;
using OktaBackend.Models;
using OktaBackend.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace OktaBackend.Services
{
    public interface IOktaPolicySourceService
    {
        Task<List<OktaPolicy>> GetPoliciesAsync();
    }

    public class OktaPolicySourceService : IOktaPolicySourceService
    {
        private readonly AppDbContext _context;
        private readonly IOktaPolicyExternalService _externalService;
        private readonly ApiSettings _settings;

        public OktaPolicySourceService(
            AppDbContext context,
            IOktaPolicyExternalService externalService,
            IOptions<ApiSettings> options)
        {
            _context = context;
            _externalService = externalService;
            _settings = options.Value;
        }

        public async Task<List<OktaPolicy>> GetPoliciesAsync()
        {
            if (_settings.UseCustomApi)
            {
                return await _context.OktaPolicies
                    .OrderBy(x => x.Priority)
                    .ToListAsync();
            }

            return await _externalService.GetPoliciesAsync();
        }
    }
}