// Merge these actions into your existing controllers. Adjust names/attributes to match your API.
// Inject: private readonly BlobStorageService _blobs;  (and your DbContext)

static readonly string[] AllowedTypes = { "image/jpeg", "image/png", "image/webp" };

// ---- In EventsController ----
[HttpPost("{id}/banner")]
[Authorize(Roles = "Organiser")]
[Consumes("multipart/form-data")]
public async Task<IActionResult> UploadBanner(int id, IFormFile file)
{
    if (file == null || file.Length == 0) return BadRequest(new { message = "No file provided." });
    if (!AllowedTypes.Contains(file.ContentType)) return BadRequest(new { message = "Only JPG, PNG or WebP images are allowed." });
    if (file.Length > 5 * 1024 * 1024) return BadRequest(new { message = "Image must be smaller than 5 MB." });

    // TODO: load the event from your DbContext (404 if missing, 403 if the caller isn't its organiser)
    // var ev = await _db.Events.FindAsync(id);
    var url = await _blobs.UploadAsync(file, "banners");
    // TODO: ev.BannerUrl = url; await _db.SaveChangesAsync();
    return Ok(new { url });
}

// ---- In ProfileController ----
[HttpPost("picture")]
[Authorize]
[Consumes("multipart/form-data")]
public async Task<IActionResult> UploadPicture(IFormFile file)
{
    if (file == null || file.Length == 0) return BadRequest(new { message = "No file provided." });
    if (!AllowedTypes.Contains(file.ContentType)) return BadRequest(new { message = "Only JPG, PNG or WebP images are allowed." });
    if (file.Length > 5 * 1024 * 1024) return BadRequest(new { message = "Image must be smaller than 5 MB." });

    var url = await _blobs.UploadAsync(file, "profiles");
    // TODO: load current user from your DbContext, set ProfilePictureUrl = url, SaveChanges
    return Ok(new { url });
}
