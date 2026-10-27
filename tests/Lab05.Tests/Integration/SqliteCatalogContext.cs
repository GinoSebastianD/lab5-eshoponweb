using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.Infrastructure.Data;

namespace Lab05.Tests.Integration;

internal sealed class SqliteCatalogContext(DbContextOptions<CatalogContext> options)
    : CatalogContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Catalog HiLo sequences are specific to SQL Server and unrelated to Basket.
        // Keep the real entity mappings; adapt key generation only for SQLite tests.
        foreach (var entity in builder.Model.GetEntityTypes())
        foreach (var property in entity.GetProperties())
        {
            property.RemoveAnnotation("SqlServer:ValueGenerationStrategy");
            property.RemoveAnnotation("SqlServer:HiLoSequenceName");
            property.RemoveAnnotation("SqlServer:HiLoSequenceSchema");
        }
        foreach (var sequence in builder.Model.GetSequences().ToList())
            builder.Model.RemoveSequence(sequence.Name, sequence.Schema);
    }
}
