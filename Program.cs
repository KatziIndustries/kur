using System.Security.Cryptography;
using System.Text;

static string SHA256(string input)
{
    byte[] bytes = Encoding.UTF8.GetBytes(input);
    return "";    
}


var builder = WebApplication.CreateBuilder(args);

// Listen on port 5060.
builder.WebHost.ConfigureKestrel(options => {
    options.ListenAnyIP(5060);
});

var packageDatabase = new PackageDatabase(
    builder.Configuration.GetConnectionString("Postgres")!
);

var app = builder.Build();

// Homepage
app.MapGet("/", () => Results.Text(
    File.ReadAllText("Public/index.html"),
    "text/html"
));

// Loginpage
app.MapGet("/login", () => Results.Text(
    File.ReadAllText("Public/login.html"),
    "text/html"
));

// Registerpage
app.MapGet("/register", () => Results.Text(
    File.ReadAllText("Public/register.html"),
    "text/html"
));

// Login API
app.MapGet("/api/login", (string? username, string? password, HttpResponse response) =>
{
    if(username != null && password != null) {
        response.Cookies.Append("jwt","-",new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        return Results.Redirect("/");
    } else {
        return Results.Redirect("/login?error=-");
    }
});

app.MapPost("/api/packages/{identifier}/katzibuild", async (
    string identifier,
    IFormFile file) =>
{
    // Validate identifier
    identifier = identifier.ToLower();

    if (!System.Text.RegularExpressions.Regex.IsMatch(
        identifier,
        @"^[a-z0-9-]+$"))
    {
        return Results.BadRequest(new
        {
            error = "Invalid identifier. Only lowercase letters, numbers and '-' are allowed."
        });
    }

    // Make sure a file was actually uploaded
    if (file.Length == 0)
    {
        return Results.BadRequest(new
        {
            error = "No file uploaded."
        });
    }

    // Read the uploaded file as text
    using var reader = new StreamReader(file.OpenReadStream());
    var katzibuild = await reader.ReadToEndAsync();

    await packageDatabase.SetKatzibuild(identifier, katzibuild);

    return Results.Ok(new
    {
        identifier,
        message = "Package uploaded successfully."
    });
}).DisableAntiforgery();

// Package List
app.MapGet("/packages", async () =>
{
    var packages = await packageDatabase.GetPackages();

    string p = "<h1>Katzi Packages</h1>";

    foreach (string package in packages) {
        p += $"<a href='/package/{package}'>{package}</a></br>";
    }

    return Results.Text(p,"text/html");
});

app.MapGet("/package/{identifier}", async (string identifier) => 
{
    return Results.Text("idk","text/html");
});

// Katzibuild Files
app.MapGet("/package/{identifier}/katzibuild", async (string identifier) =>
{
    identifier = identifier.ToLower();

    var katzibuild = await packageDatabase.GetKatzibuild(identifier);

    if (katzibuild == null)
        return Results.NotFound();

    return Results.Text(katzibuild, "text/plain");
});

//Test
app.MapGet("/hello", (string? name) =>
{
    name ??= "world";

    return Results.Json(new
    {
        message = $"Hello, {name}!",
        time = DateTimeOffset.UtcNow
    });
});

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok"
}));

app.Run();
