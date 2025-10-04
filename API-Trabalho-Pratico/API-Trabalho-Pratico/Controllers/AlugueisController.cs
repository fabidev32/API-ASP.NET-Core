using API_Trabalho_Pratico;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlugueisController : ControllerBase
    {
        private readonly LocadoraDB _context;

        public AlugueisController(LocadoraDB context)
        {
            _context = context;
        }

        /// <summary>
        /// Retorna todos os aluguéis cadastrados.
        /// </summary>
        /// <returns>Lista de aluguéis com cliente, veículo e funcionário</returns>
        /// <response code="200">Lista retornada com sucesso</response>
        /// <response code="500">Erro interno ao buscar aluguéis</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Aluguel>>> GetAlugueis()
        {
            try
            {
                var alugueis = await _context.Alugueis
                    .Include(a => a.Cliente)
                    .Include(a => a.Veiculo)
                    .Include(a => a.Funcionario)
                    .ToListAsync();

                return Ok(alugueis);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar alugueis: {ex.Message}");
            }
        }

        /// <summary>
        /// Retorna um aluguel específico pelo ID.
        /// </summary>
        /// <param name="id">ID do aluguel</param>
        /// <returns>Aluguel com cliente, veículo e funcionário</returns>
        /// <response code="200">Aluguel encontrado com sucesso</response>
        /// <response code="404">Aluguel não encontrado</response>
        /// <response code="500">Erro interno ao buscar aluguel</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Aluguel>> GetAluguel(int id)
        {
            try
            {
                var aluguel = await _context.Alugueis
                    .Include(a => a.Cliente)
                    .Include(a => a.Veiculo)
                    .Include(a => a.Funcionario)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (aluguel == null)
                    return NotFound("Aluguel não encontrado.");

                return Ok(aluguel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar aluguel: {ex.Message}");
            }
        }

        /// <summary>
        /// Cadastra um novo aluguel.
        /// </summary>
        /// <param name="aluguel">Objeto aluguel a ser cadastrado</param>
        /// <returns>Aluguel criado</returns>
        /// <response code="201">Aluguel criado com sucesso</response>
        /// <response code="400">Dados inválidos</response>
        /// <response code="409">Conflito: veículo já alugado nesse período</response>
        /// <response code="500">Erro interno ao salvar aluguel</response>
        [HttpPost]
        public async Task<ActionResult<Aluguel>> PostAluguel(Aluguel aluguel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                bool conflito = await _context.Alugueis.AnyAsync(a =>
                    a.VeiculoId == aluguel.VeiculoId &&
                    a.DataFim >= aluguel.DataInicio &&
                    a.DataInicio <= aluguel.DataFim);

                if (conflito)
                    return Conflict("Este veículo já está alugado nesse período.");

                var dias = (aluguel.DataFim - aluguel.DataInicio).TotalDays;
                if (dias > 0)
                    aluguel.ValorTotal = aluguel.ValorDiaria * (decimal)dias;

                _context.Alugueis.Add(aluguel);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAluguel), new { id = aluguel.Id }, aluguel);
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
        /// Atualiza um aluguel existente pelo ID.
        /// </summary>
        /// <param name="id">ID do aluguel</param>
        /// <param name="aluguel">Objeto aluguel com dados atualizados</param>
        /// <returns>NoContent se atualizado</returns>
        /// <response code="204">Atualizado com sucesso</response>
        /// <response code="400">ID inválido ou dados incorretos</response>
        /// <response code="404">Aluguel não encontrado</response>
        /// <response code="409">Conflito: veículo já alugado nesse período</response>
        /// <response code="500">Erro interno ao atualizar</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAluguel(int id, Aluguel aluguel)
        {
            if (id != aluguel.Id)
                return BadRequest("O ID informado não corresponde ao aluguel.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var aluguelExistente = await _context.Alugueis.FindAsync(id);
                if (aluguelExistente == null)
                    return NotFound("Aluguel não encontrado.");

                bool conflito = await _context.Alugueis.AnyAsync(a =>
                    a.VeiculoId == aluguel.VeiculoId &&
                    a.Id != id &&
                    a.DataFim >= aluguel.DataInicio &&
                    a.DataInicio <= aluguel.DataFim);

                if (conflito)
                    return Conflict("Este veículo já está alugado nesse período.");

                _context.Entry(aluguelExistente).CurrentValues.SetValues(aluguel);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar aluguel: {ex.Message}");
            }
        }

        /// <summary>
        /// Exclui um aluguel pelo ID.
        /// </summary>
        /// <param name="id">ID do aluguel</param>
        /// <response code="204">Aluguel excluído com sucesso</response>
        /// <response code="404">Aluguel não encontrado</response>
        /// <response code="500">Erro interno ao excluir</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAluguel(int id)
        {
            try
            {
                var aluguel = await _context.Alugueis.FindAsync(id);
                if (aluguel == null)
                    return NotFound("Aluguel não encontrado.");

                _context.Alugueis.Remove(aluguel);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir aluguel: {ex.Message}");
            }
        }

        /// <summary>
        /// Retorna alugueis detalhados com cliente e veículo.
        /// </summary>
        /// <returns>Lista com nome do cliente, modelo do veículo e datas do aluguel</returns>
        [HttpGet("detalhes")]
        public async Task<ActionResult<IEnumerable<object>>> GetAlugueisDetalhados()
        {
            var alugueis = await _context.Alugueis
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo)
                .Select(a => new
                {
                    Cliente = a.Cliente.Nome,
                    Veiculo = a.Veiculo.Modelo,
                    a.DataInicio,
                    a.DataFim
                })
                .ToListAsync();

            return Ok(alugueis);
        }

        /// <summary>
        /// Filtra alugueis por cliente e intervalo de datas.
        /// </summary>
        /// <param name="cliente">Nome parcial ou completo do cliente</param>
        /// <param name="inicio">Data de início do período</param>
        /// <param name="fim">Data de fim do período</param>
        /// <returns>Lista de aluguéis filtrados</returns>
        [HttpGet("filtro")]
        public async Task<ActionResult<IEnumerable<object>>> GetAlugueisPorPeriodo(
            [FromQuery] string cliente, [FromQuery] DateTime inicio, [FromQuery] DateTime fim)
        {
            var alugueis = await _context.Alugueis
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo)
                .Where(a => a.Cliente.Nome.Contains(cliente) && a.DataInicio >= inicio && a.DataFim <= fim)
                .Select(a => new
                {
                    Cliente = a.Cliente.Nome,
                    Veiculo = a.Veiculo.Modelo,
                    a.DataInicio,
                    a.DataFim
                })
                .ToListAsync();

            return Ok(alugueis);
        }
    }
}
