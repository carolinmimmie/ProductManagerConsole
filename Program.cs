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
            = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE2OTczODM0NjQsImV4cCI6MTY5NzM4NzA2NCwiaWF0IjoxNjk3MzgzNDY0fQ.pvcwUqC4XAEIfOO-LsrmzSciNFJ3Zm6lMvzRdWH-waU");

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

                    RegisterProductView();

                    break;

                case ConsoleKey.D2: // Case för knapptryck 2
                case ConsoleKey.NumPad2:

                    SearchProductView();

                    break;

                case ConsoleKey.D3: // Case för knapptryck 3
                case ConsoleKey.NumPad3:

                    Environment.Exit(0); // Metodanrop i C# 

                    return;
            }

            Clear();
        }
    }

    private static void SearchProductView()
    {
        CursorVisible = true;

        Write("SKU: ");

        string sku = ReadLine();

        CursorVisible = false;

        Clear();

        var product = GetProductBySku(sku);

        if (product is not null) // Om produkten hittades
        {
            WriteLine($"Namn: {product.Name}");
            WriteLine($"Sku: {product.Sku}");
            WriteLine($"Beskrivning: {product.Description}");
            WriteLine($"Bild (URL): {product.Image}");
            WriteLine($"Pris: {product.Price}");
            WriteLine("");
            WriteLine("");
            WriteLine("(R)adera" + "  " + "(U)ppdatera");

            while (true)
            {
                var keyPressedDeleteOrUpdateProduct = ReadKey(true); // Väntar in knapptryck

                switch (keyPressedDeleteOrUpdateProduct.Key) // Hantera knapptryck
                {
                    case ConsoleKey.R: // Case för knapptryck R

                        DeleteProduct(product);

                        return;

                    case ConsoleKey.Escape: // Case för knapptryck Escape

                        return;

                    case ConsoleKey.U: // Case för knapptryck U

                        UpdateProduct(product);

                        return;
                }
            }
        }
        else
        {
            WriteLine("Produkt finns ej");

            Thread.Sleep(2000);
        }

    }

    private static void UpdateProduct(Product product)
    {
        throw new NotImplementedException();
    }

    private static void DeleteProduct(Product product)
    {

        Clear();

        WriteLine($"Namn: {product.Name}");
        WriteLine($"Sku: {product.Sku}");
        WriteLine($"Beskrivning: {product.Description}");
        WriteLine($"Bild: {product.Image}");
        WriteLine($"Pris: {product.Price}");
        WriteLine("");
        WriteLine("");
        WriteLine("Radera produkt? (J)a eller (N)ej");

        while (true)
        {

            var keyPressedConfirmDeleteProduct = ReadKey(true); // Väntar in knapptryck

            switch (keyPressedConfirmDeleteProduct.Key)
            {
                case ConsoleKey.J:
                    Clear();

                    // DELETE https://localhost:8000/products/{id}
                    var response = httpClient.GetAsync("products").Result;

                    // Kommer kasta Exception om statuskod inte var något i 2xx-omfånget (t.ex. 404 Not Found)
                    response.EnsureSuccessStatusCode();

                    WriteLine("Produkt raderad");

                    Thread.Sleep(2000);

                    return;

                case ConsoleKey.N:

                    Clear();

                    WriteLine($"Namn: {product.Name}");
                    WriteLine($"Sku: {product.Sku}");
                    WriteLine($"Beskrivning: {product.Description}");
                    WriteLine($"Bild: {product.Image}");
                    WriteLine($"Pris: {product.Price}");
                    WriteLine("");
                    WriteLine("");
                    WriteLine("(R)adera" + "  " + "(U)ppdatera");

                    break;
            }

            switch (keyPressedConfirmDeleteProduct.Key)

            {
                case ConsoleKey.R: // Case för knapptryck R

                    DeleteProduct(product);

                    return;

                case ConsoleKey.Escape: // Case för knapptryck Escape

                    return;
            }

        }
    }

    private static Product GetProductBySku(string sku) //strint title = null tillåter oss att anropa metoden med eller utan title
    {
        // GET https://localhost:8000/products/{sku}

        HttpResponseMessage response;

        // GET https://localhost:8000/movies?title={title}
        response = httpClient.GetAsync($"products/{sku}").Result;

        var json = response.Content
            .ReadAsStringAsync()
            .Result;

        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };


        // Deserializing JSON to a single Product
        var product = JsonSerializer.Deserialize<Product>(json, serializeOptions);

        return product;
    }

    //Metoderna
    private static void RegisterProductView() // Registrera en ny produkt
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

                    RegisterProductView(); // Registrera produkten

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
}

