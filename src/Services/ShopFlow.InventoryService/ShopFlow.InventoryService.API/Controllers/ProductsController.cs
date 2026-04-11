using Microsoft.AspNetCore.Mvc;
using ShopFlow.InventoryService.Application.DTOs;
using ShopFlow.InventoryService.Application.Interfaces;

namespace ShopFlow.InventoryService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public ProductsController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    /// <summary>Adds a new product to inventory.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponseDto),StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddProduct([FromBody] CreateProductRequestDto request)
    {
        var result = await _inventoryService.CreateProductAsync(request);
        return CreatedAtAction(
            nameof(GetProductById), new { id = result.Id }, result);
    }

    /// <summary>Gets a product by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponseDto),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var result = await _inventoryService.GetProductByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Gets all products.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponseDto>),StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProducts()
    {
        var result = await _inventoryService.GetAllProductsAsync();
        return Ok(result);
    }

    /// <summary>Updates stock quantity for a product.</summary>
    [HttpPatch("{id:guid}/stock")]
    [ProducesResponseType(typeof(ProductResponseDto),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockRequestDto request)
    {
        var result = await _inventoryService
            .UpdateStockAsync(id, request.Quantity);
        return Ok(result);
    }
}