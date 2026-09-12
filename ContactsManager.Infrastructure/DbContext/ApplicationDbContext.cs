using ContactsManager.Core.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Entities
{
  public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Person> Persons { get; set; }
    public virtual DbSet<Country> Countries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.Entity<Country>().ToTable("Countries");
      modelBuilder.Entity<Person>().ToTable("Persons");

      //Seed data for Countries table
      string countriesJson = File.ReadAllTextAsync("countries.json").Result;
      List<Country>? countries = JsonSerializer.Deserialize<List<Country>>(countriesJson);

      foreach (Country country in countries!)
      {
        modelBuilder.Entity<Country>().HasData(country);
      }

      //Seed data for Persons table
      string personsJson = File.ReadAllTextAsync("persons.json").Result;
      List<Person>? persons = JsonSerializer.Deserialize<List<Person>>(personsJson);

      foreach (Person person in persons!)
      {
        modelBuilder.Entity<Person>().HasData(person);
      }

      //Fluent API
      modelBuilder.Entity<Person>().Property(temp => temp.TIN)
          .HasColumnName("TIN")
          .HasColumnType("varchar(10)");

      modelBuilder.Entity<Person>().HasIndex(temp => temp.TIN)
          .IsUnique();

      modelBuilder.Entity<Person>()
          .ToTable(t => t.HasCheckConstraint("CHK_TIN", "LEN([TIN]) = 10"));

      //Table Relations
      //modelBuilder.Entity<Person>(entity =>
      //{
      //    entity.HasOne<Country>(c => c.Country)
      //    .WithMany(p => p.Persons)
      //    .HasForeignKey(p => p.CountryID);
      //});
    }

    public async Task<List<Person>> SP_GetAllPersons()
    {
      return await Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]").ToListAsync();
    }

    public async Task<int> SP_InsertPerson(Person person)
    {
      SqlParameter[] parameters =
      [
          new SqlParameter("@PersonID", person.PersonID),
                new SqlParameter("@PersonName", person.PersonName),
                new SqlParameter("@Email", person.Email),
                new SqlParameter("@DateOfBirth", person.DateOfBirth),
                new SqlParameter("@Gender", person.Gender),
                new SqlParameter("@CountryID", person.CountryID),
                new SqlParameter("@Address", person.Address),
                new SqlParameter("@ReceiveNewsLetters", person.ReceiveNewsLetters),
                new SqlParameter("@TIN", person.TIN)
      ];

      return await Database.ExecuteSqlRawAsync(
       "EXECUTE [dbo].[InsertPerson] @PersonID, @PersonName, @Email, @DateOfBirth, @Gender, @CountryID, @Address, @ReceiveNewsLetters, @TIN",
       parameters
      );
    }

    public async Task<int> SP_UpdatePerson(Person person)
    {
      SqlParameter[] parameters =
      [
          new SqlParameter("@PersonID", person.PersonID),
                new SqlParameter("@PersonName", person.PersonName),
                new SqlParameter("@Email", person.Email),
                new SqlParameter("@DateOfBirth", person.DateOfBirth),
                new SqlParameter("@Gender", person.Gender),
                new SqlParameter("@CountryID", person.CountryID),
                new SqlParameter("@Address", person.Address),
                new SqlParameter("@ReceiveNewsLetters", person.ReceiveNewsLetters)
      ];

      return await Database.ExecuteSqlRawAsync(
       "EXECUTE [dbo].[UpdatePerson] @PersonID, @PersonName, @Email, @DateOfBirth, @Gender, @CountryID, @Address, @ReceiveNewsLetters",
       parameters
      );
    }

    public async Task<int> SP_DeletePerson(Guid personID)
    {
      SqlParameter parameter = new SqlParameter("@PersonID", personID);
      return await Database.ExecuteSqlRawAsync(
       "EXECUTE [dbo].[DeletePerson] @PersonID",
       parameter
      );
    }

    public async Task<Person?> SP_GetPersonByID(Guid? personID)
    {
      SqlParameter parameter = new SqlParameter("@PersonID", personID);
      return await Persons.FromSqlRaw(
       "EXECUTE [dbo].[GetPersonByID] @PersonID",
       parameter
      ).ToAsyncEnumerable().SingleOrDefaultAsync();
    }

    public async Task<int> SP_PopulateTinColumn()
    {
      return await Database.ExecuteSqlRawAsync(
       "EXECUTE [dbo].[PopulateTinColumn]"
      );
    }
  }
}
