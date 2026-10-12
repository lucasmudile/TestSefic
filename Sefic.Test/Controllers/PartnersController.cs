using Microsoft.AspNetCore.Mvc;
using Sefic.Api.Application.Dtos;
using Sefic.Api.Application.Interfaces;

namespace Sefic.Api.Controllers;

[ApiController]
[Route("api/partners")]
public class PartnersController(IPartnerService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePartnerRequest request, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var partner = await service.GetByIdAsync(id, cancellationToken);
        return Ok(partner);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] PartnerListQuery query, CancellationToken cancellationToken)
    {
        var result = await service.ListAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePartnerRequest request, CancellationToken cancellationToken)
    {
        var updated = await service.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }
}
