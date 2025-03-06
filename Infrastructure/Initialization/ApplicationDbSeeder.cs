using Application.Service;
using Domain.Common.Const;
using Domain.Entities;
using Domain.Identity;
using Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Initialization;

internal class ApplicationDbSeeder
{
    private readonly ILogger<ApplicationDbSeeder> _logger;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly CustomSeederRunner _seederRunner;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly string _pepper;
    private readonly IUnitOfWork _unitOfWork;


    public ApplicationDbSeeder(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        CustomSeederRunner seederRunner,
        ILogger<ApplicationDbSeeder> logger,
        IConfiguration configuration,
        IUnitOfWork unitOfWork)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _seederRunner = seederRunner;
        _logger = logger;
        _configuration = configuration;
        _pepper = _configuration["SuperSecretPepper"]!;
        _unitOfWork = unitOfWork;
    }

    public async Task SeedDatabaseAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        await SeedRolesAsync(dbContext);
        await SeedUsersAsync(cancellationToken);
        await SeedAddOnMasterAsync(cancellationToken);
        await _seederRunner.RunSeedersAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(ApplicationDbContext dbContext)
    {

        foreach (var roleName in Roles.StaticRoles)
        {
            if (await _roleManager.Roles.SingleOrDefaultAsync(r => r.Name == roleName) is not { } role)
            {
                // Create the role
                _logger.LogInformation("Seeding {Role}", roleName);
                role = new ApplicationRole(roleName, roleName);
                await _roleManager.CreateAsync(role);
            }
        }
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        var usersToSeed = new List<(string Email, string FirstName, string LastName, string Role)>
                            {
                                ("pskrrw2@gmail.com", "Admin", "Sofi", Roles.Admin.Name),
                                //("executive@thewoundpros.com", "Executive", "Doe", Roles.Executive.Name)
                                ("tikatinnon@gmail.com", "Tika", "Tinnon", Roles.Admin.Name)
                            };

        foreach (var (email, firstName, lastName, role) in usersToSeed)
        {
            if (await _userManager.Users.AnyAsync(u => u.Email == email, cancellationToken))
                continue;

            var user = new ApplicationUser
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                UserName = email,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                NormalizedEmail = email.ToUpperInvariant(),
                NormalizedUserName = email.ToUpperInvariant(),
                IsActive = true
            };

            _logger.LogInformation("Seeding user: {Email}", email);

            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var pepperedPassword = Constants.Sha512(Constants.DefaultPassword) + _pepper;
            user.PasswordHash = passwordHasher.HashPassword(user, pepperedPassword);

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to create user {Email}: {Errors}", email, result.Errors);
                continue;
            }

            // Assign the user to the role
            if (!await _userManager.IsInRoleAsync(user, role))
            {
                _logger.LogInformation("Assigning '{Role}' role to user {Email}.", role, email);
                await _userManager.AddToRoleAsync(user, role);
            }
        }
    }

    public async Task SeedAddOnMasterAsync(CancellationToken cancellationToken)
    {
        var existingAddOn = await _unitOfWork.GenericRepository<AddOnMaster>().GetAllAsync();

        if (existingAddOn.ToList().Count() == 0)
        {
            var addOnItems = new List<AddOnMaster>
            {
                new AddOnMaster { Id = 1, Menu = "A La Carte Items", CategoryName = "Appetizers and Snacks", FoodType = "(VG)(GF)", FoodItem = "Smartfood Popcorn", Price = "35", Quantity = 0 },
                new AddOnMaster { Id = 2, Menu = "A La Carte Items", CategoryName = "Appetizers and Snacks", FoodType = "(V)(GF)", FoodItem = "Mellisa's Seasonal California Fruit Platter", Price = "125", Quantity = 0 },
                new AddOnMaster { Id = 3, Menu = "A La Carte Items", CategoryName = "Appetizers and Snacks", FoodType = "(VG)", FoodItem = "Pickle Platter", Price = "115", Quantity = 0 },
                new AddOnMaster { Id = 4, Menu = "A La Carte Items", CategoryName = "Appetizers and Snacks", FoodType = "(V)(GF)", FoodItem = "House-Made Guacamole", Price = "110", Quantity = 0 },
                new AddOnMaster { Id = 5, Menu = "A La Carte Items", CategoryName = "Appetizers and Snacks", FoodItem = "House Salad", Price = "95", Quantity = 0 },
                new AddOnMaster { Id = 6, Menu = "A La Carte Items", CategoryName = "Appetizers and Snacks", FoodItem = "Chips & Salsa", Price = "95", Quantity = 0 },
                new AddOnMaster { Id = 7, Menu = "A La Carte Items", CategoryName = "Appetizers and Snacks", FoodType = "(V)", FoodItem = "Roasted Heirloom Carrot Salad", Price = "95", Quantity = 0 },
                new AddOnMaster { Id = 8, Menu = "A La Carte Items", CategoryName = "Appetizers and Snacks", FoodType = "(V)", FoodItem = "Mellisa's Seasonal Crudite Platter", Price = "95", Quantity = 0 },
                new AddOnMaster { Id = 9, Menu = "A La Carte Items", CategoryName = "Appetizers and Snacks", FoodItem = "Jalapeno Cheddar Pretzel Knots", Price = "195", Quantity = 0 },
                new AddOnMaster { Id = 10, Menu = "A La Carte Items", CategoryName = "Appetizers and Snacks", FoodItem = "California Cheeses & Charcuterie", Price = "175", Quantity = 0 },
                new AddOnMaster { Id = 11, Menu = "A La Carte Items", CategoryName = "Appetizers and Snacks", FoodItem = "Sushi", Price = "475", Quantity = 0 },
                new AddOnMaster { Id = 12, Menu = "A La Carte Items", CategoryName = "Appetizers and Snacks", FoodItem = "Italian Sandwich", Price = "175", Quantity = 0 },
                new AddOnMaster { Id = 13, Menu = "A La Carte Items", CategoryName = "Appetizers and Snacks", FoodItem = "Caviar", Price = "500", Quantity = 0 },
                new AddOnMaster { Id = 14, Menu = "A La Carte Items", CategoryName = "Entrees", FoodType = "(VG)", FoodItem = "Mozzarella Pizza", Price = "90", Quantity = 0 },
                new AddOnMaster { Id = 15, Menu = "A La Carte Items", CategoryName = "Entrees", FoodItem = "Pepperoni Pizza", Price = "105", Quantity = 0 },
                new AddOnMaster { Id = 16, Menu = "A La Carte Items", CategoryName = "Entrees", FoodItem = "City Link Hot Dogs", Price = "150", Quantity = 0 },
                new AddOnMaster { Id = 17, Menu = "A La Carte Items", CategoryName = "Entrees", FoodItem = "Smoked Jalapeno & Cheddar Bratwurst", Price = "165", Quantity = 0 },
                new AddOnMaster { Id = 18, Menu = "A La Carte Items", CategoryName = "Entrees", FoodItem = "Cheeseburger Sliders", Price = "195", Quantity = 0 },
                new AddOnMaster { Id = 19, Menu = "A La Carte Items", CategoryName = "Entrees", FoodType = "(GF)", FoodItem = "Wings", Price = "150", Quantity = 0 },
                new AddOnMaster { Id = 20, Menu = "A La Carte Items", CategoryName = "Entrees", FoodItem = "Smoked House Platter", Price = "395", Quantity = 0 },
                new AddOnMaster { Id = 21, Menu = "A La Carte Items", CategoryName = "Entrees", FoodItem = "Chicken Tenders", Price = "175", Quantity = 0 },
                new AddOnMaster { Id = 22, Menu = "A La Carte Items", CategoryName = "Entrees", FoodItem = "Cochinita Pibil Tacos", Price = "175", Quantity = 0 },
                new AddOnMaster { Id = 23, Menu = "A La Carte Items", CategoryName = "Entrees", FoodType = "(GF)", FoodItem = "Pastrami Chili Cheese Fries", Price = "165", Quantity = 0 },
                new AddOnMaster { Id = 24, Menu = "A La Carte Items", CategoryName = "Desserts", FoodType = "(VG)", FoodItem = "Jumbo Cupcake Platter", Price = "125", Quantity = 0 },
                new AddOnMaster { Id = 25, Menu = "A La Carte Items", CategoryName = "Desserts", FoodType = "(GF)", FoodItem = "Gelato Festival", Price = "110", Quantity = 0 },
                new AddOnMaster { Id = 26, Menu = "A La Carte Items", CategoryName = "Desserts", FoodItem = "Specialty Cakes", Price = "110", Quantity = 0 },
                new AddOnMaster { Id = 27, Menu = "A La Carte Items", CategoryName = "Desserts", FoodType = "(VG)", FoodItem = "Lemon Meringue Tart", Price = "90", Quantity = 0 },
                new AddOnMaster { Id = 28, Menu = "A La Carte Items", CategoryName = "Vegan Selections", FoodType = "(V)", FoodItem = "Vegan Sliders", Price = "195", Quantity = 0 },
                new AddOnMaster { Id = 29, Menu = "A La Carte Items", CategoryName = "Vegan Selections", FoodType = "(V)", FoodItem = "Grilled Vegetable Wrap", Price = "75", Quantity = 0 },
                new AddOnMaster { Id = 30, Menu = "A La Carte Items", CategoryName = "Vegan Selections", FoodType = "(V)", FoodItem = "Vegan Nuggets", Price = "165", Quantity = 0 },
                new AddOnMaster { Id = 31, Menu = "A La Carte Items", CategoryName = "Vegan Selections", FoodType = "(V)", FoodItem = "Plant Based Hot Dogs", Price = "165", Quantity = 0 },
                new AddOnMaster { Id = 32, Menu = "A La Carte Items", CategoryName = "Vegan Selections", FoodType = "(V)", FoodItem = "Spicy Cauliflower Bites", Price = "95", Quantity = 0 },
                new AddOnMaster { Id = 33, Menu = "A La Carte Items", CategoryName = "Vegan Selections", FoodType = "(V)", FoodItem = "Beyond Bratwurst", Price = "175", Quantity = 0 },
                new AddOnMaster { Id = 34, Menu = "A La Carte Items", CategoryName = "Vegan Selections", FoodType = "(V)", FoodItem = "Mushroom Pibil Tostada", Price = "195", Quantity = 0 },
                new AddOnMaster { Id = 35, Menu = "A La Carte Items", CategoryName = "Vegan Desserts", FoodType = "(V)", FoodItem = "Vegan Chocolate Tart", Price = "85", Quantity = 0 },
                new AddOnMaster { Id = 36, Menu = "A La Carte Items", CategoryName = "Vegan Desserts", FoodType = "(V)", FoodItem = "Craig's Vegan Ice Cream", Price = "110", Quantity = 0 },
                new AddOnMaster { Id = 37, Menu = "Beverages", CategoryName = "Non-Alcoholic Beverages", FoodItem = "Pure Leaf Iced Tea Unsweetened", Price = "34", Quantity = 6 },
                new AddOnMaster { Id = 38, Menu = "Beverages", CategoryName = "Non-Alcoholic Beverages", FoodItem = "Pure Leaf Iced Tea Sweet with Lemon", Price = "34", Quantity = 6 },
                new AddOnMaster { Id = 39, Menu = "Beverages", CategoryName = "Non-Alcoholic Beverages", FoodItem = "Tropicana Pure Premium Lemonade", Price = "34", Quantity = 6 },
                new AddOnMaster { Id = 40, Menu = "Beverages", CategoryName = "Non-Alcoholic Beverages", FoodItem = "Bubly Lime Sparkling Water", Price = "23", Quantity = 6 }
            };

            foreach (var item in addOnItems)
            {
                var existingItem = await _unitOfWork?.GenericRepository<AddOnMaster>()?.GetByIdAsync(item.Id)!;
                if (existingItem == null)
                {
                    _unitOfWork?.GenericRepository<AddOnMaster>().AddAsync(item);
                    _logger.LogInformation("Seeding AddOnMaster item: {FoodItem}", item.FoodItem);
                }
            }

            await _unitOfWork.SaveAsync();
        }
    }
}
