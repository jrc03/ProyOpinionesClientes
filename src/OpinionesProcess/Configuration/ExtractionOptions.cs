using System.ComponentModel.DataAnnotations;

namespace OpinionesProcess.Configuration;

public sealed class ExtractionOptions
{
    public const string SectionName = "Extraction";

    [Required(ErrorMessage = "DataFolder es obligatorio.")]
    public string DataFolder { get; set; } = "Data";

    [Required(ErrorMessage = "SurveyFile es obligatorio.")]
    public string SurveyFile { get; set; } = "surveys_part1.csv";

    [Required(ErrorMessage = "SocialApiUrl es obligatorio.")]
    [Url(ErrorMessage = "SocialApiUrl debe ser una URL válida.")]
    public string SocialApiUrl { get; set; } = string.Empty;

    [Range(1, 86400, ErrorMessage = "IntervalSeconds debe ser un entero positivo entre 1 y 86400.")]
    public int IntervalSeconds { get; set; } = 120;

    [Range(0, 3600, ErrorMessage = "InitialDelaySeconds debe ser mayor o igual a 0.")]
    public int InitialDelaySeconds { get; set; } = 3;
}
