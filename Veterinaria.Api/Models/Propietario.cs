using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Veterinaria.Api.Models;

public partial class Propietario
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Nombre { get; set; } = null!;

    [Required, StringLength(8, MinimumLength = 8), RegularExpression(@"^[0-9]{8}$")]
    public string Documento { get; set; } = null!;

    [Required, StringLength(15, MinimumLength = 7), RegularExpression(@"^[0-9+ -]{7,15}$")]
    public string Telefono { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
}
