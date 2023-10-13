using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ProductManager.Domain;
using static System.Console;

namespace ProductManagerConsole;

class Program
{
    // Httpclient finns tillgänglig, behöver inte installeras via NuGet
    //Den gör HTTP anrop över nätverket
    //För att göra HHTP.anrop ( text GET) behöver vi använda ett bibliotek
    //som kan göra detta. såsom HttpClient. Använder detta i våran metod GetForCast()

    static readonly HttpClient httpClient = new()
    {
        BaseAddress = new Uri("https://localhost:8000/")//adressen
    };
    static void Main()
    {
        // Gör att att HTTP-header "Authorization" skickas med i anropet till web APi:et
        // Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE2OTc...
        // Hämta strängen ifrån Thunder CLient logga in

        httpClient.DefaultRequestHeaders.Authorization
            = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE2OTcyMDczNzIsImV4cCI6MTY5NzIxMDk3MiwiaWF0IjoxNjk3MjA3MzcyfQ.riQbU2g3308tD9GeDXXEzuXf1ZmUEgEk7lAu09gcFUI");

        Title = "Product Manager";

        while (true) // Loop som körs tills vi stänger ner applikationen
        {
            WriteLine("1. Lägg till produkt");
            WriteLine("2. Sök produkt");
            WriteLine("3. Avsluta");

            var keyPressedMenuChoice = ReadKey(true); // Fångar in knapptryck

            Clear();

            switch (keyPressedMenuChoice.Key) //.Key hämtar knappen från ReadKey
            {
                case ConsoleKey.D1: // Case för knapptryck 1
                case ConsoleKey.NumPad1:

                    RegisterProduct();

                    break;

                case ConsoleKey.D2: // Case för knapptryck 2
                case ConsoleKey.NumPad2:

                    SearchProduct();

                    break;

                case ConsoleKey.D3: // Case för knapptryck 3
                case ConsoleKey.NumPad3:

                    Environment.Exit(0); // Metodanrop i C# 

                    return;
            }

            Clear();
        }
    }

    private static void SearchProduct()
    {
        throw new NotImplementedException();
    }

    //Metoderna
    private static void RegisterProduct() // Registrera en ny produkt
    {
        CursorVisible = true;

        Write("Namn: ");

        var name = ReadLine();

        Write("SKU: ");

        var sku = ReadLine();

        Write("Beskrivning: ");

        var description = ReadLine();

        Write("Bild (URL): ");

        var image = ReadLine();

        Write("Pris: ");

        var price = decimal.Parse(ReadLine());

        CursorVisible = false;


        var product = new Product // Samla ihop alla värden/för över värden i ett objekt/klass som representera Produkten.
        {
            Name = name,
            Sku = sku,
            Description = description,
            Image = image,
            Price = price
        };

        WriteLine();
        WriteLine();
        Write("Är detta korrekt? (J)a (N)ej");

        while (true)
        {
            var keyPressedConfirmRegisterProduct = ReadKey(true); // Väntar in knapptryck

            switch (keyPressedConfirmRegisterProduct.Key)
            {
                case ConsoleKey.J: // Case för knapptryck J

                    Clear();

                    SaveProduct(product); // Spara produkten

                    WriteLine("Produkt sparad");

                    Thread.Sleep(2000);

                    return;

                case ConsoleKey.N: // Case för knapptryck N

                    Clear();

                    RegisterProduct(); // Registrera produkten

                    return;
            }
        }

    }

    private static void SaveProduct(Product product)
    {
        // TODO Skicka information om filmen till web API:et genom att skicka 
        //      ett HTTP POST-anrop till https://localhost:8000/movies

        // 1 - Serialisera movie-objekt till JSON ({ "title": "Aliens", "plot": "Lorem ipsum dolor", ... })
        var json = JsonSerializer.Serialize(product);

        var body = new StringContent(
          json,
          Encoding.UTF8,
          // Beskriver formatet på data
          "application/json");

        // POST https://localhost:8000/movies
        var response = httpClient.PostAsync("products", body).Result;

        // Om statuskod är "400 Bad Request", kasta exception som du sedan fångar
        // där SaveMovie() anropas, och då visar meddelandet "Ogiltig information" i 2 sekunder.

        // Kasta exception om statuskoden inte ligger inom 2xx-omfånget.
        response.EnsureSuccessStatusCode();
    }

    private static void ListProductView()
    {
        // 1 - Hämta forecasts från backend (web api) (alltså skicka en HHTP GET förfrågan)
        var products = GetProducts();

        // 2 - Skriv ut en "tabell", på samma sätt som vi precis gjorde i vår web-applikation

        Write($"{"Name",-16}");
        Write($"{"Sku",-16}");
        WriteLine("Description");

        foreach (var product in products)
        {
            Write($"{product.Name,-16}");
            Write($"{product.Sku,-16}");
            WriteLine(product.Description);
        }

        // 3 - Vänta på att användaren trycker på escape, återvänd då till huvudmenyn
        WaitUntilKeyPressed(ConsoleKey.Escape);

    }

    //vi vill Returnera IEnumerable <WeatherForeCast>
    // IEnumarable inte är en datatyp i sig själv utan snarare
    // ett gränssnitt som används för att hantera upprepning över samlingar av data.

    private static IEnumerable<Product> GetProducts()
    {
        // 1 - Skicka ett HTTP GET-anrop till backend (web api)
        // HTTP finns det olika metoder (GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS, TRACE, ...)

        // https://localhost:8000/ + weatherforecast
        // Skickar en HTTP GET till https://localhost:8000/weatherforecast
        var response = httpClient.GetAsync("products").Result;

        // 2 - Läs ut JSON som vi fått tillbaka
        var json = response.Content.ReadAsStringAsync().Result;

        // 3 - Deserialisera JSON till ett objekt (IEnumerable<WeatherForecast>)
        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var products = JsonSerializer
            .Deserialize<IEnumerable<Product>>(json, serializeOptions)
            ?? new List<Product>();

        // 4 - Returnera resultatet (IEnumerable<WeatherForecast>)

        return products;
    }

    private static void WaitUntilKeyPressed(ConsoleKey key)
    {
        while (ReadKey(true).Key != key) ;
    }



}

