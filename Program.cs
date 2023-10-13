﻿using System.Net.Http.Headers;
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
        // Hämta strängen ifrån Thunder CLient

        httpClient.DefaultRequestHeaders.Authorization
            = new AuthenticationHeaderValue("Bearer", "tKE+pMd2rQAHBbOjXWTZqacLJRLqlrnTzZdmKRJEXLjtiGOnFY3w+vuUxPSgLdMFbbVXxPrFWNUd/yQyG5PsEg==");

        Title = "Product Manager";

        while (true)
        {

            //CursorVisible = false;

            WriteLine("1. Lista produkter");


            var keyPressed = ReadKey(true);

            Clear();

            switch (keyPressed.Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:

                    try
                    {
                        ListProductView();
                    }
                    catch
                    {
                        WriteLine("Du saknar behörighet att lista fordon");
                        Thread.Sleep(2000);
                    }

                    break;
            }

            Clear();


        }
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
