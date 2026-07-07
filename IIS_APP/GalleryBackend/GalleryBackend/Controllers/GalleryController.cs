using GalleryBackend.Data;
using GalleryBackend.Models;
using GalleryBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Xml.Linq;


namespace GalleryBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GalleryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IGallerySourceService _gallerySourceService;
        private readonly IGalleryImportValidationService _validationService;

        public GalleryController(
            AppDbContext context, 
            IGallerySourceService gallerySourceService,
            IGalleryImportValidationService validationService)
        {
            _context = context;
            _gallerySourceService = gallerySourceService;
            _validationService = validationService;
        }

        [HttpGet]
        [Authorize(Roles = "ReadOnly,FullAccess")]
        public async Task<ActionResult<IEnumerable<GalleryItem>>> GetGalleryItems()
        {
            try
            {
                var items = await _gallerySourceService.GetGalleryItemsAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Greška prilikom dohvaćanja gallery podataka.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("xml")]
        public async Task<IActionResult> GetGalleryItemsAsXml()
        {
            var items = await _context.GalleryItems
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var xml = new System.Xml.Linq.XDocument(
                new System.Xml.Linq.XDeclaration("1.0", "UTF-8", null),
                new System.Xml.Linq.XElement("galleryItems",
                    items.Select(item =>
                        new System.Xml.Linq.XElement("galleryItem",
                            new System.Xml.Linq.XElement("imgurId", item.ImgurId),
                            new System.Xml.Linq.XElement("title", item.Title),
                            new System.Xml.Linq.XElement("link", item.Link),
                            new System.Xml.Linq.XElement("type", item.Type),
                            new System.Xml.Linq.XElement("views", item.Views),
                            new System.Xml.Linq.XElement("ups", item.Ups),
                            new System.Xml.Linq.XElement("downs", item.Downs),
                            new System.Xml.Linq.XElement("score", item.Score),
                            new System.Xml.Linq.XElement("isAlbum", item.IsAlbum.ToString().ToLower()),
                            new System.Xml.Linq.XElement("createdAt", item.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"))
                        )
                    )
                )
            );

            return Content(xml.ToString(), "application/xml");
        }

        [HttpGet("export-xml")]
        [Authorize(Roles = "FullAccess")]
        public async Task<IActionResult> ExportGalleryItemsToXmlFile()
        {
            var items = await _context.GalleryItems
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var xml = new System.Xml.Linq.XDocument(
                new System.Xml.Linq.XDeclaration("1.0", "UTF-8", null),
                new System.Xml.Linq.XElement("galleryItems",
                    items.Select(item =>
                        new System.Xml.Linq.XElement("galleryItem",
                            new System.Xml.Linq.XElement("imgurId", item.ImgurId),
                            new System.Xml.Linq.XElement("title", item.Title),
                            new System.Xml.Linq.XElement("link", item.Link),
                            new System.Xml.Linq.XElement("type", item.Type),
                            new System.Xml.Linq.XElement("views", item.Views),
                            new System.Xml.Linq.XElement("ups", item.Ups),
                            new System.Xml.Linq.XElement("downs", item.Downs),
                            new System.Xml.Linq.XElement("score", item.Score),
                            new System.Xml.Linq.XElement("isAlbum", item.IsAlbum.ToString().ToLower()),
                            new System.Xml.Linq.XElement("createdAt", item.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"))
                        )
                    )
                )
            );

            var folderPath = @"C:\IISProject\shared";
            var filePath = System.IO.Path.Combine(folderPath, "gallery-items.xml");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            await System.IO.File.WriteAllTextAsync(filePath, xml.ToString());

            return Ok(new
            {
                message = "XML datoteka je uspješno generirana.",
                path = filePath
            });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "ReadOnly,FullAccess")]
        public async Task<ActionResult<GalleryItem>> GetGalleryItem(int id)
        {
            var item = await _context.GalleryItems.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return item;
        }

        [HttpPost("import/json")]
        [Authorize(Roles = "FullAccess")]
        [Consumes("application/json")]
        public async Task<IActionResult> ImportGalleryItemFromJson([FromBody] JsonElement jsonElement)
        {
            var json = jsonElement.GetRawText();

            var validationResult = await _validationService.ValidateJsonAsync(json);

            if (!validationResult.IsValid || validationResult.Item == null)
            {
                return BadRequest(new
                {
                    message = "JSON validacija nije prošla.",
                    errors = validationResult.Errors
                });
            }

            _context.GalleryItems.Add(validationResult.Item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetGalleryItem),
                new { id = validationResult.Item.Id },
                validationResult.Item);
        }

        [HttpPost("import/xml")]
        [Authorize(Roles = "FullAccess")]
        [Consumes("application/xml", "text/xml")]
        public async Task<IActionResult> ImportGalleryItemFromXml([FromBody] GalleryItem item)
        {
            var xml = new XDocument(
                new XElement("galleryItem",
                    new XElement("imgurId", item.ImgurId),
                    new XElement("title", item.Title),
                    new XElement("link", item.Link),
                    new XElement("type", item.Type),
                    new XElement("views", item.Views),
                    new XElement("ups", item.Ups),
                    new XElement("downs", item.Downs),
                    new XElement("score", item.Score),
                    new XElement("isAlbum", item.IsAlbum.ToString().ToLower()),
                    new XElement("createdAt", item.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"))
                )
            );

            var validationResult = await _validationService.ValidateXmlAsync(xml.ToString());

            if (!validationResult.IsValid || validationResult.Item == null)
            {
                return BadRequest(new
                {
                    message = "XML validacija nije prošla.",
                    errors = validationResult.Errors
                });
            }

            _context.GalleryItems.Add(validationResult.Item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetGalleryItem),
                new { id = validationResult.Item.Id },
                validationResult.Item);
        }

        [HttpPost]
        [Authorize(Roles = "FullAccess")]
        public async Task<ActionResult<GalleryItem>> CreateGalleryItem(GalleryItem item)
        {
            if (item.CreatedAt == default)
            {
                item.CreatedAt = DateTime.UtcNow;
            }

            _context.GalleryItems.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGalleryItem), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "FullAccess")]
        public async Task<IActionResult> UpdateGalleryItem(int id, GalleryItem item)
        {
            if (id != item.Id)
            {
                return BadRequest("ID iz URL-a se ne podudara s ID-em objekta.");
            }

            var existingItem = await _context.GalleryItems.FindAsync(id);

            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.ImgurId = item.ImgurId;
            existingItem.Title = item.Title;
            existingItem.Link = item.Link;
            existingItem.Type = item.Type;
            existingItem.Views = item.Views;
            existingItem.Ups = item.Ups;
            existingItem.Downs = item.Downs;
            existingItem.Score = item.Score;
            existingItem.IsAlbum = item.IsAlbum;
            existingItem.CreatedAt = item.CreatedAt;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "FullAccess")]
        public async Task<IActionResult> DeleteGalleryItem(int id)
        {
            var item = await _context.GalleryItems.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            _context.GalleryItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}