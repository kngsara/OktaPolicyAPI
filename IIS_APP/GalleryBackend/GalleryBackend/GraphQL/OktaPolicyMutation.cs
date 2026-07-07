using OktaBackend.Data;
using OktaBackend.GraphQL;
using OktaBackend.Models;
namespace OktaBackend.GraphQL
{
    public class OktaPolicyMutation
    {
        public async Task<OktaPolicy> CreateOktaPolicy(
            CreateOktaPolicyInput input,
            [Service] AppDbContext context)
        {
            var policy = new OktaPolicy
            {
                OktaId = input.OktaId,
                Name = input.Name,
                Type = input.Type,
                Status = input.Status,
                Priority = input.Priority,
                Link = input.Link,
                CreatedAt = input.CreatedAt == default
                    ? DateTime.UtcNow
                    : input.CreatedAt,
                LastUpdatedAt = input.LastUpdatedAt
            };

            context.OktaPolicies.Add(policy);
            await context.SaveChangesAsync();

            return policy;
        }

        public async Task<bool> DeleteOktaPolicy(
            int id,
            [Service] AppDbContext context)
        {
            var policy = await context.OktaPolicies.FindAsync(id);

            if (policy == null)
            {
                return false;
            }

            context.OktaPolicies.Remove(policy);
            await context.SaveChangesAsync();

            return true;
        }
    }
}