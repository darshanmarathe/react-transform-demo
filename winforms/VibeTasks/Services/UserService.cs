using Microsoft.EntityFrameworkCore;
using VibeTasks.Data;
using VibeTasks.Models;

namespace VibeTasks.Services;

public class UserService
{
    public async Task<List<User>> GetAllAsync()
    {
        using var db = new AppDbContext();
        return await db.Users.OrderBy(u => u.Name).ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var db = new AppDbContext();
        return await db.Users.FindAsync(id);
    }

    public async Task<User> CreateAsync(string name, string email)
    {
        using var db = new AppDbContext();
        var user = new User { Name = name, Email = email };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        using var db = new AppDbContext();
        db.Users.Update(user);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var db = new AppDbContext();
        var user = await db.Users.FindAsync(id);
        if (user != null)
        {
            db.Users.Remove(user);
            await db.SaveChangesAsync();
        }
    }
}
