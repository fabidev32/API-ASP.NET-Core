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

        /// <summary>
        /// Retorna todos os fabricantes cadastrados.
        /// </summary>
        /// <returns>Lista de fabricantes</returns>
        /// <response code="200">Lista retornada com sucesso</response>
        /// <response code="500">Erro interno ao buscar fabricantes</response>
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

        /// <summary>
        /// Cadastra um novo fabricante.
        /// </summary>
        /// <param name="fabricante">Objeto fabricante a ser cadastrado</param>
        /// <returns>Fabricante criado</returns>
        /// <response code="201">Fabricante criado com sucesso</response>
        /// <response code="400">Dados inválidos</response>
        /// <response code="409">Conflito: fabricante com mesmo nome já existe</response>
        /// <response code="500">Erro interno ao salvar fabricante</response>
        [HttpPost]
        public async Task<ActionResult<Fabricante>> PostFabricante(Fabricante fabricante)
        {
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

        /// <summary>
        /// Retorna todos os fabricantes com seus veículos (incluindo os fabricantes que não possuem veículos).
        /// </summary>
        /// <returns>Lista de fabricantes com veículos</returns>
        /// <response code="200">Lista retornada com sucesso</response>
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
