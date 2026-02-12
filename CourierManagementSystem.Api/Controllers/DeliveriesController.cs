using CourierManagementSystem.Api.Constants;
using CourierManagementSystem.Api.Models.Entities;
using CourierManagementSystem.Api.Models.DTOs.Requests;
using CourierManagementSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierManagementSystem.Api.Controllers;

[ApiController]
[Route(ApiConstants.DeliveriesRoutePrefix)]
[Authorize] 
public class DeliveriesController : ControllerBase
{
    private readonly IDeliveryService _deliveryService;
    private readonly ILogger<DeliveriesController> _logger;

    public DeliveriesController(
        IDeliveryService deliveryService,
        ILogger<DeliveriesController> logger)
    {
        _deliveryService = deliveryService ?? throw new ArgumentNullException(nameof(deliveryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [Authorize(Policy = ApiConstants.ManagerOrAdminPolicy)]
    [ProducesResponseType(typeof(IEnumerable<DeliveryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllDeliveries(
        [FromQuery] DateOnly? date,
        [FromQuery] long? courierId,
        [FromQuery] DeliveryStatus? status)
    {
        _logger.LogInformation("Getting deliveries with filters - Date: {Date}, CourierId: {CourierId}, Status: {Status}", 
            date, courierId, status);
        
        var deliveries = await _deliveryService.GetAllDeliveriesAsync(date, courierId, status);
        return Ok(deliveries);
    }

    [HttpGet("{" + ApiConstants.DeliveryIdParameter + "}")]
    [Authorize(Policy = ApiConstants.ManagerOrAdminPolicy)]
    [ProducesResponseType(typeof(DeliveryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDeliveryById(long id)
    {
        if (id <= 0)
        {
            return BadRequest(new { error = "Invalid delivery ID" });
        }

        var delivery = await _deliveryService.GetDeliveryByIdAsync(id);
        
        if (delivery == null)
        {
            return NotFound();
        }

        return Ok(delivery);
    }

    [HttpPost]
    [Authorize(Policy = ApiConstants.ManagerOrAdminPolicy)]
    [ProducesResponseType(typeof(DeliveryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateDelivery([FromBody] DeliveryRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Request cannot be null" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Failed to extract user ID from token");
            return Unauthorized();
        }

        var delivery = await _deliveryService.CreateDeliveryAsync(request, userId);
        
        _logger.LogInformation("Delivery created: {DeliveryId} by user: {UserId}", delivery.Id, userId);
        
        return CreatedAtAction(
            nameof(GetDeliveryById), 
            new { id = delivery.Id }, 
            delivery);
    }

    [HttpPut("{" + ApiConstants.DeliveryIdParameter + "}")]
    [Authorize(Policy = ApiConstants.ManagerOrAdminPolicy)]
    [ProducesResponseType(typeof(DeliveryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateDelivery(long id, [FromBody] DeliveryRequest request)
    {
        if (id <= 0)
        {
            return BadRequest(new { error = "Invalid delivery ID" });
        }

        if (request == null)
        {
            return BadRequest(new { error = "Request cannot be null" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var delivery = await _deliveryService.UpdateDeliveryAsync(id, request);
        
        if (delivery == null)
        {
            return NotFound();
        }

        _logger.LogInformation("Delivery updated: {DeliveryId}", delivery.Id);
        return Ok(delivery);
    }

    [HttpDelete("{" + ApiConstants.DeliveryIdParameter + "}")]
    [Authorize(Policy = ApiConstants.ManagerOrAdminPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteDelivery(long id)
    {
        if (id <= 0)
        {
            return BadRequest(new { error = "Invalid delivery ID" });
        }

        var result = await _deliveryService.DeleteDeliveryAsync(id);
        
        if (!result)
        {
            return NotFound();
        }

        _logger.LogInformation("Delivery deleted: {DeliveryId}", id);
        return NoContent();
    }

    [HttpPost(ApiConstants.GenerateDeliveriesEndpoint)]
    [Authorize(Policy = ApiConstants.ManagerOrAdminPolicy)]
    [ProducesResponseType(typeof(GenerateDeliveriesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GenerateDeliveries([FromBody] GenerateDeliveriesRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Request cannot be null" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Failed to extract user ID from token");
            return Unauthorized();
        }

        var response = await _deliveryService.GenerateDeliveriesAsync(request, userId);
        
        _logger.LogInformation("Generated {Count} deliveries by user: {UserId}", 
            response.GeneratedCount, userId);
        
        return Ok(response);
    }
}
