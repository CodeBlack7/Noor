using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Noor.Core.Dtos;

namespace Noor.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/uploads")]
[Authorize(Roles = "Admin")]
public class AdminUploadsController : ControllerBase
{
    private static readonly Dictionary<string, string[]> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = new[] { "image/jpeg" },
        [".jpeg"] = new[] { "image/jpeg" },
        [".png"] = new[] { "image/png" },
        [".webp"] = new[] { "image/webp" }
    };

    private readonly IWebHostEnvironment _environment;

    public AdminUploadsController(IWebHostEnvironment environment) => _environment = environment;

    [HttpPost]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<UploadResponse>> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Choose an image to upload." });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedTypes.TryGetValue(extension, out var allowedContentTypes) ||
            !allowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Please upload a JPG, PNG, or WebP image." });
        }

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "Images must be 5MB or smaller." });

        await using (var stream = file.OpenReadStream())
        {
            var header = new byte[12];
            var read = await stream.ReadAsync(header);
            if (!IsExpectedImage(extension, header, read))
            {
                return BadRequest(new { message = "The file does not look like a valid image." });
            }
        }

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var uploads = Path.Combine(webRoot, "uploads");
        Directory.CreateDirectory(uploads);

        var safeName = $"{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}{extension}";
        var destination = Path.Combine(uploads, safeName);
        await using (var output = System.IO.File.Create(destination))
        {
            await file.CopyToAsync(output);
        }

        return Ok(new UploadResponse
        {
            Url = $"/uploads/{safeName}",
            FileName = safeName
        });
    }

    private static bool IsExpectedImage(string extension, byte[] header, int read) => extension switch
    {
        ".png" => read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47,
        ".jpg" or ".jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
        ".webp" => read >= 12 &&
                   header[0] == (byte)'R' && header[1] == (byte)'I' && header[2] == (byte)'F' && header[3] == (byte)'F' &&
                   header[8] == (byte)'W' && header[9] == (byte)'E' && header[10] == (byte)'B' && header[11] == (byte)'P',
        _ => false
    };
}
