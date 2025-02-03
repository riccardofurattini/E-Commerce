using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class MagazzinoHttpClient
{
    private readonly HttpClient _httpClient;

    public MagazzinoHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://192.168.1.5:8080/");
    }

    public async Task<bool> CheckItemAvailability(Guid idArticolo, int q)
    {
        var requestBody = new
        {
            id = idArticolo,
            Quantita = q
        };

        try
        {
            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Invia la richiesta POST
            var response = await _httpClient.PostAsync("modificaQuantita/", content);
            Console.WriteLine($"Richiesta: {jsonContent}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Risposta ricevuta: {responseContent}");

                // Deserializza la risposta come oggetto
                var responseObject = JsonConvert.DeserializeObject<ResponseModel>(responseContent);

                // Ritorna true solo se il campo success è true
                return responseObject?.Success ?? false;
            }
            else
            {
                Console.WriteLine($"Errore nel servizio magazzino: {response.Content}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore durante la comunicazione con il magazzino: {ex.Message}");
            return false;
        }
    }

    // Modello per la risposta JSON del server
    public class ResponseModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
