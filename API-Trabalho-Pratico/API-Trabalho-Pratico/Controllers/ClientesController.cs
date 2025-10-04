using API_Trabalho_Pratico;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly LocadoraDB _context;

        public ClientesController(LocadoraDB context)
        {
            _context = context;
        }

        // GET: api/Clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            try
            {
                var clientes = await _context.Clientes.ToListAsync();
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar clientes: {ex.Message}");
            }
        }

        // GET: api/Clientes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            try
            {
                var cliente = await _context.Clientes.FindAsync(id);

                if (cliente == null)
                    return NotFound("Cliente não encontrado.");

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar cliente: {ex.Message}");
            }
        }

        // POST: api/Clientes
        [HttpPost]
        public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Verifica se CPF ou e-mail já existem
                var cpfOuEmailExistem = await _context.Clientes
                    .AnyAsync(c => c.CPF == cliente.CPF || c.Email == cliente.Email);

                if (cpfOuEmailExistem)
                    return Conflict("Já existe um cliente com o mesmo CPF ou e-mail.");

                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, cliente);
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"Erro ao salvar no banco de dados: {dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }

        // PUT: api/Clientes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, Cliente cliente)
        {
            if (id != cliente.Id)
                return BadRequest("O ID informado não corresponde ao cliente.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var clienteExistente = await _context.Clientes.FindAsync(id);
                if (clienteExistente == null)
                    return NotFound("Cliente não encontrado.");

                // Verifica duplicidade de CPF e e-mail em outros clientes
                var duplicado = await _context.Clientes
                    .AnyAsync(c => (c.CPF == cliente.CPF || c.Email == cliente.Email) && c.Id != id);

                if (duplicado)
                    return Conflict("Já existe outro cliente com o mesmo CPF ou e-mail.");

                _context.Entry(clienteExistente).CurrentValues.SetValues(cliente);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar cliente: {ex.Message}");
            }
        }

        // DELETE: api/Clientes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            try
            {
                var cliente = await _context.Clientes.FindAsync(id);

                if (cliente == null)
                    return NotFound("Cliente não encontrado.");

                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir cliente: {ex.Message}");
            }
        }
    }
}
