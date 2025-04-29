using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HyperTyk.Controllers.Survey;
using Newtonsoft.Json;

public class OfferService
{
    private readonly string _apiUrl = "https://api.bitlabs.ai/v1/offers";
    private readonly string _apiKey = "IZPviy0fD8jl1ndnm33JyCpnxyFsVuIk";

    public async Task<List<Offer>> GetOffersAsync(string userId)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            var response = await client.GetAsync($"{_apiUrl}?user_id={userId}");

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var offerResponse = JsonConvert.DeserializeObject<OfferResponse>(responseBody);
                return offerResponse.Offers;
            }
            else
            {
                throw new HttpRequestException("Failed to retrieve offers");
            }
        }
    }
}