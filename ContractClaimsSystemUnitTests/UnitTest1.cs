
using Moq;
using Microsoft.AspNetCore.Mvc;
using ContractClaimSystem.Controllers;
using ContractClaimSystem.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;

namespace ContractClaimSystemUnitTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task SubmitClaim_SavesClaimToDatabase()
    {
        // Arrange
        var context = GetDbContext();
        var fileServiceMock = new Mock<IFileStorageService>();
        fileServiceMock.Setup(f => f.SaveFileAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>()))
                       .ReturnsAsync(("dummy.txt", "/uploads/dummy.txt"));

        var controller = new ClaimController(context, fileServiceMock.Object);

        var model = new ClaimSubmissionViewModel
        {
            HoursWorked = 10,
            HourlyRate = 100,
            Notes = "Test Claim",
            SupportingFiles = new List<Microsoft.AspNetCore.Http.IFormFile>()
        };

        // Act
        var result = await controller.Submit(model);

        // Assert
        var claim = context.Claims.FirstOrDefault();
        Assert.NotNull(claim);
        Assert.Equal(10, claim.HoursWorked);
        Assert.Equal(100, claim.HourlyRate);
        Assert.Equal("Test Claim", claim.AdditionalNotes);
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task ApproveClaim_ChangesStatusToApproved()
    {
        // Arrange
        var context = GetDbContext();
        var claim = new Claim { ClaimId = 1, Status = ClaimStatus.Pending };
        context.Claims.Add(claim);
        await context.SaveChangesAsync();

        var fileServiceMock = new Mock<IFileStorageService>();
        var controller = new ClaimController(context, fileServiceMock.Object);

        // Act
        var result = await controller.Approve(1);

        // Assert
        var updatedClaim = await context.Claims.FindAsync(1);
        Assert.Equal(ClaimStatus.Approved, updatedClaim.Status);
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task RejectClaim_ChangesStatusToRejected()
    {
        // Arrange
        var context = GetDbContext();
        var claim = new Claim { ClaimId = 1, Status = ClaimStatus.Pending };
        context.Claims.Add(claim);
        await context.SaveChangesAsync();

        var fileServiceMock = new Mock<IFileStorageService>();
        var controller = new ClaimController(context, fileServiceMock.Object);

        // Act
        var result = await controller.Reject(1);

        // Assert
        var updatedClaim = await context.Claims.FindAsync(1);
        Assert.Equal(ClaimStatus.Rejected, updatedClaim.Status);
        Assert.IsType<RedirectToActionResult>(result);
    }
}
