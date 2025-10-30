using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShopCart.Models;
using EShopCart.DTOs;
using AutoMapper;
using EShopCart.Repositories;
using System.Security.Claims;

namespace EShopCart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<ProductController> _logger;
        private readonly IMapper _mapper;

        // Constructor with Dependency Injection for Repository
        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, ILogger<ProductController> logger, IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
            _mapper = mapper;
        }

       

        // GET: All Products (Available to Everyone)

                #region GetAllProducts

        [HttpGet("all")]
        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                // Get the UserId from the authenticated user's claims
                var userIdClaim = User?.FindFirst("UserId")?.Value; 

                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("User not authenticated.");

                int userId = int.Parse(userIdClaim);

                // Fetch products for the current user (merchant)
                var products = await _productRepository.GetProductsByUserIdAsync(userId);

                if (products == null || !products.Any())
                    return NotFound("No products found for the merchant.");

                // Map the products to ProductDto
                var productDtos = _mapper.Map<List<ProductDto>>(products);

                return Ok(productDtos);
            }
            catch (Exception ex)  
            {
                _logger.LogError(ex, "Error fetching products: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion

        #region searchByCategoryName
        [HttpGet("category/{categoryName}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductsByCategory(string categoryName)
        {
            try
            {
                // Fetch category by its name
                var category = await _categoryRepository.GetCategoryByNameAsync(categoryName);

                // If category is not found, return 404
                if (category == null)
                    return NotFound("Category not found.");

                // Fetch products by CategoryId
                var products = await _productRepository.GetProductsByCategoryAsync(category.CategoryId); // Using CategoryId now
                if (!products.Any())
                    return NotFound("No products found under this category.");

                // Map the products to ProductDto
                var productDtos = _mapper.Map<List<ProductDto>>(products);

                // Return the product data
                return Ok(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching products by category name: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        #endregion

        #region categories
                                                                                                     
        // GET: All Categories (Merchant Only)
        [HttpGet("categories")]
        // [Authorize(Roles = "Merchant")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _categoryRepository.GetAllCategoriesAsync();
                var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);
                return Ok(categoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching categories: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        #endregion


        #region addProduct
        // POST: Add a Product (Merchant Only)
        [HttpPost]
        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> AddProduct([FromForm] ProductCreateDto productCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Ensure that the category exists
                var category = await _categoryRepository.GetCategoryByIdAsync(productCreateDto.CategoryId);
                if (category == null)
                    return BadRequest("Invalid category ID.");

                // Get the logged-in user's ID
                var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID is not available.");

                // Handle image upload
                string imageUrls = null;
                if (productCreateDto.ImageUrl != null)
                {
                    string fileName = $"{Guid.NewGuid()}_{productCreateDto.ImageUrl.FileName}";
                    string filePath = Path.Combine(@"C:\EShopCartProducts", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await productCreateDto.ImageUrl.CopyToAsync(stream);
                    }

                    imageUrls = $"/images/{fileName}";
                }

                // Map ProductCreateDto to Product
                var product = _mapper.Map<Product>(productCreateDto);
                product.ImageUrl = imageUrls; // Set the image URL
                product.UserId = int.Parse(userId); // Set the UserId of the merchant

                // Save the product
                await _productRepository.AddProductAsync(product);

                var productDto = _mapper.Map<ProductDto>(product);
                return CreatedAtAction(nameof(GetAllProducts), new { id = product.ProductId }, productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        #endregion

        #region updateProduct
        // PUT: Update Product (Merchant Only)
[HttpPut("{id}")]
[Authorize(Roles = "Merchant")]
public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductUpdateDto productUpdateDto)
{
    try
    {
        // Get the UserId from the authenticated user's claims
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("User is not authenticated.");
        }

        int loggedInMerchantId = int.Parse(userIdClaim);

        // Fetch the product by its ID
        var product = await _productRepository.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound("Product not found.");
        }

        // Check if the logged-in merchant owns this product
        if (product.UserId != loggedInMerchantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "You are not authorized to update this product.");
        }

        // Ensure that the category exists
        var category = await _categoryRepository.GetCategoryByIdAsync(productUpdateDto.CategoryId);
        if (category == null)
        {
            return BadRequest("Invalid category ID.");
        }

        // Handle image upload if a new image is provided
        if (productUpdateDto.ImageUrl != null)
        {
            string fileName = $"{Guid.NewGuid()}_{productUpdateDto.ImageUrl.FileName}";
            string filePath = Path.Combine(@"C:\EShopCartProducts", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await productUpdateDto.ImageUrl.CopyToAsync(stream);
            }

            // **Ensure consistency with AddProduct method**
            product.ImageUrl = $"/images/{fileName}"; // Store relative path
        }

        // Map the updates to the existing product entity
        _mapper.Map(productUpdateDto, product);

        // Save the updated product
        await _productRepository.UpdateProductAsync(product);

        return NoContent(); // Success, no content returned
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error updating product: {ex.Message}");
        return StatusCode(500, "Internal server error");
    }
}

#endregion

#region deleteProduct
        // DELETE: Delete Product (Merchant Only)
[HttpDelete("{id}")]
[Authorize(Roles = "Merchant")]
public async Task<IActionResult> DeleteProduct(int id)
{
    try
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("User is not authenticated.");
        }

        int loggedInMerchantId = int.Parse(userIdClaim);
        var product = await _productRepository.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound("Product not found.");
        }

        if (product.UserId != loggedInMerchantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "You are not authorized to modify this product.");
        }

        // Set quantity to 0 instead of deleting
        product.StockQuantity = 0;
        await _productRepository.UpdateProductAsync(product);

        return NoContent(); // Success, no content returned
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error updating product quantity: {ex.Message}");
        return StatusCode(500, "Internal server error");
    }
}

        #endregion

        #region summary
   [HttpGet("summary")]
public async Task<IActionResult> GetOrderSummary()
{
    var userIdClaim = User.FindFirst("UserId")?.Value;

    if (string.IsNullOrEmpty(userIdClaim))
    {
        return Unauthorized("User is not authenticated.");
    }

    if (!int.TryParse(userIdClaim, out int userId))
    {
        return BadRequest("Invalid UserId format.");
    }

    var orderSummary = await _productRepository.GetOrderSummaryAsync(userId);

    if (orderSummary == null || orderSummary.Count == 0)
    {
        return NotFound("No order summary found for this merchant.");
    }

    return Ok(orderSummary);
}
        #endregion
    
    }
}
