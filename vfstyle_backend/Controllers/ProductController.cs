using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vfstyle_backend.Data;
using vfstyle_backend.Models.Domain;
using vfstyle_backend.Models.DTOs;

namespace vfstyle_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] string category = null, [FromQuery] string style = null)
        {
            var query = _context.Products
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category.Name.Contains(category));
            }

            if (!string.IsNullOrEmpty(style))
            {
                query = query.Where(p => p.Style.Contains(style));
            }

            var products = await query
                .Select(p => new
                {
                    id = p.Id,
                    sku = p.Sku,
                    name = p.Name,
                    price = p.Price,
                    imageUrl = p.ImageUrl,
                    style = p.Style,
                    category = p.Category.Name
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(string id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return Ok(new
            {
                id = product.Id,
                sku = product.Sku,
                name = product.Name,
                price = product.Price,
                description = product.Description,
                imageUrl = product.ImageUrl,
                style = product.Style,
                material = product.Material,
                faceShapeRecommendation = product.FaceShapeRecommendation,
                category = product.Category?.Name
            });
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    description = c.Description
                })
                .ToListAsync();

            return Ok(categories);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDto productDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = new Product
            {
                Id = productDto.Id,
                Sku = productDto.Sku,
                Name = productDto.Name,
                Price = productDto.Price,
                Description = productDto.Description,
                ImageUrl = productDto.ImageUrl,
                Style = productDto.Style,
                Material = productDto.Material,
                FaceShapeRecommendation = productDto.FaceShapeRecommendation,
                CategoryId = productDto.CategoryId,
                Keywords = productDto.Keywords,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(string id, [FromBody] ProductDto productDto)
        {
            if (id != productDto.Id)
                return BadRequest();

            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            product.Sku = productDto.Sku;
            product.Name = productDto.Name;
            product.Price = productDto.Price;
            product.Description = productDto.Description;
            product.ImageUrl = productDto.ImageUrl;
            product.Style = productDto.Style;
            product.Material = productDto.Material;
            product.FaceShapeRecommendation = productDto.FaceShapeRecommendation;
            product.CategoryId = productDto.CategoryId;
            product.Keywords = productDto.Keywords;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            // Soft delete
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(string id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
