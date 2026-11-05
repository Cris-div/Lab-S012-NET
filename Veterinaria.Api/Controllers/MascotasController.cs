using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Veterinaria.Api.Models;

namespace Veterinaria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MascotasController(VeterinariaContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Mascota>>> Get(
        [Range(1, int.MaxValue)] int? propietarioId,
        [StringLength(80)] string? buscar,
        [StringLength(40)] string? especie,
        CancellationToken ct)
    {
        var consulta = context.Mascotas.Include(m => m.Propietario).AsNoTracking();
        if (propietarioId.HasValue) consulta = consulta.Where(m => m.PropietarioId == propietarioId);
        if (!string.IsNullOrWhiteSpace(buscar)) consulta = consulta.Where(m => m.Nombre.Contains(buscar));
        if (!string.IsNullOrWhiteSpace(especie)) consulta = consulta.Where(m => m.Especie == especie);
        return Ok(await consulta.OrderBy(m => m.Nombre).ToListAsync(ct));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Mascota>> GetById(int id, CancellationToken ct)
    {
        var mascota = await context.Mascotas.Include(m => m.Propietario).AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, ct);
        return mascota is null ? NotFound() : Ok(mascota);
    }

    [HttpPost]
    public async Task<ActionResult<Mascota>> Post(Mascota mascota, CancellationToken ct)
    {
        if (mascota.Id != 0) return BadRequest(new ProblemDetails { Status = 400, Title = "El Id se genera automáticamente." });
        if (!await context.Propietarios.AnyAsync(p => p.Id == mascota.PropietarioId, ct))
            return PropietarioInvalido();
        // Solo se vincula la FK; nunca se inserta un propietario recibido en el JSON.
        mascota.Propietario = null;
        context.Mascotas.Add(mascota);
        try { await context.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        { return PropietarioInvalido(); }
        return CreatedAtAction(nameof(GetById), new { id = mascota.Id }, mascota);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, Mascota mascota, CancellationToken ct)
    {
        if (id != mascota.Id) return BadRequest(new ProblemDetails { Status = 400, Title = "El Id de la ruta debe coincidir con el cuerpo." });
        var actual = await context.Mascotas.FindAsync([id], ct);
        if (actual is null) return NotFound();
        if (!await context.Propietarios.AnyAsync(p => p.Id == mascota.PropietarioId, ct))
            return PropietarioInvalido();

        actual.Nombre = mascota.Nombre;
        actual.Especie = mascota.Especie;
        actual.Edad = mascota.Edad;
        actual.Peso = mascota.Peso;
        actual.PropietarioId = mascota.PropietarioId;
        try { await context.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { return NotFound(); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        { return PropietarioInvalido(); }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var mascota = await context.Mascotas.FindAsync([id], ct);
        if (mascota is null) return NotFound();
        context.Mascotas.Remove(mascota);
        try { await context.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { return NotFound(); }
        return NoContent();
    }

    private ActionResult PropietarioInvalido()
    {
        ModelState.AddModelError(nameof(Mascota.PropietarioId), "El propietario indicado no existe.");
        return ValidationProblem(ModelState);
    }
}
