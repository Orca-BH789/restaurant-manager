using System;
using System.Configuration;
using System.Threading.Tasks;
using Supabase;

public class SupabaseService
{
    private static Supabase.Client _supabaseClient;

    // Hàm khởi tạo Supabase, chỉ chạy 1 lần duy nhất
    public static async Task InitializeAsync()
    {
        if (_supabaseClient == null)
        {
            string supabaseUrl = ConfigurationManager.AppSettings["SupabaseUrl"];
            string supabaseKey = ConfigurationManager.AppSettings["SupabaseKey"];

            var options = new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = true
            };

            _supabaseClient = new Supabase.Client(supabaseUrl, supabaseKey, options);
            await _supabaseClient.InitializeAsync();
        }
    }

    // Hàm lấy client Supabase (đã khởi tạo)
    public static Supabase.Client GetClient()
    {
        if (_supabaseClient == null)
        {
            throw new InvalidOperationException("Supabase chưa được khởi tạo! Hãy gọi InitializeAsync() trước.");
        }
        return _supabaseClient;
    }
}
