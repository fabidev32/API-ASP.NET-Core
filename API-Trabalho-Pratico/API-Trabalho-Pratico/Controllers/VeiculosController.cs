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

        /// <summary>
        /// Retorna todos os veículos cadastrados.
        /// </summary>
        /// <returns>Lista de veículos com informações do fabricante</returns>
        /// <response code="200">Lista retornada com sucesso</response>
        /// <response code="500">Erro interno ao buscar veículos</response>
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

        /// <summary>
        /// Retorna um veículo pelo ID.
        /// </summary>
        /// <param name="id">ID do veículo</param>
        /// <returns>Veículo com informações do fabricante</returns>
        /// <response code="200">Veículo encontrado</response>
        /// <response code="404">Veículo não encontrado</response>
        /// <response code="500">Erro ao buscar veículo</response>
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

        /// <summary>
        /// Cadastra um novo veículo.
        /// </summary>
        /// <param name="veiculo">Objeto veículo a ser cadastrado</param>
        /// <returns>Veículo criado</returns>
        /// <response code="201">Veículo criado com sucesso</response>
        /// <response code="400">Dados inválidos ou fabricante não existe</response>
        /// <response code="409">Veículo com mesma placa já existe</response>
        /// <response code="500">Erro interno ao salvar veículo</response>
        [HttpPost]
        public async Task<ActionResult<Veiculo>> PostVeiculo(Veiculo veiculo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var fabricanteExiste = await _context.Fabricantes.AnyAsync(f => f.Id == veiculo.FabricanteId);
                if (!fabricanteExiste)
                    return BadRequest("Fabricante informado não existe.");

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

        /// <summary>
        /// Atualiza um veículo existente.
        /// </summary>
        /// <param name="id">ID do veículo</param>
        /// <param name="veiculo">Objeto veículo atualizado</param>
        /// <response code="204">Atualização realizada com sucesso</response>
        /// <response code="400">ID não corresponde ou fabricante não existe</response>
        /// <response code="404">Veículo não encontrado</response>
        /// <response code="500">Erro ao atualizar veículo</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVeiculo(int id, Veiculo veiculo)
        {
            if (id != veiculo.Id)
                return BadRequest("ID do veículo não corresponde.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
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

        /// <summary>
        /// Exclui um veículo pelo ID.
        /// </summary>
        /// <param name="id">ID do veículo</param>
        /// <response code="204">Veículo excluído com sucesso</response>
        /// <response code="404">Veículo não encontrado</response>
        /// <response code="500">Erro ao excluir veículo</response>
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

        /// <summary>
        /// Retorna veículos filtrados pelo nome do fabricante.
        /// </summary>
        /// <param name="nomeFabricante">Nome do fabricante</param>
        /// <returns>Lista de veículos com informações do fabricante</returns>
        /// <response code="200">Lista retornada com sucesso</response>
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
