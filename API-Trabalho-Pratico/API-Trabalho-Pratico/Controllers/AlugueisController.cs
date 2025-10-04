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

        // GET: api/Alugueis
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

        // GET: api/Alugueis/{id}
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

        // POST: api/Alugueis
        [HttpPost]
        public async Task<ActionResult<Aluguel>> PostAluguel(Aluguel aluguel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Verifica se o veículo já está alugado nesse período
                bool conflito = await _context.Alugueis.AnyAsync(a =>
                    a.VeiculoId == aluguel.VeiculoId &&
                    a.DataFim >= aluguel.DataInicio &&
                    a.DataInicio <= aluguel.DataFim);

                if (conflito)
                    return Conflict("Este veículo já está alugado nesse período.");

                // Cálculo automático (opcional)
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

        // PUT: api/Alugueis/{id}
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

                // Verifica conflito com outros aluguéis
                bool conflito = await _context.Alugueis.AnyAsync(a =>
                    a.VeiculoId == aluguel.VeiculoId &&
                    a.Id != id &&
                    a.DataFim >= aluguel.DataInicio &&
                    a.DataInicio <= aluguel.DataFim);

                if (conflito)
                    return Conflict("Este veículo já está alugado nesse período.");

                // Atualiza valores
                _context.Entry(aluguelExistente).CurrentValues.SetValues(aluguel);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar aluguel: {ex.Message}");
            }
        }

        // DELETE: api/Alugueis/{id}
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

        // GET: api/alugueis/detalhes -> alugueis detalhados com cliente e veículo
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

        // GET: api/alugueis/filtro?cliente=Maria&inicio=2024-01-01&fim=2024-12-31 => alugueis por cliente e intervalo de datas
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
