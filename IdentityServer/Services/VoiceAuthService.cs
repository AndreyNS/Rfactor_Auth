using System.Text;

namespace IdentityServer.Services
{
    public static class VoiceAuthService
    {
        public static async Task<bool> VerifyVoiceAsync(string username, string voiceData)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:5003");

                var content = new StringContent($"{{ \"username\": \"{username}\", \"voiceData\": \"{voiceData}\" }}", Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/voiceauth/verify", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return result == "true";
                }
                else
                {
                    return false;
                }
            }
        }
    }

}
