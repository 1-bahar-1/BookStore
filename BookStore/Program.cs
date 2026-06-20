using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "book-details",
    pattern: "books/{slug}",
    defaults: new { controller = "Book", action = "Details" });

app.MapControllerRoute(
    name: "author-details",
    pattern: "authors/{slug}",
    defaults: new { controller = "Author", action = "Details" });

app.MapControllerRoute(
    name: "category-details",
    pattern: "categories/{slug}",
    defaults: new { controller = "Category", action = "Details" });

app.MapControllerRoute(
    name: "keyword-details",
    pattern: "keywords/{word}",
    defaults: new { controller = "Keyword", action = "Details" });

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await context.Database.MigrateAsync();

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    var adminEmail = "admin@test.com";
    var adminPassword = "Admin123!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
    else if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    if (!context.Categories.Any())
    {
        var categories = new List<Category>
        {
            new Category { Name = "رمان", Slug = "roman" },
            new Category { Name = "علمی", Slug = "elmi" },
            new Category { Name = "تاریخی", Slug = "tarikhi" },
            new Category { Name = "فلسفه", Slug = "falsafe" }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
    }

    if (!context.Authors.Any())
    {
        var authors = new List<Author>
        {
            new Author { FullName = "جلال آل احمد", Slug = "jalal-al-e-ahmad" },
            new Author { FullName = "سعید نفیسی", Slug = "said-nafisi" },
            new Author { FullName = "فروغ فرخزاد", Slug = "forugh-farrokhzad" },
            new Author { FullName = "نادر ابراهیمی", Slug = "nader-ebrahimi" }
        };

        context.Authors.AddRange(authors);
        await context.SaveChangesAsync();
    }

    if (!context.Keywords.Any())
    {
        var keywords = new List<Keyword>
        {
            new Keyword { Word = "ادبیات" },
            new Keyword { Word = "ایران" },
            new Keyword { Word = "فلسفه" },
            new Keyword { Word = "تاریخ" },
            new Keyword { Word = "شعر" }
        };

        context.Keywords.AddRange(keywords);
        await context.SaveChangesAsync();
    }

    if (!context.Books.Any())
    {
        var romanCat = await context.Categories.FirstAsync(c => c.Slug == "roman");
        var elmiCat = await context.Categories.FirstAsync(c => c.Slug == "elmi");
        var tarikhiCat = await context.Categories.FirstAsync(c => c.Slug == "tarikhi");
        var author1 = await context.Authors.FirstAsync(a => a.Slug == "jalal-al-e-ahmad");
        var author2 = await context.Authors.FirstAsync(a => a.Slug == "said-nafisi");
        var keyword1 = await context.Keywords.FirstAsync(k => k.Word == "ادبیات");
        var keyword2 = await context.Keywords.FirstAsync(k => k.Word == "ایران");

        var book1 = new Book
        {
            Title = "گrib Dust",
            Slug = "gharb-dast",
            PageCount = 240,
            CategoryId = romanCat.Id,
            FilePath = null
        };

        context.Books.Add(book1);
        await context.SaveChangesAsync();

        context.BookAuthors.Add(new BookAuthor { BookId = book1.Id, AuthorId = author1.Id });
        context.BookKeywords.Add(new BookKeyword { BookId = book1.Id, KeywordId = keyword1.Id });

        var book2 = new Book
        {
            Title = "چشم‌هایت",
            Slug = "chesh-mhayat",
            PageCount = 180,
            CategoryId = romanCat.Id,
            FilePath = null
        };

        context.Books.Add(book2);
        await context.SaveChangesAsync();

        context.BookAuthors.Add(new BookAuthor { BookId = book2.Id, AuthorId = author2.Id });
        context.BookKeywords.Add(new BookKeyword { BookId = book2.Id, KeywordId = keyword2.Id });

        context.BookRelations.Add(new BookRelation { BookId = book1.Id, RelatedBookId = book2.Id });

        await context.SaveChangesAsync();
    }
}

app.Run();
