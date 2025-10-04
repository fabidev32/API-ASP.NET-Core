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

        /// <summary>
        /// Retorna todos os clientes cadastrados.
        /// </summary>
        /// <returns>Lista de clientes</returns>
        /// <response code="200">Lista retornada com sucesso</response>
        /// <response code="500">Erro interno ao buscar clientes</response>
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

        /// <summary>
        /// Retorna um cliente específico pelo ID.
        /// </summary>
        /// <param name="id">ID do cliente</param>
        /// <returns>Cliente encontrado</returns>
        /// <response code="200">Cliente retornado com sucesso</response>
        /// <response code="404">Cliente não encontrado</response>
        /// <response code="500">Erro interno ao buscar cliente</response>
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

        /// <summary>
        /// Cadastra um novo cliente.
        /// </summary>
        /// <param name="cliente">Objeto cliente a ser cadastrado</param>
        /// <returns>Cliente criado</returns>
        /// <response code="201">Cliente criado com sucesso</response>
        /// <response code="400">Dados inválidos</response>
        /// <response code="409">Conflito: CPF ou e-mail já existe</response>
        /// <response code="500">Erro interno ao salvar cliente</response>
        [HttpPost]
        public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
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

        /// <summary>
        /// Atualiza um cliente existente pelo ID.
        /// </summary>
        /// <param name="id">ID do cliente</param>
        /// <param name="cliente">Objeto cliente com dados atualizados</param>
        /// <returns>NoContent se atualizado</returns>
        /// <response code="204">Atualizado com sucesso</response>
        /// <response code="400">ID inválido ou dados incorretos</response>
        /// <response code="404">Cliente não encontrado</response>
        /// <response code="409">Conflito: CPF ou e-mail duplicado</response>
        /// <response code="500">Erro interno ao atualizar cliente</response>
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

        /// <summary>
        /// Exclui um cliente pelo ID.
        /// </summary>
        /// <param name="id">ID do cliente</param>
        /// <response code="204">Cliente excluído com sucesso</response>
        /// <response code="404">Cliente não encontrado</response>
        /// <response code="500">Erro interno ao excluir cliente</response>
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
