using System.Text.Json;

namespace AISaPA.Models
{
    // Repository = miesto, kde sa ukladajú a čítajú dáta. Controller nevie (a nemusí vedieť),
    // KDE sú dáta uložené - len zavolá GetAll(), GetById() alebo Create().
    // Keby sme neskôr prešli na databázu, zmení sa len táto trieda, controller zostane rovnaký.
    //
    // Študentov držíme v pamäti (List) a po každej zmene ich zapíšeme do JSON súboru.
    // Vďaka tomu nezmiznú, keď sa aplikácia reštartuje (na Azure F1 sa to deje často).
    //
    // Trieda je v Program.cs zaregistrovaná ako Singleton - všetky požiadavky (všetci používatelia
    // naraz) zdieľajú tú istú inštanciu. Preto prístup k zoznamu chránime zámkom (lock).
    public class Repository
    {
        private readonly List<StudentModel> students = new List<StudentModel>();

        // Objekt pre lock. Kým jedno vlákno je vnútri lock (sync) { ... }, ostatné čakajú.
        // Bez toho by dve súčasné požiadavky mohli naraz meniť List a pokaziť ho.
        private readonly object sync = new object();

        private readonly string filePath;
        private readonly ILogger<Repository> logger;

        // Parametre konštruktora dodá DI automaticky:
        //   IWebHostEnvironment - info o aplikácii (napr. v akom priečinku beží),
        //   ILogger             - zápis do logu (konzola, Azure Log stream).
        public Repository(IWebHostEnvironment env, ILogger<Repository> logger)
        {
            this.logger = logger;

            // Kam uložiť súbor:
            //   - na Azure App Service do %HOME%\data (napr. D:\home\data) - tento priečinok
            //     prežije reštart aj nové nasadenie (publish ho neprepíše),
            //   - lokálne do priečinka App_Data v projekte.
            // Premennú WEBSITE_SITE_NAME nastavuje len Azure, podľa nej zistíme, kde bežíme.
            string? home = Environment.GetEnvironmentVariable("HOME");
            bool onAzure = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") != null;
            string dataDir = onAzure && home != null
                ? Path.Combine(home, "data", "cv910")
                : Path.Combine(env.ContentRootPath, "App_Data");

            filePath = Path.Combine(dataDir, "students.json");
            Load();
        }

        // Vraciame KÓPIU zoznamu (ToList), nie samotný List. Keby sme vrátili originál,
        // view by ho prechádzal foreach-om, zatiaľ čo iná požiadavka do neho pridáva -> výnimka.
        public List<StudentModel> GetAll()
        {
            lock (sync)
            {
                return students.OrderBy(s => s.Id).ToList();
            }
        }

        // FirstOrDefault vráti prvého študenta s daným ID, alebo null, ak neexistuje.
        // Preto návratový typ StudentModel? (otáznik = môže byť null).
        public StudentModel? GetById(int id)
        {
            lock (sync)
            {
                return students.FirstOrDefault(s => s.Id == id);
            }
        }

        // Vráti false, ak študent s rovnakým ID už existuje - controller potom ukáže chybu vo formulári.
        // Kontrola aj pridanie sú v jednom lock-u, aby medzi nimi nemohol niekto iný pridať rovnaké ID.
        public bool Create(StudentModel student)
        {
            lock (sync)
            {
                if (students.Any(s => s.Id == student.Id))
                {
                    return false;
                }

                students.Add(student);
                Save();
                return true;
            }
        }

        // Načítanie zo súboru pri štarte aplikácie. Ak súbor ešte neexistuje, začíname s prázdnym zoznamom.
        private void Load()
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                // JSON text -> List<StudentModel>
                var loaded = JsonSerializer.Deserialize<List<StudentModel>>(File.ReadAllText(filePath));
                if (loaded != null)
                {
                    students.AddRange(loaded);
                }
            }
            catch (JsonException ex)
            {
                // Poškodený súbor nezhodí celú aplikáciu - zapíšeme chybu do logu a ideme ďalej.
                logger.LogError(ex, "Could not read {File}, starting with an empty list.", filePath);
            }
        }

        // List<StudentModel> -> JSON text -> súbor. WriteIndented = pekne odsadený, čitateľný JSON.
        private void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, JsonSerializer.Serialize(students, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
