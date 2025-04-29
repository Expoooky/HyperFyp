using HyperTyk.Controllers.Giveaway;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GiveawayService : IDisposable
{
    private readonly Timer _timer;
    private readonly string _jsonFilePath;

    public GiveawayService()
    {
        _jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Controllers", "Giveaway", "GiveawayDatabase.json");
        _timer = new Timer(CheckGiveaways, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }

    private void CheckGiveaways(object state)
    {
        var now = DateTime.UtcNow;
        GiveawayDatabase giveawayData;

        try
        {
            // Read the JSON file
            string json = System.IO.File.ReadAllText(_jsonFilePath);
            giveawayData = JsonConvert.DeserializeObject<GiveawayDatabase>(json);

            // Check for expired giveaways
            foreach (var giveaway in giveawayData.Giveaways.Where(g => !g.winners.Any() && DateTime.Parse(g.expired_on) <= now).ToList())
            {
                var winners = SelectWinners(giveaway);
                giveaway.winners.AddRange(winners);
            }

            // Write the updated data back to the JSON file
            string updatedJson = JsonConvert.SerializeObject(giveawayData, Formatting.Indented);
            System.IO.File.WriteAllText(_jsonFilePath, updatedJson);
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., logging)
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }

    private List<string> SelectWinners(Giveaway giveaway)
    {
        Random random = new Random();
        var winners = new List<string>();

        var participants = giveaway.participants.ToList();

        for (int i = 0; i < giveaway.winnerCount && participants.Count > 0; i++)
        {
            int winnerIndex = random.Next(participants.Count);
            winners.Add(participants[winnerIndex]);
            participants.RemoveAt(winnerIndex);
        }

        return winners;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}