using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIPedidos.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly PedidosDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public PedidosController(PedidosDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    // GET: api/Pedidos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
    {
        return await _context.Pedidos.ToListAsync();
    }

    // GET: api/Pedidos/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Pedido>> GetPedido(int id)
    {
        var pedido = await _context.Pedidos.FindAsync(id);

        if (pedido == null)
        {
            return NotFound();
        }

        return pedido;
    }

    // GET: api/Pedidos/cliente/5
    [HttpGet("cliente/{clienteId}")]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidosByCliente(int clienteId)
    {
        return await _context.Pedidos
            .Where(p => p.ClienteId == clienteId)
            .ToListAsync();
    }

    // PUT: api/Pedidos/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPedido(int? id, Pedido pedido)
    {
        if (id != pedido.Id)
        {
            return BadRequest();
        }

        var clienteValidation = await ValidateCliente(pedido.ClienteId);
        if (clienteValidation != null)
        {
            return clienteValidation;
        }

        _context.Entry(pedido).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PedidoExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Pedidos
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<IActionResult> PostPedido(Pedido pedido)
    {
        var clienteValidation = await ValidateCliente(pedido.ClienteId);
        if (clienteValidation != null)
        {
            return clienteValidation;
        }

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetPedido", new { id = pedido.Id }, pedido);
    }

    // DELETE: api/Pedidos/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePedido(int? id)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido == null)
        {
            return NotFound();
        }

        _context.Pedidos.Remove(pedido);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PedidoExists(int? id)
    {
        return _context.Pedidos.Any(e => e.Id == id);
    }

    private async Task<IActionResult?> ValidateCliente(int clienteId)
    {
        var client = _httpClientFactory.CreateClient("ClientesApi");
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/Clientes/{clienteId}");

        if (Request.Headers.TryGetValue("Authorization", out var authorization))
        {
            request.Headers.TryAddWithoutValidation("Authorization", authorization.ToString());
        }

        try
        {
            using var response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return BadRequest($"El cliente con Id {clienteId} no existe.");
            }

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    "No fue posible validar el cliente en APIClientes.");
            }
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                "APIClientes no está disponible para validar el cliente.");
        }

        return null;
    }
}
