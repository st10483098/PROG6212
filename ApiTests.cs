using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Tests;

public class ApiTests : IClassFixture<ApiFactory>
{
    private const string OrgCode = "RACEDAY-ORG-2025";
    private const string Pwd = "Passw0rd!";
    private readonly ApiFactory _f;
    public ApiTests(ApiFactory f) => _f = f;

    private static string NewEmail() => $"{Guid.NewGuid():N}@test.com";

    // Each client keeps its own session cookie automatically (HandleCookies = true by default).
    private async Task<HttpClient> LoggedIn(string role)
    {
        var c = _f.CreateClient();
        var email = NewEmail();
        (await c.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test", lastName = "User", email, password = Pwd, role,
            organiserCode = role == "Organiser" ? OrgCode : null
        })).EnsureSuccessStatusCode();
        (await c.PostAsJsonAsync("/api/auth/login", new { email, password = Pwd })).EnsureSuccessStatusCode();
        return c;
    }

    private static async Task<int> IdOf(HttpResponseMessage r)
    {
        r.EnsureSuccessStatusCode();
        var j = await r.Content.ReadFromJsonAsync<JsonElement>();
        return j.GetProperty("id").GetInt32();
    }

    private static Task<HttpResponseMessage> CreateEvent(HttpClient c) =>
        c.PostAsJsonAsync("/api/events", new
        {
            name = "City 10K", description = "Fun run", date = DateTime.UtcNow.AddDays(30),
            location = "Durban", distanceKm = 10, eventType = "Run"
        });

    // ---------- Registration & login ----------
    [Fact]
    public async Task Register_Participant_Succeeds()
    {
        var c = _f.CreateClient();
        var r = await c.PostAsJsonAsync("/api/auth/register",
            new { firstName = "A", lastName = "B", email = NewEmail(), password = Pwd, role = "Participant" });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var c = _f.CreateClient();
        var body = new { firstName = "A", lastName = "B", email = NewEmail(), password = Pwd, role = "Participant" };
        await c.PostAsJsonAsync("/api/auth/register", body);
        var r = await c.PostAsJsonAsync("/api/auth/register", body);
        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
    }

    [Fact]
    public async Task Register_Organiser_WithoutCode_IsForbidden()
    {
        var c = _f.CreateClient();
        var r = await c.PostAsJsonAsync("/api/auth/register",
            new { firstName = "A", lastName = "B", email = NewEmail(), password = Pwd, role = "Organiser" });
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task Register_ShortPassword_ReturnsBadRequest()
    {
        var c = _f.CreateClient();
        var r = await c.PostAsJsonAsync("/api/auth/register",
            new { firstName = "A", lastName = "B", email = NewEmail(), password = "123", role = "Participant" });
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var c = _f.CreateClient();
        var email = NewEmail();
        await c.PostAsJsonAsync("/api/auth/register",
            new { firstName = "A", lastName = "B", email, password = Pwd, role = "Participant" });
        var r = await c.PostAsJsonAsync("/api/auth/login", new { email, password = "WrongPass1" });
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_AllowsProfileAccess()
    {
        var c = await LoggedIn("Participant");
        var r = await c.GetAsync("/api/profile");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }

    // ---------- Authentication ----------
    [Fact]
    public async Task Unauthenticated_Request_ReturnsUnauthorized()
    {
        var r = await _f.CreateClient().GetAsync("/api/events");
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task Logout_EndsSession()
    {
        var c = await LoggedIn("Participant");
        await c.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.GetAsync("/api/profile")).StatusCode);
    }

    // ---------- Event management ----------
    [Fact]
    public async Task Organiser_CanCreateUpdateAndDeleteEvent()
    {
        var org = await LoggedIn("Organiser");
        var create = await CreateEvent(org);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var id = await IdOf(create);

        var upd = await org.PutAsJsonAsync($"/api/events/{id}", new
        {
            name = "Renamed", description = "d", date = DateTime.UtcNow.AddDays(40),
            location = "Cape Town", distanceKm = 21, eventType = "Cycle"
        });
        Assert.Equal(HttpStatusCode.OK, upd.StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await org.DeleteAsync($"/api/events/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await org.GetAsync($"/api/events/{id}")).StatusCode);
    }

    [Fact]
    public async Task Organiser_CreateEvent_InvalidData_ReturnsBadRequest()
    {
        var org = await LoggedIn("Organiser");
        var r = await org.PostAsJsonAsync("/api/events", new { name = "", description = "", location = "", distanceKm = 0 });
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotModifyAnotherOrganisersEvent()
    {
        var owner = await LoggedIn("Organiser");
        var other = await LoggedIn("Organiser");
        var id = await IdOf(await CreateEvent(owner));
        Assert.Equal(HttpStatusCode.Forbidden, (await other.DeleteAsync($"/api/events/{id}")).StatusCode);
    }

    [Fact]
    public async Task Participant_CanViewEvents()
    {
        var org = await LoggedIn("Organiser");
        await CreateEvent(org);
        var p = await LoggedIn("Participant");
        Assert.Equal(HttpStatusCode.OK, (await p.GetAsync("/api/events")).StatusCode);
    }

    // ---------- Role enforcement ----------
    [Fact]
    public async Task Participant_CannotCreateEvent()
    {
        var p = await LoggedIn("Participant");
        Assert.Equal(HttpStatusCode.Forbidden, (await CreateEvent(p)).StatusCode);
    }

    [Fact]
    public async Task Participant_CannotViewEventEnrolments()
    {
        var org = await LoggedIn("Organiser");
        var id = await IdOf(await CreateEvent(org));
        var p = await LoggedIn("Participant");
        Assert.Equal(HttpStatusCode.Forbidden, (await p.GetAsync($"/api/events/{id}/enrolments")).StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotEnrol()
    {
        var org = await LoggedIn("Organiser");
        var r = await org.PostAsJsonAsync("/api/enrolments", new { eventId = 1, categoryId = 1 });
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCaptureResults()
    {
        var p = await LoggedIn("Participant");
        var r = await p.PostAsJsonAsync("/api/results", new { enrolmentId = 1, finishTime = "00:45:30", position = 1 });
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    // ---------- Enrolments ----------
    private async Task<(HttpClient org, HttpClient p, int eventId, int catId)> SetupEvent()
    {
        var org = await LoggedIn("Organiser");
        var eventId = await IdOf(await CreateEvent(org));
        var catId = await IdOf(await org.PostAsJsonAsync("/api/categories", new { eventId, name = "Senior" }));
        var p = await LoggedIn("Participant");
        return (org, p, eventId, catId);
    }

    [Fact]
    public async Task Participant_CanEnrol_AndOrganiserSeesIt()
    {
        var (org, p, eventId, catId) = await SetupEvent();
        var r = await p.PostAsJsonAsync("/api/enrolments", new { eventId, categoryId = catId });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);

        var list = await org.GetFromJsonAsync<JsonElement>($"/api/events/{eventId}/enrolments");
        Assert.Equal(1, list.GetArrayLength());
        Assert.Equal(catId, list[0].GetProperty("categoryId").GetInt32());
    }

    [Fact]
    public async Task Participant_CannotEnrolTwice()
    {
        var (_, p, eventId, catId) = await SetupEvent();
        await p.PostAsJsonAsync("/api/enrolments", new { eventId, categoryId = catId });
        var r = await p.PostAsJsonAsync("/api/enrolments", new { eventId, categoryId = catId });
        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
    }

    [Fact]
    public async Task Enrol_WithCategoryFromDifferentEvent_ReturnsBadRequest()
    {
        var (org, p, eventId, _) = await SetupEvent();
        var otherEvent = await IdOf(await CreateEvent(org));
        var otherCat = await IdOf(await org.PostAsJsonAsync("/api/categories", new { eventId = otherEvent, name = "Junior" }));
        var r = await p.PostAsJsonAsync("/api/enrolments", new { eventId, categoryId = otherCat });
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    // ---------- Results ----------
    [Fact]
    public async Task Organiser_CapturesResult_ParticipantViewsIt()
    {
        var (org, p, eventId, catId) = await SetupEvent();
        var enrolId = await IdOf(await p.PostAsJsonAsync("/api/enrolments", new { eventId, categoryId = catId }));

        var post = await org.PostAsJsonAsync("/api/results", new { enrolmentId = enrolId, finishTime = "00:45:30", position = 1 });
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var mine = await p.GetFromJsonAsync<JsonElement>("/api/results/mine");
        Assert.Equal(1, mine.GetArrayLength());
        Assert.Equal(1, mine[0].GetProperty("position").GetInt32());
    }
}
