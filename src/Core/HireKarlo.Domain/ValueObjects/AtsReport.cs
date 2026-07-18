namespace HireKarlo.Domain.ValueObjects;

public record AtsScore
{
    public int OverallScore { get; init; } // 0-100
    public int TitleMatchScore { get; init; }
    public int KeywordDensityScore { get; init; }
    public int SectionDetectionScore { get; init; }
    public int FormattingScore { get; init; }
    public int DateFormatScore { get; init; }

    public static AtsScore Create(
        int titleMatch,
        int keywordDensity,
        int sectionDetection,
        int formatting,
        int dateFormat)
    {
        var overall = (int)((titleMatch * 0.25) + (keywordDensity * 0.35) + 
                           (sectionDetection * 0.15) + (formatting * 0.15) + (dateFormat * 0.10));

        return new AtsScore
        {
            OverallScore = overall,
            TitleMatchScore = titleMatch,
            KeywordDensityScore = keywordDensity,
            SectionDetectionScore = sectionDetection,
            FormattingScore = formatting,
            DateFormatScore = dateFormat
        };
    }
}

public record AtsReport
{
    public AtsScore Score { get; init; } = null!;
    public List<string> MissingKeywords { get; init; } = new();
    public List<string> MatchingKeywords { get; init; } = new();
    public List<string> FormattingIssues { get; init; } = new();
    public List<string> MissingSections { get; init; } = new();
    public List<string> Recommendations { get; init; } = new();
    public TitleMatchResult TitleMatch { get; init; } = null!;
    public List<DateFormatIssue> DateIssues { get; init; } = new();
}

public record TitleMatchResult
{
    public string ResumeTitle { get; init; } = string.Empty;
    public string JobTitle { get; init; } = string.Empty;
    public double Similarity { get; init; }
    public bool IsExactMatch { get; init; }
    public bool IsFuzzyMatch { get; init; }
    public string? SuggestedTitle { get; init; }
}

public record DateFormatIssue
{
    public string OriginalText { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string Issue { get; init; } = string.Empty;
    public string? SuggestedFormat { get; init; }
}
