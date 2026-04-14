using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
        
        // 1. Login to get token
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { Email = "p.startek@benefitsystems.pl", Password = "Admin123!" });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = loginResult.GetProperty("token").GetString();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        // 2. Fetch all sets to get a valid ID
        var setsResponse = await client.GetAsync("/api/sets?setTypes=Quiz");
        var setsResult = await setsResponse.Content.ReadFromJsonAsync<JsonElement>();
        var firstSetId = setsResult[0].GetProperty("id").GetString();
        
        Console.WriteLine($"Found Set ID: {firstSetId}");
        
        // 3. Get set details
        var setResp = await client.GetAsync($"/api/sets/{firstSetId}");
        var setString = await setResp.Content.ReadAsStringAsync();
        Console.WriteLine($"Set details: {setString}");
        
        // Parse the dynamic JSON and prepare update payload
        var setObj = JsonSerializer.Deserialize<JsonElement>(setString);
        
        // We will just send the same items back to see if it saves!
        var updatePayload = new {
            title = setObj.GetProperty("title").GetString() + " (Edytowane)",
            description = setObj.GetProperty("description").GetString(),
            type = setObj.GetProperty("type").GetString(),
            items = setObj.GetProperty("items")
        };
        
        // 4. Update the set
        var putResp = await client.PutAsJsonAsync($"/api/sets/{firstSetId}", updatePayload);
        var putString = await putResp.Content.ReadAsStringAsync();
        
        Console.WriteLine($"PUT Response Status: {putResp.StatusCode}");
        Console.WriteLine($"PUT Response Body: {putString}");
    }
}
