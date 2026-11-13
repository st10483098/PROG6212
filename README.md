# Changes to add to your Part 2 API (Azure Blob Storage)

1. `dotnet add package Azure.Storage.Blobs`
2. Add to `appsettings.json` (use user-secrets / environment variables for the real value, never commit it):
   `"BlobStorage": { "ConnectionString": "<your connection string>", "Container": "raceday-images" }`
3. Copy `BlobStorageService.cs` into the API and register it in `Program.cs`:
   `builder.Services.AddSingleton<BlobStorageService>();`
4. Add the two upload endpoints from `UploadEndpoints.cs` to your existing controllers (merge, don't duplicate routes).
   Replace the `TODO` lines with your own DbContext / entity code that saves the returned URL.
