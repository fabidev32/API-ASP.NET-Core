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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Veiculo>>> GetVeiculos()
        {
            return await _context.Veiculos.Include(v => v.Fabricante).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Veiculo>> PostVeiculo(Veiculo veiculo)
        {
            _context.Veiculos.Add(veiculo);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetVeiculos), new { id = veiculo.Id }, veiculo);
        }
    }
}
