namespace videogame_randomized_back.DTOs;

public class NoteRequest
{
    public string Note { get; set; } = string.Empty;
}

public class RatingRequest
{
    public int PersonalRating { get; set; }
}
