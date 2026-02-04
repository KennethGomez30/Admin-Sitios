using System.ComponentModel.DataAnnotations;


namespace Sistema_Contable.Entities
{
    public class Pantalla
    {
        public ulong pantalla_id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(40, ErrorMessage = "El nombre no debe ser mayor a 40 caracteres.")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ ]+$", ErrorMessage = "El nombre solo debe tener letras y espacios.")]
        public string nombre { get; set; } = "";

        [Required(ErrorMessage = "La descripción es requerida.")]
        [StringLength(200, ErrorMessage = "La descripción no debe ser mayor a 200 caracteres.")]
        [RegularExpression(@"^[A-Za-z0-9ÁÉÍÓÚÜÑáéíóúüñ ]+$", ErrorMessage = "La descripción solo permite letras, números y espacios.")]
        public string descripcion { get; set; } = "";

        [Required(ErrorMessage = "La ruta es requerida.")]
        [StringLength(255)]
        public string ruta { get; set; } = "";

        [Required(ErrorMessage = "El estado es requerido.")]
        [RegularExpression(@"^(Activa|Inactiva)$", ErrorMessage = "Estado inválido.")]
        public string estado { get; set; } = "Activa";
    }
}
