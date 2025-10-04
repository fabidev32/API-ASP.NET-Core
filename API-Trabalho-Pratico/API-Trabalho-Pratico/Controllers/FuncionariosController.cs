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

        /// <summary>
        /// Retorna todos os funcionários cadastrados.
        /// </summary>
        /// <returns>Lista de funcionários</returns>
        /// <response code="200">Lista retornada com sucesso</response>
        /// <response code="500">Erro interno ao buscar funcionários</response>
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

        /// <summary>
        /// Retorna um funcionário pelo ID.
        /// </summary>
        /// <param name="id">ID do funcionário</param>
        /// <returns>Objeto funcionário</returns>
        /// <response code="200">Funcionário encontrado</response>
        /// <response code="404">Funcionário não encontrado</response>
        /// <response code="500">Erro interno ao buscar funcionário</response>
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

        /// <summary>
        /// Cadastra um novo funcionário.
        /// </summary>
        /// <param name="funcionario">Objeto funcionário a ser cadastrado</param>
        /// <returns>Funcionário criado</returns>
        /// <response code="201">Funcionário criado com sucesso</response>
        /// <response code="400">Dados inválidos</response>
        /// <response code="409">Funcionário com mesmo CPF já existe</response>
        /// <response code="500">Erro interno ao salvar funcionário</response>
        [HttpPost]
        public async Task<ActionResult<Funcionario>> PostFuncionario(Funcionario funcionario)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var existe = await _context.Funcionarios.AnyAsync(f => f.CPF == funcionario.CPF);
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

        /// <summary>
        /// Atualiza um funcionário existente.
        /// </summary>
        /// <param name="id">ID do funcionário</param>
        /// <param name="funcionario">Objeto funcionário atualizado</param>
        /// <response code="204">Atualização realizada com sucesso</response>
        /// <response code="400">ID não corresponde ou dados inválidos</response>
        /// <response code="404">Funcionário não encontrado</response>
        /// <response code="500">Erro ao atualizar funcionário</response>
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

        /// <summary>
        /// Exclui um funcionário pelo ID.
        /// </summary>
        /// <param name="id">ID do funcionário</param>
        /// <response code="204">Funcionário excluído com sucesso</response>
        /// <response code="404">Funcionário não encontrado</response>
        /// <response code="500">Erro ao excluir funcionário</response>
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

        /// <summary>
        /// Retorna funcionários que possuem aluguéis registrados.
        /// </summary>
        /// <returns>Lista de funcionários com quantidade de aluguéis</returns>
        /// <response code="200">Lista retornada com sucesso</response>
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
