using AISaPA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AISaPA.Controllers
{
    // Controller = "C" v MVC. Prijme HTTP požiadavku, pracuje s modelom/repository
    // a rozhodne, čo vráti (view, presmerovanie, chybu).
    //
    // Názov triedy určuje URL: StudentController -> /Student/...
    // Každá public metóda je "akcia":   Read() -> /Student/Read
    // A akcia View() hľadá súbor Views/Student/<NázovAkcie>.cshtml.
    public class StudentController : Controller
    {
        private readonly Repository repository;

        // Dependency Injection: Repository nevytvárame cez "new", ale vypýtame si ho v konštruktore.
        // ASP.NET Core nám dá inštanciu zaregistrovanú v Program.cs (AddSingleton).
        public StudentController(Repository repository)
        {
            this.repository = repository;
        }

        // GET /Student/Read (alebo len "/" - je to predvolená akcia, viď Program.cs)
        // Pošle zoznam študentov do Views/Student/Read.cshtml.
        public IActionResult Read()
        {
            return View(repository.GetAll());
        }

        // GET /Student/Student_info/5
        // Parameter "id" sa automaticky naplní z URL (route {id?}) - to je model binding.
        // Funguje aj /Student/Student_info?id=5
        public IActionResult Student_info(int id)
        {
            StudentModel? student = repository.GetById(id);
            if (student == null)
            {
                // Študent neexistuje -> HTTP 404 a vlastný view Views/Student/NotFound.cshtml.
                // Modelom view je tu len číslo (int id), aby sme ho mohli vypísať v hláške.
                Response.StatusCode = StatusCodes.Status404NotFound;
                return View("NotFound", id);
            }

            return View(student);
        }

        // GET /Student/Create - zobrazí prázdny formulár.
        public IActionResult Create()
        {
            return View();
        }

        // POST /Student/Create - spracuje odoslaný formulár.
        // Rovnaký názov ako metóda vyššie; [HttpPost] určuje, že táto sa volá pri odoslaní (POST).
        //
        // Model binding: ASP.NET z polí formulára (Id, Name, Gender, City) vytvorí StudentModel
        // a zároveň skontroluje validačné atribúty z modelu -> výsledok je v ModelState.
        //
        // [ValidateAntiForgeryToken] - ochrana proti CSRF. <form> tag helper do formulára vloží
        // skryté pole s tokenom; požiadavka bez platného tokenu (napr. z cudzej stránky) skončí chybou 400.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StudentModel studentModel)
        {
            // Ak sú dáta platné, skúsime študenta uložiť. Create() vráti false, keď ID už existuje -
            // vtedy pridáme vlastnú chybu k políčku Id (zobrazí sa pod ním cez asp-validation-for).
            if (ModelState.IsValid && !repository.Create(studentModel))
            {
                ModelState.AddModelError(nameof(StudentModel.Id), $"A student with ID {studentModel.Id} already exists.");
            }

            // Chyba -> ten istý formulár znova, aj s vyplnenými hodnotami a chybovými hláškami.
            if (!ModelState.IsValid)
            {
                return View(studentModel);
            }

            // Vzor Post/Redirect/Get (PRG): po úspešnom POST nevraciame view, ale presmerujeme na GET.
            // Keby sme vrátili View("Thanks"), F5 v prehliadači by formulár odoslal znova
            // a študent by sa pridal dvakrát.
            return RedirectToAction(nameof(Thanks), new { id = studentModel.Id });
        }

        // GET /Student/Thanks/5 - poďakovanie po pridaní študenta.
        public IActionResult Thanks(int id)
        {
            StudentModel? student = repository.GetById(id);
            if (student == null)
            {
                return RedirectToAction(nameof(Read));
            }

            return View(student);
        }
    }
}