using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Api.Models;

public partial class Mascota
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Nombre { get; set; } = null!;

    [Required, StringLength(40)]
    public string Especie { get; set; } = null!;

    [Range(0, 50)]
    public int Edad { get; set; }

    [Range(typeof(decimal), "0.01", "1000", ParseLimitsInInvariantCulture = true)]
    public decimal Peso { get; set; }

    [Range(1, int.MaxValue)]
    public int PropietarioId { get; set; }

    public virtual Propietario? Propietario { get; set; }
}
