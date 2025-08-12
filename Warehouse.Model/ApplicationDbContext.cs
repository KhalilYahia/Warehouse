
using Warehouse.Data.SeedData;
using Warehouse.Domain.Entities;
using Warehouse.Model.Configuration;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Warehouse.Model
{
    public class ApplicationDbContext : DbContext /*IdentityDbContext*/
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Balance> Balances => Set<Balance>();
    public DbSet<InboundDocument> InboundDocuments => Set<InboundDocument>();
    public DbSet<InboundItem> InboundItems => Set<InboundItem>();
    public DbSet<OutboundDocument> OutboundDocuments => Set<OutboundDocument>();
    public DbSet<OutboundItem> OutboundItems => Set<OutboundItem>();
   

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
            MyDbContextSeed.SeedData(modelBuilder); // uncomment this to insert data
                                                    //modelBuilder.ApplyConfiguration(new PostConfiguration());


            modelBuilder.ApplyConfiguration(new ResourceConfiguration());
            modelBuilder.ApplyConfiguration(new UnitConfiguration());
            modelBuilder.ApplyConfiguration(new ClientConfiguration());
            modelBuilder.ApplyConfiguration(new BalanceConfiguration());
            modelBuilder.ApplyConfiguration(new InboundDocumentConfiguration());
            modelBuilder.ApplyConfiguration(new InboundItemConfiguration());
            modelBuilder.ApplyConfiguration(new OutboundDocumentConfiguration());
            modelBuilder.ApplyConfiguration(new OutboundItemConfiguration());

            
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // uncomment to start database logger 
            //var lf = new LoggerFactory();
            //lf.AddProvider(new MyLoggerProvider());
            //optionsBuilder.UseLoggerFactory(lf);

            //optionsBuilder.UseLazyLoadingProxies();
            base.OnConfiguring(optionsBuilder);
        }
    }
}
