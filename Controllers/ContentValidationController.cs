using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class ContentValidationController : ControllerBase
{
    private readonly IMongoCollection<Vintellitour_Framework.Models.Post> _postsCollection;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _huggingFaceApiKey;

    public ContentValidationController(IMongoClient mongoClient, IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        var database = mongoClient.GetDatabase("Vintellitour");
        _postsCollection = database.GetCollection<Vintellitour_Framework.Models.Post>("posts");
        _huggingFaceApiKey = config["HUGGINGFACE_API_KEY"];
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("validate")]
    public async Task<IActionResult> ValidatePosts()
    {
        if (string.IsNullOrEmpty(_huggingFaceApiKey))
            return StatusCode(500, new { success = false, message = "Missing Hugging Face API Key" });

        var filterUnflagged = Builders<Vintellitour_Framework.Models.Post>.Filter.Ne(p => p.Status, "flagged");
        var posts = await _postsCollection.Find(filterUnflagged).ToListAsync();

        var invalidPosts = new List<object>();

        foreach (var post in posts)
        {
            var content = post.Content?.Trim();
            if (string.IsNullOrEmpty(content)) continue;

            var sensitiveCheck = CheckSensitiveWords(content);

            if (sensitiveCheck.Flagged)
            {
                invalidPosts.Add(new
                {
                    postId = post.Id,
                    content,
                    label = "violation",
                    score = 1.0,
                    reason = sensitiveCheck.Type,
                    source = "keyword"
                });
                continue;
            }

            var lang = DetectLanguage(content);
            if (lang == "eng")
            {
                var toxicCheck = await CheckToxicWithHuggingFace(content);
                if (toxicCheck.IsToxic)
                {
                    invalidPosts.Add(new
                    {
                        postId = post.Id,
                        content,
                        label = "toxic",
                        score = toxicCheck.Score,
                        source = "toxic_bert"
                    });
                    continue;
                }
            }
        }

        return Ok(new
        {
            success = true,
            invalidCount = invalidPosts.Count,
            invalidPosts
        });
    }


    private SensitiveCheckResult CheckSensitiveWords(string content)
    {
        var lowerContent = content.ToLowerInvariant();

        // Mở rộng danh sách từ khóa nhạy cảm
        var sensitiveWordsList = new Dictionary<string, List<string>>
        {
            ["hate_speech"] = new List<string>
            {
                "địt mẹ", "địt", "súc vật", "óc chó", "đồ ngu", "mất dạy", "chó đẻ",
                "con chó", "thằng ngu", "con điên", "đồ khốn", "ngu si", "ngu ngốc",
                "fuck", "shit", "damn", "bitch", "asshole", "stupid", "idiot"
            },
            ["sexual"] = new List<string>
            {
                "khiêu dâm", "gái gọi", "massage", "sex", "porn", "xxx", "địt",
                "chịch", "lồn", "cu", "cặc", "buồi", "vú", "nude"
            },
            ["spam"] = new List<string>
            {
                "click here", "buy now", "limited time", "make money", "earn cash",
                "kiếm tiền", "làm giàu", "bán hàng", "quảng cáo"
            }
        };

        foreach (var kvp in sensitiveWordsList)
        {
            foreach (var word in kvp.Value)
            {
                // Kiểm tra nhiều pattern khác nhau
                var patterns = new[]
                {
                    // Từ đứng độc lập
                    $@"\b{Regex.Escape(word)}\b",
                    // Từ có thể có ký tự đặc biệt xung quanh
                    $@"(^|[\s.,!?;:\-_]){Regex.Escape(word)}($|[\s.,!?;:\-_])",
                    // Từ viết liền (không dấu cách)
                    $@"{Regex.Escape(word)}"
                };

                foreach (var pattern in patterns)
                {
                    if (Regex.IsMatch(lowerContent, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                    {
                        return new SensitiveCheckResult
                        {
                            Flagged = true,
                            Type = kvp.Key,
                            Word = word
                        };
                    }
                }
            }
        }

        return new SensitiveCheckResult { Flagged = false };
    }

    private async Task<ToxicCheckResult> CheckToxicWithHuggingFace(string content)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _huggingFaceApiKey);
            client.Timeout = TimeSpan.FromSeconds(30); // Thêm timeout

            var requestBody = new { inputs = content.ToLowerInvariant() };
            var json = JsonSerializer.Serialize(requestBody);

            var response = await client.PostAsync(
                "https://api-inference.huggingface.co/models/unitary/toxic-bert",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Hugging Face API error: {response.StatusCode}");
                return new ToxicCheckResult { IsToxic = false };
            }

            var responseData = await response.Content.ReadAsStringAsync();

            // Parse response - có thể là array của arrays
            var results = JsonSerializer.Deserialize<JsonElement>(responseData);

            if (results.ValueKind == JsonValueKind.Array && results.GetArrayLength() > 0)
            {
                var firstResult = results[0];
                if (firstResult.ValueKind == JsonValueKind.Array)
                {
                    var labels = firstResult.EnumerateArray()
                        .Select(item => new ToxicLabel
                        {
                            Label = item.GetProperty("label").GetString(),
                            Score = item.GetProperty("score").GetDouble()
                        }).ToList();

                    var toxic = labels.FirstOrDefault(r => r.Label.ToLower().Contains("toxic") && r.Score > 0.7);

                    return new ToxicCheckResult
                    {
                        IsToxic = toxic != null,
                        Score = toxic?.Score ?? 0
                    };
                }
            }

            return new ToxicCheckResult { IsToxic = false };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking toxicity: {ex.Message}");
            return new ToxicCheckResult { IsToxic = false };
        }
    }

    private string DetectLanguage(string text)
    {
        // Cải thiện detection đơn giản
        var vietnameseChars = new[] { "à", "á", "ả", "ã", "ạ", "ă", "ằ", "ắ", "ẳ", "ẵ", "ặ",
                                     "â", "ầ", "ấ", "ẩ", "ẫ", "ậ", "đ", "è", "é", "ẻ", "ẽ", "ẹ",
                                     "ê", "ề", "ế", "ể", "ễ", "ệ", "ì", "í", "ỉ", "ĩ", "ị",
                                     "ò", "ó", "ỏ", "õ", "ọ", "ô", "ồ", "ố", "ổ", "ỗ", "ộ",
                                     "ơ", "ờ", "ớ", "ở", "ỡ", "ợ", "ù", "ú", "ủ", "ũ", "ụ",
                                     "ư", "ừ", "ứ", "ử", "ữ", "ự", "ỳ", "ý", "ỷ", "ỹ", "ỵ" };

        var vietnameseWords = new[] { "là", "của", "và", "có", "không", "được", "một", "này", "cho", "tôi", "bạn", "họ" };

        var lowerText = text.ToLowerInvariant();

        // Kiểm tra ký tự tiếng Việt
        if (vietnameseChars.Any(ch => lowerText.Contains(ch)))
            return "vie";

        // Kiểm tra từ tiếng Việt phổ biến
        if (vietnameseWords.Any(word => lowerText.Contains(word)))
            return "vie";

        return "eng";
    }

    // Helper classes
    private class SensitiveCheckResult
    {
        public bool Flagged { get; set; }
        public string Type { get; set; }
        public string Word { get; set; }
    }

    private class ToxicLabel
    {
        public string Label { get; set; }
        public double Score { get; set; }
    }

    private class ToxicCheckResult
    {
        public bool IsToxic { get; set; }
        public double Score { get; set; }
    }
}