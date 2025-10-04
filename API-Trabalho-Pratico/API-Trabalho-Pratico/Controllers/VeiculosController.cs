using API_Trabalho_Pratico;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VeiculosController : ControllerBase
    {
        private readonly LocadoraDB _context;

        public VeiculosController(LocadoraDB context)
        {
            _context = context;
        }

        // GET: api/Veiculos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Veiculo>>> GetVeiculos()
        {
            try
            {
                var veiculos = await _context.Veiculos
                    .Include(v => v.Fabricante)
                    .ToListAsync();

                return Ok(veiculos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao buscar veículos: {ex.Message}");
            }
        }

        // GET: api/Veiculos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Veiculo>> GetVeiculo(int id)
        {
            try
            {
                var veiculo = await _context.Veiculos
                    .Include(v => v.Fabricante)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (veiculo == null)
                    return NotFound("Veículo não encontrado.");

                return Ok(veiculo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar veículo: {ex.Message}");
            }
        }

        // POST: api/Veiculos
        [HttpPost]
        public async Task<ActionResult<Veiculo>> PostVeiculo(Veiculo veiculo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Verifica se o fabricante informado existe
                var fabricanteExiste = await _context.Fabricantes.AnyAsync(f => f.Id == veiculo.FabricanteId);
                if (!fabricanteExiste)
                    return BadRequest("Fabricante informado não existe.");

                // Evita veículos duplicados (por exemplo, pela placa)
                var existe = await _context.Veiculos.AnyAsync(v => v.Placa == veiculo.Placa);
                if (existe)
                    return Conflict("Já existe um veículo cadastrado com essa placa.");

                _context.Veiculos.Add(veiculo);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetVeiculo), new { id = veiculo.Id }, veiculo);
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

        // PUT: api/Veiculos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVeiculo(int id, Veiculo veiculo)
        {
            if (id != veiculo.Id)
                return BadRequest("ID do veículo não corresponde.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Verifica se o fabricante informado existe
                var fabricanteExiste = await _context.Fabricantes.AnyAsync(f => f.Id == veiculo.FabricanteId);
                if (!fabricanteExiste)
                    return BadRequest("Fabricante informado não existe.");

                _context.Entry(veiculo).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Veiculos.AnyAsync(v => v.Id == id))
                    return NotFound("Veículo não encontrado.");

                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar veículo: {ex.Message}");
            }
        }

        // DELETE: api/Veiculos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVeiculo(int id)
        {
            try
            {
                var veiculo = await _context.Veiculos.FindAsync(id);
                if (veiculo == null)
                    return NotFound("Veículo não encontrado.");

                _context.Veiculos.Remove(veiculo);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao deletar veículo: {ex.Message}");
            }
        }

        // GET: api/veiculos/fabricante/{nomeFabricante} - veículo por fabricante 
        [HttpGet("fabricante/{nomeFabricante}")]
        public async Task<ActionResult<IEnumerable<object>>> GetVeiculosPorFabricante(string nomeFabricante)
        {
            var veiculos = await _context.Veiculos
                .Include(v => v.Fabricante)
                .Where(v => v.Fabricante.Nome.Contains(nomeFabricante))
                .Select(v => new
                {
                    v.Modelo,
                    v.Placa,
                    v.Ano,
                    Fabricante = v.Fabricante.Nome
                })
                .ToListAsync();

            return Ok(veiculos);
        }

    }
}
