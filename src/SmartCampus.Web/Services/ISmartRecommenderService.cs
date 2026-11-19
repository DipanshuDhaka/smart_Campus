namespace SmartCampus.Web.Services;

public class DepartmentRecommendationResult
{
    public int? DepartmentId { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int ConfidencePercentage { get; set; }
    public List<string> MatchedKeywords { get; set; } = new();
    public int SuggestedTechnicians { get; set; } = 1;
    public string RecommendationReason { get; set; } = string.Empty;
}

public interface ISmartRecommenderService
{
    Task<DepartmentRecommendationResult> RecommendDepartmentAsync(string title, string description, string? categoryName);
}
