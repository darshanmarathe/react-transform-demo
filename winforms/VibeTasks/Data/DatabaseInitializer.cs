using Microsoft.EntityFrameworkCore;

namespace VibeTasks.Data;

public static class DatabaseInitializer
{
    public static void Initialize()
    {
        using var db = new AppDbContext();
        db.Database.EnsureCreated();
    }
}
