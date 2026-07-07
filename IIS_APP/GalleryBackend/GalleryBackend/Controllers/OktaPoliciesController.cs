using OktaBackend.Data;
using OktaBackend.Models;
using OktaBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Json.Schema;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;


namespace OktaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OktaPoliciesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IOktaPolicySourceService _sourceService;

        public OktaPoliciesController(
            AppDbContext context,
            IOktaPolicySourceService sourceService)
        {
            _context = context;
            _sourceService = sourceService;
        }

        [HttpGet]
        [Authorize(Roles = "ReadOnly,FullAccess")]
        public async Task<ActionResult<IEnumerable<OktaPolicy>>> GetPolicies()
        {
            try
            {
                var policies = await _sourceService.GetPoliciesAsync();
                return Ok(policies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Greška prilikom dohvaćanja Okta policy podataka.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "ReadOnly,FullAccess")]
        public async Task<ActionResult<OktaPolicy>> GetPolicy(int id)
        {
            var policy = await _context.OktaPolicies.FindAsync(id);

            if (policy == null)
            {
                return NotFound();
            }

            return policy;
        }

        [HttpPost]
        [Authorize(Roles = "FullAccess")]
        public async Task<ActionResult<OktaPolicy>> CreatePolicy(OktaPolicy policy)
        {
            if (policy.CreatedAt == default)
            {
                policy.CreatedAt = DateTime.UtcNow;
            }

            _context.OktaPolicies.Add(policy);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPolicy), new { id = policy.Id }, policy);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "FullAccess")]
        public async Task<IActionResult> UpdatePolicy(int id, OktaPolicy policy)
        {
            if (id != policy.Id)
            {
                return BadRequest("ID iz URL-a se ne podudara s ID-em objekta.");
            }

            var existingPolicy = await _context.OktaPolicies.FindAsync(id);

            if (existingPolicy == null)
            {
                return NotFound();
            }

            existingPolicy.OktaId = policy.OktaId;
            existingPolicy.Name = policy.Name;
            existingPolicy.Type = policy.Type;
            existingPolicy.Status = policy.Status;
            existingPolicy.Priority = policy.Priority;
            existingPolicy.Link = policy.Link;
            existingPolicy.CreatedAt = policy.CreatedAt;
            existingPolicy.LastUpdatedAt = policy.LastUpdatedAt;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "FullAccess")]
        public async Task<IActionResult> DeletePolicy(int id)
        {
            var policy = await _context.OktaPolicies.FindAsync(id);

            if (policy == null)
            {
                return NotFound();
            }

            _context.OktaPolicies.Remove(policy);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //za SOAP i Jakarta validator jer im treba spremna XML datoteka
        [HttpGet("export-xml")]
        [Authorize(Roles = "FullAccess")]
        public async Task<IActionResult> ExportXml()
        {
            var policies = await _sourceService.GetPoliciesAsync();

            var sharedFolder = @"C:\IISProject\shared";
            Directory.CreateDirectory(sharedFolder);

            var filePath = System.IO.Path.Combine(sharedFolder, "okta-policies.xml");

            var document = new System.Xml.Linq.XDocument(
                new System.Xml.Linq.XDeclaration("1.0", "UTF-8", null),
                new System.Xml.Linq.XElement("oktaPolicies",
                    policies.Select(policy =>
                        new System.Xml.Linq.XElement("oktaPolicy",
                            new System.Xml.Linq.XElement("oktaId", policy.OktaId),
                            new System.Xml.Linq.XElement("name", policy.Name),
                            new System.Xml.Linq.XElement("type", policy.Type),
                            new System.Xml.Linq.XElement("status", policy.Status),
                            new System.Xml.Linq.XElement("priority", policy.Priority),
                            new System.Xml.Linq.XElement("link", policy.Link),
                            new System.Xml.Linq.XElement("createdAt", policy.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")),
                            new System.Xml.Linq.XElement("lastUpdatedAt", policy.LastUpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? "")
                        )
                    )
                )
            );

            document.Save(filePath);

            return Ok(new
            {
                message = "Okta policies XML datoteka je uspješno generirana.",
                path = filePath
            });
        }

        [HttpPost("import/json")]
        [Authorize(Roles = "FullAccess")]
        [Consumes("application/json")]
        public async Task<IActionResult> ImportJson([FromBody] JsonElement jsonElement)
        {
            
            var schemaPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Schemas", "okta-policy.schema.json");

            if (!System.IO.File.Exists(schemaPath))
            {
                return StatusCode(500, new
                {
                    message = "JSON schema datoteka nije pronađena.",
                    path = schemaPath
                });
            }

            var jsonText = jsonElement.GetRawText();
            var schemaText = await System.IO.File.ReadAllTextAsync(schemaPath);
            var schema = JsonSchema.FromText(schemaText);

            using var jsonDocument = JsonDocument.Parse(jsonText);

            var result = schema.Evaluate(jsonDocument.RootElement);

            if (!result.IsValid)
            {
                var errors = new List<string>();

                CollectJsonValidationErrors(result, errors);

                return BadRequest(new
                {
                    message = "JSON validacija nije prošla.",
                    errors
                });
            }

            var policy = JsonSerializer.Deserialize<OktaPolicy>(
                jsonText,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (policy == null)
            {
                return BadRequest(new
                {
                    message = "JSON nije moguće pretvoriti u OktaPolicy."
                });
            }

            policy.Id = 0;

            _context.OktaPolicies.Add(policy);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPolicy), new { id = policy.Id }, policy);
            

        }


        [HttpPost("import/xml")]
        [Authorize(Roles = "FullAccess")]
        [Consumes("application/xml", "text/xml")]
        public async Task<IActionResult> ImportXml([FromBody] OktaPolicy item)
        {
            if (item == null)
            {
                return BadRequest(new
                {
                    message = "XML nije moguće pretvoriti u OktaPolicy objekt."
                });
            }

            var xml = new XDocument(
                new XElement("oktaPolicy",
                    new XElement("oktaId", item.OktaId),
                    new XElement("name", item.Name),
                    new XElement("type", item.Type),
                    new XElement("status", item.Status),
                    new XElement("priority", item.Priority),
                    new XElement("link", item.Link),
                    new XElement("createdAt", item.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")),
                    item.LastUpdatedAt.HasValue
                        ? new XElement("lastUpdatedAt", item.LastUpdatedAt.Value.ToString("yyyy-MM-ddTHH:mm:ss"))
                        : null
                )
            );

            var schemaPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Schemas", "okta-policy.xsd");

            if (!System.IO.File.Exists(schemaPath))
            {
                return StatusCode(500, new
                {
                    message = "XSD datoteka nije pronađena.",
                    path = schemaPath
                });
            }

            var errors = new List<string>();

            var schemas = new XmlSchemaSet();
            schemas.Add("", schemaPath);

            xml.Validate(schemas, (sender, args) =>
            {
                errors.Add(args.Message);
            });

            if (errors.Any())
            {
                return BadRequest(new
                {
                    message = "XML validacija nije prošla.",
                    errors
                });
            }

            item.Id = 0;

            if (item.CreatedAt == default)
            {
                item.CreatedAt = DateTime.UtcNow;
            }

            _context.OktaPolicies.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPolicy), new { id = item.Id }, item);
        }


        private static void CollectJsonValidationErrors(EvaluationResults result, List<string> errors)
        {
            if (result.Errors != null)
            {
                foreach (var error in result.Errors)
                {
                    errors.Add($"{result.InstanceLocation}: {error.Value}");
                }
            }

            if (result.Details == null)
            {
                return;
            }

            foreach (var detail in result.Details)
            {
                CollectJsonValidationErrors(detail, errors);
            }
        }
    }

}