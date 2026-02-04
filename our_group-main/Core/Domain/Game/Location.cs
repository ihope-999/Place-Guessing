namespace our_group.Core.Domain.Game;

public class Location{
    private string _name;
    private string _country;
    //private string _city;
    private double _latitude;
    private double _longitude;
    private string _category;
    private int _difficulty;
    private List<Review> _reviews;
    private int _externalLocationId;
    
    public Location(){} // for EF core

    public int Id{ get; set; } // Ef core id

    public int ExternalLocationId{
        get => _externalLocationId;
        private set => _externalLocationId = value;
    }

    public string Name{
        get => _name;
        private set => _name = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Country{
        get => _country;
        private set => _country = value ?? throw new ArgumentNullException(nameof(value));
    }

    public double Latitude{
        get => _latitude;
        private set => _latitude = value;
    }

    public double Longitude{
        get => _longitude;
        private set => _longitude = value;
    }

    public string Category{
        get => _category;
        private set => _category = value ?? throw new ArgumentNullException(nameof(value));
    }

    public int Difficulty{
        get => _difficulty;
        private set => _difficulty = value;
    }

    public List<Review> Reviews{
        get => _reviews;
        set => _reviews = value ?? throw new ArgumentNullException(nameof(value));
    }

  /*  public string City
    {
        get => _city;
        private set => _city = value /*?? throw new ArgumentNullException(nameof(value));
    }
*/
    // Constructor which sets up the Location object
    public Location(LocationDto locationDto, List<Review> reviews){
        _externalLocationId = locationDto.Id;
        _name = locationDto.Name;
        _country = locationDto.Country;
     //   _city = locationDto.City;
        _latitude = locationDto.Latitude;
        _longitude = locationDto.Longitude;
        _category = locationDto.Category;
        _difficulty = locationDto.Difficulty;
        _reviews = reviews; 
    }

}