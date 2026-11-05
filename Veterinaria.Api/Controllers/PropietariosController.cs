using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Veterinaria.Api.Models;

namespace Veterinaria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropietariosController(VeterinariaContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Propietario>>> Get(CancellationToken ct)
        => Ok(await context.Propietarios.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Propietario>> GetById(int id, CancellationToken ct)
    {
        var propietario = await context.Propietarios.FindAsync([id], ct);
        return propietario is null ? NotFound() : Ok(propietario);
    }

    [HttpGet("{id:int}/mascotas")]
    public async Task<ActionResult<IEnumerable<Mascota>>> GetMascotas(int id, CancellationToken ct)
    {
        if (!await context.Propietarios.AnyAsync(p => p.Id == id, ct)) return NotFound();
        return Ok(await context.Mascotas.AsNoTracking().Where(m => m.PropietarioId == id)
            .OrderBy(m => m.Nombre).ToListAsync(ct));
    }

    [HttpPost]
    public async Task<ActionResult<Propietario>> Post(Propietario propietario, CancellationToken ct)
    {
        if (propietario.Id != 0) return BadRequest(new ProblemDetails { Status = 400, Title = "El Id se genera automáticamente." });
        if (await context.Propietarios.AnyAsync(p => p.Documento == propietario.Documento, ct))
            return DocumentoDuplicado();

        context.Propietarios.Add(propietario);
        try { await context.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { return DocumentoDuplicado(); }
        return CreatedAtAction(nameof(GetById), new { id = propietario.Id }, propietario);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, Propietario propietario, CancellationToken ct)
    {
        if (id != propietario.Id) return BadRequest(new ProblemDetails { Status = 400, Title = "El Id de la ruta debe coincidir con el cuerpo." });
        var actual = await context.Propietarios.FindAsync([id], ct);
        if (actual is null) return NotFound();
        if (await context.Propietarios.AnyAsync(p => p.Documento == propietario.Documento && p.Id != id, ct))
            return DocumentoDuplicado();

        actual.Nombre = propietario.Nombre;
        actual.Documento = propietario.Documento;
        actual.Telefono = propietario.Telefono;
        try { await context.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { return NotFound(); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { return DocumentoDuplicado(); }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var propietario = await context.Propietarios.FindAsync([id], ct);
        if (propietario is null) return NotFound();
        if (await context.Mascotas.AnyAsync(m => m.PropietarioId == id, ct)) return TieneMascotas();
        context.Propietarios.Remove(propietario);
        try { await context.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { return NotFound(); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        { return TieneMascotas(); }
        return NoContent();
    }

    private ConflictObjectResult DocumentoDuplicado() => Conflict(new ProblemDetails
    { Status = 409, Title = "Documento duplicado", Detail = "Ya existe un propietario con ese documento." });

    private ConflictObjectResult TieneMascotas() => Conflict(new ProblemDetails
    { Status = 409, Title = "Propietario con mascotas", Detail = "Elimina o reasigna sus mascotas antes de eliminar al propietario." });
}
