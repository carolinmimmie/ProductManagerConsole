using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using ProductManager.Domain;
using static System.Console;
using System.Security.Claims;
using System.Net;


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

        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJnaXZlbl9uYW1lIjoiUGVuIiwiZmFtaWx5X25hbWUiOiJEb2UiLCJuYmYiOjE2OTgzMTkzNDAsImV4cCI6MTY5ODMyMjk0MCwiaWF0IjoxNjk4MzE5MzQwfQ.UqQCcb7QZNGUPLyB3wz6QTt9EU4tjdDDRhc-voZb_1c";

        httpClient.DefaultRequestHeaders.Authorization
            = new AuthenticationHeaderValue("Bearer", token);

        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);

        var firstName = jwtSecurityToken.Claims.First(Claim => Claim.Type == "given_name"); // få ut förnamn

        var lastName = jwtSecurityToken.Claims.First(Claim => Claim.Type == "family_name"); // få ut efternamn

        WriteLine();
        WriteLine($"Välkommen {firstName.Value} {lastName.Value}");// skriver ut förnamn och efternamn
        WriteLine();


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

        try
        {
            var product = GetProductBySku(sku);

            if (product != null)
            {
                ShowProductBySku(product);

                // Move the while loop outside the try block
                while (true)
                {
                    var keyPressedDeleteOrUpdateProduct = ReadKey(true); // Väntar in knapptryck

                    switch (keyPressedDeleteOrUpdateProduct.Key) // Hantera knapptryck
                    {
                        case ConsoleKey.R: // Case för knapptryck R
                            DeleteProductView(product);
                            return;

                        case ConsoleKey.Escape: // Case för knapptryck Escape
                            return;

                        case ConsoleKey.U: // Case för knapptryck U
                            UpdateProduct(product);
                            return;
                    }
                }
            }
        }
        catch 
        {
            WriteLine("Produkt saknas");
            Thread.Sleep(2000);
        }
    }



    private static void ShowProductBySku(Product product)
    {
        WriteLine($"Namn: {product.Name}");
        WriteLine($"Sku: {product.Sku}");
        WriteLine($"Beskrivning: {product.Description}");
        WriteLine($"Bild (URL): {product.Image}");
        WriteLine($"Pris: {product.Price}");
        WriteLine("");
        WriteLine("");
        WriteLine("(R)adera" + "  " + "(U)ppdatera");
    }

    private static void UpdateProduct(Product product)
    {
        throw new NotImplementedException();
    }

    private static void DeleteProductView(Product product)
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

                    DeleteProduct(product.Sku);

                    WriteLine("produkt raderad");

                    Thread.Sleep(2000);

                    return;

                case ConsoleKey.N:

                    Clear();

                    ShowProductBySku(product);

                    break;
            }

            switch (keyPressedConfirmDeleteProduct.Key)

            {
                case ConsoleKey.R: // Case för knapptryck R

                    DeleteProductView(product);

                    return;

                case ConsoleKey.Escape: // Case för knapptryck Escape

                    return;
            }

        }
    }

    private static bool DeleteProduct(string sku)
    {
        // DELETE https://localhost:8000/ + "vehicles/{vehicleId}"
        // DELETE https://localhost:8000/vehicles/1
        var response = httpClient.DeleteAsync($"products/{sku}").Result;

        // Kontrollera om vi fick en 2xx statuskod tillbaka (kommer vara 204 No Content i så fall)
        return response.IsSuccessStatusCode;
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
                    SaveProduct(product);

                    // try
                    // {
                    //     SaveProduct(product); // Spara produkten

                    //     WriteLine("Produkt sparad");
                    // }
                    // catch
                    // {
                    //     WriteLine("Ogiltig information");
                    // }

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
        try
        {
            // Serialisera produktobjektet till JSON
            var json = JsonSerializer.Serialize(product);

            // Skapa en StringContent med JSON-datan
            var body = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            // Gör HTTP POST-anropet till "products" endpoint
            var response = httpClient.PostAsync("products", body).Result;

            // Kasta exception om statuskoden inte är inom 2xx-området
            response.EnsureSuccessStatusCode();

            // Om inget exception kastas, skriv ut att produkten sparades
            WriteLine("Produkt sparad");
        }
        catch (HttpRequestException ex)
        {
            // Fånga upp exception om HTTP-anropet misslyckas
            if (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                // Visa felmeddelandet i 2 sekunder
                WriteLine("Ogiltig information");
                Thread.Sleep(2000);
            }
            else
            {
                // Om annat fel, skriv ut felmeddelandet
                WriteLine($"Fel vid HTTP-anrop: {ex.Message}");
            }
        }
    }

}




// private static void SaveProduct(Product product)
// {
//     // TODO Skicka information om filmen till web API:et genom att skicka 
//     //      ett HTTP POST-anrop till https://localhost:8000/movies

//     // 1 - Serialisera movie-objekt till JSON ({ "title": "Aliens", "plot": "Lorem ipsum dolor", ... })
//     var json = JsonSerializer.Serialize(product);

//     var body = new StringContent(
//       json,
//       Encoding.UTF8,
//       // Beskriver formatet på data
//       "application/json");

//     // POST https://localhost:8000/movies
//     var response = httpClient.PostAsync("products", body).Result;

//     // Om statuskod är "400 Bad Request", kasta exception som du sedan fångar
//     // där SaveMovie() anropas, och då visar meddelandet "Ogiltig information" i 2 sekunder.

//     // Kasta exception om statuskoden inte ligger inom 2xx-omfånget.
//     response.EnsureSuccessStatusCode();
// }



