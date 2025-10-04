using API_Trabalho_Pratico;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FabricantesController : ControllerBase
    {
        private readonly LocadoraDB _context;

        public FabricantesController(LocadoraDB context)
        {
            _context = context;
        }

        // GET: api/Fabricantes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Fabricante>>> GetFabricantes()
        {
            try
            {
                var fabricantes = await _context.Fabricantes.ToListAsync();
                return Ok(fabricantes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao buscar fabricantes: {ex.Message}");
            }
        }

        // POST: api/Fabricantes
        [HttpPost]
        public async Task<ActionResult<Fabricante>> PostFabricante(Fabricante fabricante)
        {
            // Validação dos dados enviados no corpo da requisição
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var existe = await _context.Fabricantes.AnyAsync(f => f.Nome == fabricante.Nome);
                if (existe)
                    return Conflict("Já existe um fabricante com esse nome.");

                _context.Fabricantes.Add(fabricante);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetFabricantes), new { id = fabricante.Id }, fabricante);
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

        // GET: api/fabricantes/veiculos => fabricantes com seus veículos (incluindo os que não tem)
        [HttpGet("veiculos")]
        public async Task<ActionResult<IEnumerable<object>>> GetFabricantesComVeiculos()
        {
            var fabricantes = await _context.Fabricantes
                .Include(f => f.Veiculos)
                .Select(f => new
                {
                    Fabricante = f.Nome,
                    Veiculos = f.Veiculos.Select(v => v.Modelo).ToList()
                })
                .ToListAsync();

            return Ok(fabricantes);
        }

    }
}
