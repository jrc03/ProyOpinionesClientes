namespace OpinionesETL.Transformers;

public static class RatingClassifier
{
    public static string Clasificar(int rating) => rating switch
    {
        >= 4 => "Positiva",
        3 => "Neutra",
        _ => "Negativa",
    };
}
