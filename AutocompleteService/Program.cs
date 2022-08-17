using Elastic.Clients.Elasticsearch;
using ElasticService;
using Nest;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable CORS
builder.Services.AddCors(c =>
{
    c.AddPolicy("default", options => options.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin());
});

var app = builder.Build();

app.UseCors("default");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/autocomplete", (string input) =>
{
    var settings = new ConnectionSettings(new Uri("http://elasticsearch:9200"))
                        .DefaultIndex("cities");

    var client = new ElasticClient(settings);

    var searchRequest = new Nest.SearchRequest<City>()
    {
        Size = 5,
        Query = new FunctionScoreQuery
        {
            Query = new MultiMatchQuery
            {
                Query = input,
                Type = TextQueryType.BoolPrefix,
                Fields = Nest.Infer.Field<City>(f => f.AsciiName, 5).And<City>(f => f.AlternateNames)
            },
            Functions = new List<IScoreFunction>
            {
                new FieldValueFactorFunction
                {
                    Field = Nest.Infer.Field<City>(c => c.Population),
                    Modifier = FieldValueFactorModifier.SquareRoot
                }
            },
            ScoreMode = FunctionScoreMode.Average
        }
    };

    var searchResponse = client.Search<City>(searchRequest);

    var cities = MapDto(searchResponse.Documents);

    return cities;
});

Coordinates ParseCoordinates(string coordinateString)
{
    var latlng = coordinateString.Split(',');

    Coordinates coordinates = new Coordinates
    {
        Latitude = Double.Parse(latlng[0], CultureInfo.InvariantCulture),
        Longitude = Double.Parse(latlng[1], CultureInfo.InvariantCulture)
    };

    return coordinates;
}

IEnumerable<CityDto> MapDto(IEnumerable<City> results)
{

    List<CityDto> cities = new();

    foreach (var city in results)
    {
        CityDto dto = new CityDto
        {
            GeonameID = city.GeonameID,
            AsciiName = city.AsciiName,
            CountryNameEN = city.CountryNameEN,
            Coordinates = ParseCoordinates(city.Coordinates)
            
        };

        cities.Add(dto);
    }

    return cities;
}

app.MapGet("/clusterhealth", () =>
{
    var settings = new ElasticsearchClientSettings(new Uri("http://elasticsearch:9200"))
                        .DefaultIndex("cities");

    var client = new ElasticsearchClient(settings);

    var health = client.Cluster.Health();

    return health;
});

app.Run();

