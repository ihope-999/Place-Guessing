namespace our_group.Core.Domain.Game;

public class Review{
    private int _id;
    private int _starRating;
    private string _text;
    private string _authorName;
    
    public Review(){} // for ef core

    public int Id{
        get => _id;
        set => _id = value;
    }

    public int Stars{
        get => _starRating;
        set => _starRating = value;
    }

    public string Text{
        get => _text;
        set => _text = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string AuthorName{
        get => _authorName;
        set => _authorName = value ?? throw new ArgumentNullException(nameof(value));
    }

    public Review(int id, int starRating, string text, string authorName){
        _id = id;
        _starRating = starRating;
        _text = text;
        _authorName = authorName;
    }
}