using API_Trabalho_Pratico;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FuncionariosController : ControllerBase
    {
        private readonly LocadoraDB _context;

        public FuncionariosController(LocadoraDB context)
        {
            _context = context;
        }

        // GET: api/Funcionarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Funcionario>>> GetFuncionarios()
        {
            try
            {
                var funcionarios = await _context.Funcionarios.ToListAsync();
                return Ok(funcionarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao buscar funcionários: {ex.Message}");
            }
        }

        // GET: api/Funcionarios/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Funcionario>> GetFuncionario(int id)
        {
            try
            {
                var funcionario = await _context.Funcionarios.FindAsync(id);
                if (funcionario == null)
                    return NotFound("Funcionário não encontrado.");

                return Ok(funcionario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar funcionário: {ex.Message}");
            }
        }

        // POST: api/Funcionarios
        [HttpPost]
        public async Task<ActionResult<Funcionario>> PostFuncionario(Funcionario funcionario)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Evita duplicidade de CPF, caso exista esse campo
                var existe = await _context.Funcionarios
                    .AnyAsync(f => f.CPF == funcionario.CPF);

                if (existe)
                    return Conflict("Já existe um funcionário com este CPF.");

                _context.Funcionarios.Add(funcionario);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetFuncionario), new { id = funcionario.Id }, funcionario);
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

        // PUT: api/Funcionarios/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFuncionario(int id, Funcionario funcionario)
        {
            if (id != funcionario.Id)
                return BadRequest("ID do funcionário não corresponde.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _context.Entry(funcionario).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Funcionarios.AnyAsync(f => f.Id == id))
                    return NotFound("Funcionário não encontrado.");
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar funcionário: {ex.Message}");
            }
        }

        // DELETE: api/Funcionarios/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFuncionario(int id)
        {
            try
            {
                var funcionario = await _context.Funcionarios.FindAsync(id);
                if (funcionario == null)
                    return NotFound("Funcionário não encontrado.");

                _context.Funcionarios.Remove(funcionario);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao deletar funcionário: {ex.Message}");
            }
        }

        // GET: api/funcionarios/com-alugueis -> funcionários com aluguéis registrados 
        [HttpGet("com-alugueis")]
        public async Task<ActionResult<IEnumerable<object>>> GetFuncionariosComAlugueis()
        {
            var funcionarios = await _context.Funcionarios
                .Where(f => f.AlugueisRegistrados.Any())
                .Select(f => new
                {
                    f.Nome,
                    f.Cargo,
                    QuantidadeDeAlugueis = f.AlugueisRegistrados.Count
                })
                .ToListAsync();

            return Ok(funcionarios);
        }

    }
}
