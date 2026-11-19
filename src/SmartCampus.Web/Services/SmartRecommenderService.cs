using Microsoft.EntityFrameworkCore;
using SmartCampus.Web.Data;

namespace SmartCampus.Web.Services;

public class SmartRecommenderService : ISmartRecommenderService
{
    private readonly ApplicationDbContext _context;

    // Trade keyword dictionaries with relative weights
    private static readonly Dictionary<string, List<string>> DepartmentKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ELEC"] = new() 
        { 
            "light", "lighting", "streetlight", "bulb", "tube", "wire", "wiring", "spark", "short circuit", 
            "electricity", "power", "mcb", "switch", "socket", "plug", "fan", "ceiling fan", "dark", 
            "blackout", "tripped", "flicker", "generator", "phase", "voltage"
        },
        ["PLUMB"] = new() 
        { 
            "leak", "leaking", "leakage", "water", "pipe", "tap", "faucet", "flush", "drain", "drainage", 
            "clog", "choked", "sewage", "sink", "washbasin", "restroom", "toilet", "washroom", "urinal", 
            "tank", "overflow", "valve", "low pressure"
        },
        ["CIVIL"] = new() 
        { 
            "door", "lock", "handle", "hinge", "window", "glass", "cracked", "wall", "plaster", "ceiling", 
            "floor", "tile", "bench", "desk", "chair", "board", "podium", "stairs", "railing", "pothole", 
            "masonry", "carpentry", "furniture"
        },
        ["HVAC"] = new() 
        { 
            "ac", "air conditioner", "cooling", "chiller", "compressor", "ventilation", "thermostat", 
            "duct", "fan coil", "not cooling", "warm air", "freon", "hvac", "remote"
        },
        ["ITNET"] = new() 
        { 
            "wifi", "internet", "lan", "ethernet", "router", "access point", "cctv", "camera", "cable", 
            "projector", "screen", "mic", "microphone", "amplifier", "sound", "smart board", "hdmi"
        },
        ["HOUSE"] = new() 
        { 
            "clean", "cleaning", "dirty", "trash", "garbage", "dustbin", "dust", "sweep", "mop", 
            "smell", "odor", "stain", "pest", "spider", "insect", "litter"
        },
        ["HORT"] = new() 
        { 
            "tree", "branch", "fallen branch", "grass", "lawn", "plant", "garden", "hedge", "trim", 
            "flower", "horticulture", "irrigation sprinkler"
        }
    };

    public SmartRecommenderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DepartmentRecommendationResult> RecommendDepartmentAsync(string title, string description, string? categoryName)
    {
        var textToAnalyze = $"{title} {description} {categoryName}".ToLowerInvariant();
        var departments = await _context.Departments.ToListAsync();

        var scores = new Dictionary<string, (int score, List<string> matchedWords)>();

        foreach (var (deptCode, keywords) in DepartmentKeywords)
        {
            int score = 0;
            var matched = new List<string>();

            foreach (var kw in keywords)
            {
                if (textToAnalyze.Contains(kw, StringComparison.OrdinalIgnoreCase))
                {
                    // Exact keyword presence gives score based on keyword length/specificity
                    int weight = kw.Contains(' ') ? 3 : 2;
                    score += weight;
                    if (!matched.Contains(kw))
                    {
                        matched.Add(kw);
                    }
                }
            }

            if (score > 0)
            {
                scores[deptCode] = (score, matched);
            }
        }

        // If no keywords matched, fallback to category name heuristic
        if (scores.Count == 0 && !string.IsNullOrWhiteSpace(categoryName))
        {
            var catLower = categoryName.ToLowerInvariant();
            if (catLower.Contains("elec")) scores["ELEC"] = (2, new() { "category-heuristic" });
            else if (catLower.Contains("plumb") || catLower.Contains("water")) scores["PLUMB"] = (2, new() { "category-heuristic" });
            else if (catLower.Contains("civil") || catLower.Contains("infra")) scores["CIVIL"] = (2, new() { "category-heuristic" });
            else if (catLower.Contains("hvac") || catLower.Contains("ac")) scores["HVAC"] = (2, new() { "category-heuristic" });
            else if (catLower.Contains("it") || catLower.Contains("network")) scores["ITNET"] = (2, new() { "category-heuristic" });
            else if (catLower.Contains("clean") || catLower.Contains("sanitat")) scores["HOUSE"] = (2, new() { "category-heuristic" });
            else if (catLower.Contains("garden") || catLower.Contains("hort")) scores["HORT"] = (2, new() { "category-heuristic" });
        }

        if (scores.Count == 0)
        {
            // Default to Civil / General Maintenance if completely ambiguous
            var defaultDept = departments.FirstOrDefault(d => d.DepartmentCode == "CIVIL") ?? departments.FirstOrDefault();
            return new DepartmentRecommendationResult
            {
                DepartmentId = defaultDept?.DepartmentId,
                DepartmentCode = defaultDept?.DepartmentCode ?? "CIVIL",
                DepartmentName = defaultDept?.DepartmentName ?? "Civil & Infrastructure",
                ConfidencePercentage = 40,
                MatchedKeywords = new List<string> { "general campus maintenance" },
                SuggestedTechnicians = 1,
                RecommendationReason = "Standard default trade assignment based on general maintenance policy."
            };
        }

        // Pick top scoring department
        var top = scores.OrderByDescending(s => s.Value.score).First();
        var bestDept = departments.FirstOrDefault(d => d.DepartmentCode == top.Key) 
                       ?? departments.FirstOrDefault();

        // Calculate confidence (capped at 98%)
        int confidence = Math.Min(98, 50 + (top.Value.score * 12));

        // Suggested technicians heuristic: heavy/outdoor or high complexity requires 2 technicians
        int suggestedTechs = 1;
        var textLow = textToAnalyze.ToLowerInvariant();
        if (textLow.Contains("streetlight") || textLow.Contains("transformer") || textLow.Contains("leakage") || 
            textLow.Contains("chiller") || textLow.Contains("tree") || textLow.Contains("fallen") || 
            textLow.Contains("heavy") || textLow.Contains("ceiling") || top.Value.score >= 5)
        {
            suggestedTechs = 2;
        }

        return new DepartmentRecommendationResult
        {
            DepartmentId = bestDept?.DepartmentId,
            DepartmentCode = bestDept?.DepartmentCode ?? top.Key,
            DepartmentName = bestDept?.DepartmentName ?? "Maintenance Department",
            ConfidencePercentage = confidence,
            MatchedKeywords = top.Value.matchedWords,
            SuggestedTechnicians = suggestedTechs,
            RecommendationReason = $"Matched {top.Value.matchedWords.Count} diagnostic keyword(s) indicating {bestDept?.DepartmentName} trade scope."
        };
    }
}
