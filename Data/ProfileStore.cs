using System;
using System.IO;
using System.Text.Json;
using wada.Models;

namespace wada.Data
{
    /// <summary>Saves/loads FreelancerProfile as profile.json beside the database.</summary>
    public static class ProfileStore
    {
        private static readonly string FilePath =
            Path.Combine(Directory.GetCurrentDirectory(), "profile.json");

        public static FreelancerProfile Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<FreelancerProfile>(json) ?? new FreelancerProfile();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProfileStore.Load error: {ex.Message}");
            }
            return new FreelancerProfile();
        }

        public static void Save(FreelancerProfile profile)
        {
            try
            {
                var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProfileStore.Save error: {ex.Message}");
            }
        }
    }
}
