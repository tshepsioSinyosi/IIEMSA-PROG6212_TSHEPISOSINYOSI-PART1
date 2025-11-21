using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContractClaimSystem.Models;
using Microsoft.AspNetCore.Identity;

public class LecturerService
{
    private readonly UserManager<User> _userManager;

    public LecturerService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    // Get all users in the Lecturer role
    public async Task<List<User>> GetLecturersAsync()
    {
        var users = _userManager.Users.ToList();
        var lecturers = new List<User>();

        foreach (var user in users)
        {
            if (await _userManager.IsInRoleAsync(user, "Lecturer"))
                lecturers.Add(user);
        }

        return lecturers;
    }

    // Update lecturer info
    public async Task UpdateLecturerInfoAsync(string userId, string email, string phone)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.Email = email;
            user.PhoneNumber = phone;
            await _userManager.UpdateAsync(user);
        }
    }
}
