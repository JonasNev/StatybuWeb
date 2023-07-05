using Auth0.AspNetCore.Authentication;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using StatybuWeb.Services.Api;
using StatybuWeb.Services.Auth0;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IAzureService, AzureService>();
builder.Services.AddTransient<IAzureBlobStorageService, AzureBlobStorageService>();
builder.Services.AddTransient<IAzureKeyVaultService, AzureKeyVaultService>();
builder.Services.AddTransient<IAuth0Service,Auth0Service>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = "https://dev-4b6q5857i7jcmzkw.eu.auth0.com/";
    options.Audience = "https://statybuweb.azurewebsites.net";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        RoleClaimType = "https://dev-4b6q5857i7jcmzkw.eu.auth0.com/roles",// Replace with your actual claim type
    };
});
var credential = new DefaultAzureCredential();
var secretClient = new SecretClient(new Uri(StatybuWeb.Constants.AzureConstants.KeyVaultUrl), credential);

// Fetch secrets from Key Vault
var auth0DomainSecret = secretClient.GetSecretAsync(StatybuWeb.Constants.AzureConstants.KeyVaultSecretNames.Auth0Domain).Result.Value;
var auth0ClientIdSecret = secretClient.GetSecretAsync(StatybuWeb.Constants.AzureConstants.KeyVaultSecretNames.Auth0ClientId).Result.Value;
// Add Auth0 Auth, reading the app settings above
builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = auth0DomainSecret.Value;
    options.ClientId = auth0ClientIdSecret.Value;
});

var app = builder.Build();
app.UseResponseCompression();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
