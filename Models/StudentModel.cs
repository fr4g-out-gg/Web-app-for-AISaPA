using System.ComponentModel.DataAnnotations;

namespace AISaPA.Models
{
    // Model = "M" v MVC. Obyčajná trieda, ktorá popisuje dáta jedného študenta.
    // Controller ju posiela do view a model binding ju naopak vyplní z odoslaného formulára
    // (políčko <input name="Name"> sa zapíše do vlastnosti Name).
    //
    // Atribúty v [hranatých zátvorkách] sú validačné pravidlá (Data Annotations).
    // Kontrolujú sa dvakrát:
    //   - v prehliadači cez jQuery Validation (hneď pri písaní, bez odoslania),
    //   - na serveri v controlleri cez ModelState.IsValid (to je to dôležité - prehliadaču neveríme).
    public class StudentModel
    {
        // [Display] - text, ktorý sa použije v chybových hláškach ("The ID field is required.").
        // [Range] - ID musí byť kladné číslo.
        [Display(Name = "ID")]
        [Range(1, int.MaxValue, ErrorMessage = "ID must be a positive number.")]
        public int Id { get; set; }

        // string? = hodnota môže byť null (prázdne políčko). [Required] zabezpečí, že nebude.
        [Required(ErrorMessage = "Please enter a name.")]
        [StringLength(50)]
        public string? Name { get; set; }

        // [RegularExpression] - povolíme len hodnoty zo selectu. Bez toho by niekto mohol
        // poslať akýkoľvek text, napr. cez Postman alebo upravený HTML formulár.
        [Required(ErrorMessage = "Please select a gender.")]
        [RegularExpression("^(Male|Female)$", ErrorMessage = "Please select a gender.")]
        public string? Gender { get; set; }

        [Required(ErrorMessage = "Please enter a city.")]
        [StringLength(50)]
        public string? City { get; set; }
    }
}