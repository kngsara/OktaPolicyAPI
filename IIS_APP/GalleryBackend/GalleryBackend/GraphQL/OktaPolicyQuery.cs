using OktaBackend.Data;
using OktaBackend.Models;
using Microsoft.EntityFrameworkCore;


namespace OktaBackend.GraphQL
{
    public class OktaPolicyQuery
    {
        public async Task<List<OktaPolicy>> GetOktaPolicies([Service] AppDbContext context)
        {
            return await context.OktaPolicies
                .OrderBy(x => x.Priority)
                .ToListAsync();
        }
    }
}