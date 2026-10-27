using Microsoft.eShopWeb.ApplicationCore.Entities;

namespace Lab05.Tests;

internal static class EntityIdentity
{
    // Simulate database-assigned keys only in isolated unit-test fixtures.
    internal static T WithId<T>(this T entity, int id) where T : BaseEntity
    {
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(entity, id);
        return entity;
    }
}
