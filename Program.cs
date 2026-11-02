using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Win32;
using System.Windows.Forms;

namespace WebSelec;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

internal sealed class Site
{
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";
    public string Category { get; set; } = "Other";
    public string Icon { get; set; } = "🌐";
    [JsonIgnore] public bool Custom { get; set; }
}

internal sealed class AppData
{
    public List<Site> CustomSites { get; set; } = new();
    public List<string> Favorites { get; set; } = new();
    public List<Site> Recent { get; set; } = new();
    public string Appearance { get; set; } = "system";
}

internal sealed class ActivationRecord
{
    public string Key { get; set; } = "";
    public string Version { get; set; } = "1.0.0";
    public DateTime ActivatedAt { get; set; }
    public string Type { get; set; } = "DEMO ACTIVATION — NOT A DIGITAL SIGNATURE";
}

internal sealed class MainForm : Form
{
    private const string Version = "1.0.0";
    private static readonly string[] Categories = { "Social", "Video", "Technology", "Games", "Learning", "Music", "Productivity", "Shopping", "Other" };
    private static readonly string[] DemoKeys = {
        "WEBSELEC-100-7KPX-2MRA", "WEBSELEC-100-9VQD-4NWT",
        "WEBSELEC-100-3HJF-8ZLC", "WEBSELEC-100-6BYS-1GKE",
        "WEBSELEC-100-5RUM-7DPA", "WEBSELEC-100-2XCN-9LHF",
        "WEBSELEC-100-4TWB-6QJG", "WEBSELEC-100-8ZRA-3VKM"
    };
    private static readonly List<Site> DefaultSites = new()
    {
        new(){Name="Google",Url="https://www.google.com",Category="Productivity",Icon="🔎"},
        new(){Name="YouTube",Url="https://www.youtube.com",Category="Video",Icon="▶️"},
        new(){Name="Roblox",Url="https://www.roblox.com",Category="Games",Icon="🎮"},
        new(){Name="Apple",Url="https://www.apple.com",Category="Technology",Icon="🍎"},
        new(){Name="GitHub",Url="https://github.com",Category="Technology",Icon="🐙"},
        new(){Name="Wikipedia",Url="https://www.wikipedia.org",Category="Learning",Icon="📚"},
        new(){Name="Discord",Url="https://discord.com",Category="Social",Icon="💬"},
        new(){Name="ChatGPT",Url="https://chatgpt.com",Category="Technology",Icon="🤖"},
        new(){Name="Reddit",Url="https://www.reddit.com",Category="Social",Icon="🗨️"},
        new(){Name="Twitch",Url="https://www.twitch.tv",Category="Video",Icon="🟣"},
        new(){Name="Spotify",Url="https://open.spotify.com",Category="Music",Icon="🎵"},
        new(){Name="Microsoft",Url="https://www.microsoft.com",Category="Technology",Icon="🪟"},
        new(){Name="Gmail",Url="https://mail.google.com",Category="Productivity",Icon="✉️"},
        new(){Name="Stack Overflow",Url="https://stackoverflow.com",Category="Learning",Icon="🧩"},
        new(){Name="MDN Web Docs",Url="https://developer.mozilla.org",Category="Learning",Icon="📖"},
        new(){Name="Canva",Url="https://www.canva.com",Category="Productivity",Icon="🎨"},
        new(){Name="Steam",Url="https://store.steampowered.com",Category="Games",Icon="🕹️"},
        new(){Name="Epic Games",Url="https://store.epicgames.com",Category="Games",Icon="🎯"},
        new(){Name="Pinterest",Url="https://www.pinterest.com",Category="Social",Icon="📌"},
        new(){Name="Amazon",Url="https://www.amazon.com",Category="Shopping",Icon="📦"},
        new(){Name="DuckDuckGo",Url="https://duckduckgo.com",Category="Productivity",Icon="🦆"},
        new(){Name="Proton",Url="https://proton.me",Category="Productivity",Icon="🛡️"},
        new(){Name="NASA",Url="https://www.nasa.gov",Category="Learning",Icon="🚀"},
        new(){Name="Internet Archive",Url="https://archive.org",Category="Learning",Icon="🗄️"},
        new(){Name="Placeholder",Url="about:blank",Category="Other",Icon="🌀"}
    };

    private readonly string dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WebSelec");
    private string DataPath => Path.Combine(dataDir, "data.json");
    private string ActivationPath => Path.Combine(dataDir, "activation.json");
    private readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    private AppData data = new();
    private ActivationRecord? activation;
    private readonly Color forest = Color.FromArgb(32, 92, 59);
    private readonly Color forestLight = Color.FromArgb(220, 238, 225);
    private readonly Color lightBg = Color.FromArgb(243, 245, 241);
    private readonly Color darkBg = Color.FromArgb(16, 22, 18);
    private readonly Color darkPanel = Color.FromArgb(25, 35, 28);
    private readonly Color darkText = Color.FromArgb(233, 241, 233);
    private bool dark;
    private string currentTab = "main";
    private string currentFilter = "all";
    private string currentCategory = "all";
    private string? editingUrl;
    private Panel root = null!;
    private Panel content = null!;
    private Panel nav = null!;
    private TextBox searchBox = null!;
    private ComboBox categoryFilter = null!;
    private ComboBox appearanceBox = null!;
    private Label statusLabel = null!;
    private TextBox nameInput = null!;
    private TextBox urlInput = null!;
    private TextBox iconInput = null!;
    private ComboBox siteCategoryInput = null!;
    private Button saveSiteButton = null!;

    public MainForm()
    {
        Text = "WebSelec 1.0.0 — Red Forest";
        MinimumSize = new Size(900, 650);
        Size = new Size(1240, 820);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9.5f);
        data = LoadData();
        activation = LoadActivation();
        dark = data.Appearance == "dark" || (data.Appearance == "system" && SystemUsesDarkMode());
        if (activation == null) BuildActivation(); else BuildApplication();
    }

    private AppData LoadData()
    {
        try { Directory.CreateDirectory(dataDir); if (File.Exists(DataPath)) return JsonSerializer.Deserialize<AppData>(File.ReadAllText(DataPath), jsonOptions) ?? new AppData(); }
        catch { }
        return new AppData();
    }
    private void SaveData()
    {
        try { Directory.CreateDirectory(dataDir); File.WriteAllText(DataPath, JsonSerializer.Serialize(data, jsonOptions)); }
        catch (Exception ex) { MessageBox.Show("Could not save WebSelec data.\n" + ex.Message, "WebSelec", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
    private ActivationRecord? LoadActivation()
    {
        try { if (!File.Exists(ActivationPath)) return null; var a = JsonSerializer.Deserialize<ActivationRecord>(File.ReadAllText(ActivationPath), jsonOptions); return a != null && a.Version == Version && DemoKeys.Contains(a.Key) ? a : null; }
        catch { return null; }
    }

    private void BuildActivation()
    {
        Controls.Clear(); BackColor = lightBg;
        var outer = new Panel { Dock = DockStyle.Fill, BackColor = lightBg, Padding = new Padding(30) };
        var card = new Panel { Size = new Size(490, 440), BackColor = Color.White, Padding = new Padding(30) };
        outer.Controls.Add(card); Controls.Add(outer);
        outer.Resize += (_, _) => card.Location = new Point((outer.ClientSize.Width-card.Width)/2, (outer.ClientSize.Height-card.Height)/2);
        card.Location = new Point((outer.ClientSize.Width-card.Width)/2, (outer.ClientSize.Height-card.Height)/2);
        var brand = new Label { Text = "🌲  WebSelec", Font = new Font("Segoe UI", 23, FontStyle.Bold), AutoSize = true, ForeColor = forest, Location = new Point(28, 28) };
        card.Controls.Add(brand);
        card.Controls.Add(new Label { Text = "WEB SELECTOR  ·  RED FOREST  ·  VERSION 1.0.0", AutoSize = true, ForeColor = Color.DimGray, Location = new Point(32, 78) });
        card.Controls.Add(new Label { Text = "Welcome to Red Forest.", Font = new Font("Segoe UI", 21, FontStyle.Bold), AutoSize = true, Location = new Point(30, 135) });
        card.Controls.Add(new Label { Text = "Activate WebSelec to unlock your website dashboard and all its features.", AutoSize = false, Size = new Size(420, 45), Location = new Point(32, 180) });
        card.Controls.Add(new Label { Text = "Activation key", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true, Location = new Point(32, 240) });
        var keyBox = new TextBox { Location = new Point(32, 265), Width = 420, Height = 34, Font = new Font("Segoe UI", 11), PlaceholderText = "WEBSELEC-100-XXXX-XXXX" };
        card.Controls.Add(keyBox);
        var activate = MakeButton("Activate WebSelec  →", true); activate.Location = new Point(32, 315); activate.Size = new Size(205, 42);
        var generate = MakeButton("Generate demo key"); generate.Location = new Point(247, 315); generate.Size = new Size(205, 42);
        var message = new Label { Location = new Point(32, 365), Size = new Size(420, 28), ForeColor = Color.Firebrick };
        card.Controls.Add(activate); card.Controls.Add(generate); card.Controls.Add(message);
        card.Controls.Add(new Label { Text = "Demo activation only. These preset keys are not secure licensing or cryptographic release verification.", Location = new Point(32, 400), Size = new Size(420, 34), ForeColor = Color.DimGray });
        generate.Click += (_, _) => { keyBox.Text = DemoKeys[Random.Shared.Next(DemoKeys.Length)]; message.ForeColor = Color.DimGray; message.Text = "Demo key generated. Press Activate WebSelec to continue."; };
        activate.Click += (_, _) =>
        {
            var key = keyBox.Text.Trim().ToUpperInvariant();
            if (!DemoKeys.Contains(key)) { message.ForeColor = Color.Firebrick; message.Text = "Activation failed. That key does not match a preset key."; return; }
            activation = new ActivationRecord { Key = key, ActivatedAt = DateTime.Now };
            try { Directory.CreateDirectory(dataDir); File.WriteAllText(ActivationPath, JsonSerializer.Serialize(activation, jsonOptions)); }
            catch (Exception ex) { message.Text = "Could not save activation: " + ex.Message; return; }
            BuildApplication();
        };
    }

    private void BuildApplication()
    {
        Controls.Clear();
        root = new Panel { Dock = DockStyle.Fill, BackColor = CurrentBg };
        var top = new Panel { Dock = DockStyle.Top, Height = 66, BackColor = CurrentPanel, Padding = new Padding(24, 10, 22, 8) };
        var title = new Label { Text = "🌲  WebSelec", Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, ForeColor = CurrentText, Location = new Point(22, 15) };
        var subtitle = new Label { Text = "RED FOREST  ·  v1.0.0", AutoSize = true, ForeColor = Muted, Location = new Point(205, 24) };
        var themeButton = MakeButton(dark ? "☀" : "◐"); themeButton.Size = new Size(42, 38); themeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right; themeButton.Location = new Point(ClientSize.Width - 75, 13);
        themeButton.Click += (_, _) => { data.Appearance = dark ? "light" : "dark"; SaveData(); ApplyTheme(); BuildApplication(); };
        top.Controls.Add(title); top.Controls.Add(subtitle); top.Controls.Add(themeButton);
        top.Resize += (_, _) => themeButton.Left = top.ClientSize.Width - themeButton.Width - 18;
        nav = new Panel { Dock = DockStyle.Left, Width = 215, BackColor = CurrentPanel, Padding = new Padding(12, 20, 12, 12) };
        var navTitle = new Label { Text = "WORKSPACE", AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(14, 14) }; nav.Controls.Add(navTitle);
        AddNav("⌂   Home", "main", 45); AddNav("★   Favorites", "favorites", 91); AddNav("◷   Recent history", "history", 137);
        var section = new Label { Text = "WEBSELEC", AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(14, 205) }; nav.Controls.Add(section);
        AddNav("⚙   Settings", "settings", 230); AddNav("▤   Release archive", "signed", 276); AddNav("✧   Upcoming / Ideas", "ideas", 322);
        var navFooter = new Label { Text = "WebSelec 1.0.0\nRed Forest\nMade by Cat", AutoSize = true, ForeColor = Muted, Location = new Point(16, 390) }; nav.Controls.Add(navFooter);
        content = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = CurrentBg, Padding = new Padding(28, 24, 28, 28) };
        root.Controls.Add(content); root.Controls.Add(nav); root.Controls.Add(top); Controls.Add(root);
        ApplyTheme(); ShowPage("main");
    }

    private void AddNav(string text, string tab, int y)
    {
        var b = new Button { Text = text, Tag = tab, Location = new Point(5, y), Size = new Size(180, 40), FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9.5f), Cursor = Cursors.Hand };
        b.FlatAppearance.BorderSize = 0; b.Click += (_, _) => ShowPage(tab); nav.Controls.Add(b);
    }

    private void ShowPage(string tab)
    {
        currentTab = tab; content.Controls.Clear();
        switch (tab)
        {
            case "main": BuildHome(); break;
            case "favorites": BuildSitePage("★ Favorites", "Your bookmarked websites, all in one place.", GetFavorites()); break;
            case "history": BuildHistory(); break;
            case "settings": BuildSettings(); break;
            case "signed": BuildArchive(); break;
            case "ideas": BuildIdeas(); break;
        }
        foreach (Control c in nav.Controls) if (c is Button b) { bool active = (string)b.Tag! == tab; b.BackColor = active ? forestLight : CurrentPanel; b.ForeColor = active ? forest : CurrentText; }
    }

    private void BuildHome()
    {
        var hero = new Panel { Location = new Point(0, 0), Size = new Size(930, 175), BackColor = Color.FromArgb(21, 60, 40), Padding = new Padding(26) };
        AddLabel(hero, "THE FIRST MAJOR RELEASE", 26, 20, 12, Color.FromArgb(220, 238, 225), true);
        AddLabel(hero, "Find your corner of the internet.", 26, 48, 26, Color.White, true);
        AddLabel(hero, "Your websites, your favorites, your shortcuts. Everything in one place.", 28, 93, 11, Color.FromArgb(225, 238, 228));
        var addHero = MakeButton("＋ Add a website", true); addHero.Location = new Point(28, 125); addHero.Size = new Size(145, 34); addHero.Click += (_, _) => { nameInput?.Focus(); content.ScrollControlIntoView(nameInput); };
        var randomHero = MakeButton("↗ Surprise me"); randomHero.Location = new Point(182, 125); randomHero.Size = new Size(135, 34); randomHero.Click += (_, _) => OpenRandom(); hero.Controls.Add(addHero); hero.Controls.Add(randomHero); content.Controls.Add(hero);
        int total = AllSites().Count; int fav = data.Favorites.Count; int recent = data.Recent.Count;
        var stats = new FlowLayoutPanel { Location = new Point(0, 190), Size = new Size(930, 72), BackColor = Color.Transparent, WrapContents = false };
        stats.Controls.Add(StatCard("Available websites", total.ToString())); stats.Controls.Add(StatCard("Your favorites", fav.ToString())); stats.Controls.Add(StatCard("Recently visited", recent.ToString())); content.Controls.Add(stats);
        AddLabel(content, "Your websites", 0, 280, 22, CurrentText, true); AddLabel(content, "Search, discover and open your favorite places online.", 0, 313, 10, Muted);
        searchBox = new TextBox { Location = new Point(0, 345), Width = 360, Height = 32, Font = new Font("Segoe UI", 10), PlaceholderText = "Search websites or URLs…" }; searchBox.TextChanged += (_, _) => RenderHomeSites(); content.Controls.Add(searchBox);
        var random = MakeButton("🎲 Random"); random.Location = new Point(370, 343); random.Size = new Size(105, 34); random.Click += (_, _) => OpenRandom(); content.Controls.Add(random);
        var filter = new ComboBox { Location = new Point(485, 345), Width = 145, DropDownStyle = ComboBoxStyle.DropDownList }; filter.Items.AddRange(new object[] { "All websites", "★ Favorites", "Custom" }); filter.SelectedIndex = 0; filter.SelectedIndexChanged += (_, _) => { currentFilter = filter.SelectedIndex switch { 1 => "favorites", 2 => "custom", _ => "all" }; RenderHomeSites(); }; content.Controls.Add(filter);
        categoryFilter = new ComboBox { Location = new Point(640, 345), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList }; categoryFilter.Items.Add("All categories"); categoryFilter.Items.AddRange(Categories.Cast<object>().ToArray()); categoryFilter.SelectedIndex = 0; categoryFilter.SelectedIndexChanged += (_, _) => { currentCategory = categoryFilter.SelectedIndex == 0 ? "all" : categoryFilter.SelectedItem?.ToString() ?? "all"; RenderHomeSites(); }; content.Controls.Add(categoryFilter);
        var listHost = new Panel { Name = "siteList", Location = new Point(0, 390), Size = new Size(930, 330), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, AutoScroll = true, BackColor = Color.Transparent }; content.Controls.Add(listHost);
        BuildCustomForm(735);
        AddLabel(content, "Recently visited", 0, 945, 18, CurrentText, true);
        AddLabel(content, "Your five most recently opened websites.", 0, 974, 9, Muted);
        int recentY = 1005;
        if (data.Recent.Count == 0) AddLabel(content, "Nothing here yet. Open a website to get started!", 10, recentY, 9, Muted);
        foreach (var recentSite in data.Recent)
        {
            var recentCard = SiteCard(recentSite, 890); recentCard.Location = new Point(0, recentY); content.Controls.Add(recentCard); recentY += 96;
        }
        var joke = MakeButton("Hello this does nothing"); joke.Location = new Point(0, recentY + 18); joke.Size = new Size(190, 34);
        joke.Click += (_, _) => MessageBox.Show("Hello! This button still does nothing.", "WebSelec"); content.Controls.Add(joke);
        RenderHomeSites();
    }

    private void RenderHomeSites()
    {
        var host = content.Controls.Find("siteList", false).FirstOrDefault() as Panel; if (host == null) return;
        host.Controls.Clear();
        var sites = AllSites().Where(s => (currentFilter == "all" || (currentFilter == "favorites" && data.Favorites.Contains(s.Url)) || (currentFilter == "custom" && s.Custom)) && (currentCategory == "all" || s.Category == currentCategory));
        var q = searchBox?.Text.Trim() ?? ""; if (q.Length > 0) sites = sites.Where(s => (s.Name + " " + s.Url + " " + s.Category).Contains(q, StringComparison.OrdinalIgnoreCase));
        int y = 0; foreach (var site in sites) { var card = SiteCard(site, 890); card.Location = new Point(0, y); host.Controls.Add(card); y += card.Height + 8; }
        if (y == 0) AddLabel(host, "No websites found. Try a different search or category.", 15, 20, 11, Muted);
        host.AutoScrollMinSize = new Size(0, y);
    }

    private void BuildCustomForm(int y)
    {
        var panel = new Panel { Location = new Point(0, y), Size = new Size(930, 190), BackColor = CurrentPanel, Padding = new Padding(18) }; content.Controls.Add(panel);
        AddLabel(panel, "＋ Add a custom website", 18, 12, 16, CurrentText, true); AddLabel(panel, "Create your own shortcut. Only HTTP and HTTPS links are accepted.", 18, 42, 9, Muted);
        nameInput = AddTextField(panel, "Website name", "My website", 18, 78, 205); urlInput = AddTextField(panel, "Website URL", "https://example.com", 240, 78, 300); iconInput = AddTextField(panel, "Icon (emoji)", "🌐", 555, 78, 100);
        siteCategoryInput = new ComboBox { Location = new Point(670, 94), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList }; siteCategoryInput.Items.AddRange(Categories.Cast<object>().ToArray()); siteCategoryInput.SelectedItem = "Other"; panel.Controls.Add(siteCategoryInput); AddLabel(panel, "Category", 670, 72, 9, CurrentText, true);
        saveSiteButton = MakeButton("Add website", true); saveSiteButton.Location = new Point(18, 137); saveSiteButton.Size = new Size(125, 34); saveSiteButton.Click += (_, _) => SaveCustomSite(); panel.Controls.Add(saveSiteButton);
        var cancel = MakeButton("Cancel edit"); cancel.Location = new Point(153, 137); cancel.Size = new Size(110, 34); cancel.Click += (_, _) => ResetSiteForm(); panel.Controls.Add(cancel);
    }

    private TextBox AddTextField(Control parent, string label, string placeholder, int x, int y, int width)
    {
        AddLabel(parent, label, x, y - 22, 9, CurrentText, true); var box = new TextBox { Location = new Point(x, y), Width = width, Height = 29, PlaceholderText = placeholder }; parent.Controls.Add(box); return box;
    }

    private void SaveCustomSite()
    {
        string name = nameInput.Text.Trim(), url = urlInput.Text.Trim();
        if (name.Length == 0 || name.Length > 100) { MessageBox.Show("Enter a website name (up to 100 characters).", "WebSelec"); return; }
        if (!ValidUrl(url)) { MessageBox.Show("Only valid HTTP and HTTPS URLs are accepted.", "WebSelec"); return; }
        if (AllSites().Any(s => s.Url.Equals(url, StringComparison.OrdinalIgnoreCase) && s.Url != editingUrl)) { MessageBox.Show("That URL is already in your website list.", "WebSelec"); return; }
        var site = new Site { Name = name, Url = url, Category = siteCategoryInput.SelectedItem?.ToString() ?? "Other", Icon = string.IsNullOrWhiteSpace(iconInput.Text) ? "🌐" : iconInput.Text.Trim(), Custom = true };
        if (editingUrl != null)
        {
            var old = editingUrl; int i = data.CustomSites.FindIndex(s => s.Url == old); if (i >= 0) data.CustomSites[i] = site;
            data.Favorites = data.Favorites.Select(v => v == old ? url : v).ToList(); data.Recent = data.Recent.Select(s => s.Url == old ? site : s).ToList();
        }
        else data.CustomSites.Add(site);
        SaveData(); ResetSiteForm(); ShowPage("main");
    }
    private void ResetSiteForm() { editingUrl = null; if (nameInput == null) return; nameInput.Clear(); urlInput.Clear(); iconInput.Text = "🌐"; siteCategoryInput.SelectedItem = "Other"; saveSiteButton.Text = "Add website"; }
    private void EditSite(Site site) { ShowPage("main"); editingUrl = site.Url; nameInput.Text = site.Name; urlInput.Text = site.Url; iconInput.Text = site.Icon; siteCategoryInput.SelectedItem = site.Category; saveSiteButton.Text = "Save changes"; content.ScrollControlIntoView(nameInput); }
    private void DeleteSite(Site site) { if (MessageBox.Show($"Delete the custom website \"{site.Name}\"?", "WebSelec", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return; data.CustomSites.RemoveAll(s => s.Url == site.Url); data.Favorites.RemoveAll(v => v == site.Url); data.Recent.RemoveAll(s => s.Url == site.Url); SaveData(); ShowPage(currentTab); }

    private Panel SiteCard(Site site, int width)
    {
        var p = new Panel { Width = width, Height = 88, BackColor = CurrentPanel, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(12) };
        AddLabel(p, site.Icon, 14, 18, 23, CurrentText); AddLabel(p, site.Name, 60, 12, 12, CurrentText, true); AddLabel(p, site.Url, 60, 37, 8, Muted); AddLabel(p, site.Category, 60, 58, 8, Muted);
        int actionStart = site.Custom ? width - 285 : width - 205;
        var star = MakeButton(data.Favorites.Contains(site.Url) ? "★" : "☆"); star.Size = new Size(38, 32); star.Location = new Point(actionStart, 25); star.Click += (_, _) => ToggleFavorite(site); p.Controls.Add(star);
        var open = MakeButton("↗ Open", true); open.Size = new Size(78, 32); open.Location = new Point(actionStart + 45, 25); open.Click += (_, _) => OpenSite(site); p.Controls.Add(open);
        if (site.Custom)
        {
            var edit = MakeButton("Edit"); edit.Size = new Size(55, 32); edit.Location = new Point(actionStart + 128, 25); edit.Click += (_, _) => EditSite(site); p.Controls.Add(edit);
            var delete = MakeButton("Delete"); delete.Size = new Size(62, 32); delete.ForeColor = Color.Firebrick; delete.Location = new Point(actionStart + 188, 25); delete.Click += (_, _) => DeleteSite(site); p.Controls.Add(delete);
        }
        return p;
    }
    private void ToggleFavorite(Site site) { if (data.Favorites.Contains(site.Url)) data.Favorites.Remove(site.Url); else data.Favorites.Add(site.Url); SaveData(); ShowPage(currentTab); }
    private List<Site> AllSites() { var result = DefaultSites.Select(CloneSite).ToList(); result.AddRange(data.CustomSites.Select(s => { var c = CloneSite(s); c.Custom = true; return c; })); return result; }
    private static Site CloneSite(Site s) => new() { Name = s.Name, Url = s.Url, Category = s.Category, Icon = s.Icon, Custom = s.Custom };
    private List<Site> GetFavorites() => AllSites().Where(s => data.Favorites.Contains(s.Url)).ToList();

    private void OpenSite(Site site)
    {
        data.Recent.RemoveAll(s => s.Url == site.Url); data.Recent.Insert(0, CloneSite(site)); if (data.Recent.Count > 5) data.Recent = data.Recent.Take(5).ToList(); SaveData();
        try { if (site.Url == "about:blank") Process.Start(new ProcessStartInfo("about:blank") { UseShellExecute = true }); else Process.Start(new ProcessStartInfo(site.Url) { UseShellExecute = true }); }
        catch (Exception ex) { MessageBox.Show("Windows could not open this website.\n" + ex.Message, "WebSelec"); }
    }
    private void OpenRandom()
    {
        IEnumerable<Site> sites = AllSites();
        if (currentTab == "main") { sites = sites.Where(s => currentFilter == "all" || (currentFilter == "favorites" && data.Favorites.Contains(s.Url)) || (currentFilter == "custom" && s.Custom)); if (currentCategory != "all") sites = sites.Where(s => s.Category == currentCategory); var q = searchBox?.Text.Trim() ?? ""; if (q.Length > 0) sites = sites.Where(s => (s.Name + s.Url + s.Category).Contains(q, StringComparison.OrdinalIgnoreCase)); }
        var list = sites.ToList(); if (list.Count == 0) { MessageBox.Show("No matching websites to choose from.", "WebSelec"); return; } OpenSite(list[Random.Shared.Next(list.Count)]);
    }

    private void BuildSitePage(string title, string subtitle, List<Site> sites)
    {
        AddLabel(content, title, 0, 0, 25, CurrentText, true); AddLabel(content, subtitle, 0, 38, 10, Muted);
        int y = 80; if (sites.Count == 0) AddLabel(content, "No favorites yet. Tap the star on any website to save it here.", 10, y, 11, Muted);
        foreach (var s in sites) { var card = SiteCard(s, 890); card.Location = new Point(0, y); content.Controls.Add(card); y += 96; }
    }
    private void BuildHistory()
    {
        AddLabel(content, "◷ Recent history", 0, 0, 25, CurrentText, true); AddLabel(content, "Your five most recently opened websites, newest first.", 0, 38, 10, Muted);
        var clear = MakeButton("Clear history"); clear.Location = new Point(720, 8); clear.Click += (_, _) => { data.Recent.Clear(); SaveData(); ShowPage("history"); }; content.Controls.Add(clear);
        int y = 80; foreach (var s in data.Recent) { var card = SiteCard(s, 890); card.Location = new Point(0, y); content.Controls.Add(card); y += 96; }
        if (data.Recent.Count == 0) AddLabel(content, "No history yet. Websites opened through WebSelec will appear here.", 10, y, 11, Muted);
    }

    private void BuildSettings()
    {
        AddLabel(content, "Settings", 0, 0, 25, CurrentText, true); AddLabel(content, "Make WebSelec work the way you like.", 0, 38, 10, Muted);
        var appearance = SettingsCard("🎨 Appearance", 0, 80, 430, 150); AddLabel(appearance, "Choose how WebSelec looks.", 16, 42, 9, Muted);
        AddLabel(appearance, "Theme", 16, 73, 9, CurrentText, true); appearanceBox = new ComboBox { Location = new Point(16, 95), Width = 190, DropDownStyle = ComboBoxStyle.DropDownList }; appearanceBox.Items.AddRange(new object[] { "System", "Light", "Dark" }); appearanceBox.SelectedItem = char.ToUpper(data.Appearance[0]) + data.Appearance[1..]; appearanceBox.SelectedIndexChanged += (_, _) => { data.Appearance = appearanceBox.SelectedItem?.ToString()?.ToLowerInvariant() ?? "system"; SaveData(); dark = data.Appearance == "dark" || (data.Appearance == "system" && SystemUsesDarkMode()); ApplyTheme(); }; appearance.Controls.Add(appearanceBox);
        var reset = MakeButton("Reset settings"); reset.Location = new Point(220, 94); reset.Click += (_, _) => { data.Appearance = "system"; dark = SystemUsesDarkMode(); SaveData(); ShowPage("settings"); }; appearance.Controls.Add(reset);
        var activationCard = SettingsCard("🔐 Activation", 450, 80, 430, 150); AddLabel(activationCard, activation == null ? "This installation is not activated." : $"Activated for v{Version} on {activation.ActivatedAt:g}.", 16, 45, 9, Muted); AddLabel(activationCard, "Demo activation only; no cryptographic verification.", 16, 72, 9, Muted);
        var backup = SettingsCard("💾 Backup and restore", 0, 245, 430, 170); AddLabel(backup, "Export your saved data or restore a JSON backup.", 16, 45, 9, Muted);
        var export = MakeButton("Export JSON", true); export.Location = new Point(16, 82); export.Click += (_, _) => ExportBackup(); backup.Controls.Add(export);
        var import = MakeButton("Import JSON"); import.Location = new Point(130, 82); import.Click += (_, _) => ImportBackup(); backup.Controls.Add(import);
        var resetAll = MakeButton("Reset all data"); resetAll.Location = new Point(245, 82); resetAll.Click += (_, _) => ResetAll(); backup.Controls.Add(resetAll);
        var updates = SettingsCard("🔄 Updates", 450, 245, 430, 170); AddLabel(updates, "Live GitHub update checking is not connected in this build.", 16, 45, 9, Muted); var check = MakeButton("Check for updates"); check.Location = new Point(16, 82); check.Click += (_, _) => MessageBox.Show("No online check was performed. Live GitHub release checking and downloads are not connected in this build.", "WebSelec"); updates.Controls.Add(check);
        var compatibility = SettingsCard("💻 Compatibility", 0, 430, 430, 160); AddLabel(compatibility, $"Detected OS: {Environment.OSVersion}\nRuntime: {Environment.Version}\nArchitecture: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}", 16, 45, 9, Muted);
        var detect = MakeButton("Refresh detection"); detect.Location = new Point(16, 105); detect.Click += (_, _) => ShowPage("settings"); compatibility.Controls.Add(detect);
        var resetSettings = MakeButton("Reset appearance to System"); resetSettings.Location = new Point(450, 430); resetSettings.Click += (_, _) => { data.Appearance = "system"; dark = SystemUsesDarkMode(); SaveData(); ShowPage("settings"); }; content.Controls.Add(resetSettings);
    }
    private Panel SettingsCard(string title, int x, int y, int w, int h) { var p = new Panel { Location = new Point(x, y), Size = new Size(w, h), BackColor = CurrentPanel, BorderStyle = BorderStyle.FixedSingle }; AddLabel(p, title, 16, 12, 14, CurrentText, true); content.Controls.Add(p); return p; }
    private void ExportBackup()
    {
        using var dialog = new SaveFileDialog { Filter = "JSON backup (*.json)|*.json", FileName = "webselec-backup-v1.0.0.json" }; if (dialog.ShowDialog() != DialogResult.OK) return;
        try { var payload = new { product = "Web Selector", version = Version, exportedAt = DateTime.UtcNow, data }; File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(payload, jsonOptions)); MessageBox.Show("JSON backup exported.", "WebSelec"); } catch (Exception ex) { MessageBox.Show(ex.Message, "WebSelec"); }
    }
    private void ImportBackup()
    {
        using var dialog = new OpenFileDialog { Filter = "JSON backup (*.json)|*.json" }; if (dialog.ShowDialog() != DialogResult.OK) return;
        try { using var doc = JsonDocument.Parse(File.ReadAllText(dialog.FileName)); var node = doc.RootElement.TryGetProperty("data", out var d) ? d : doc.RootElement; var incoming = JsonSerializer.Deserialize<AppData>(node.GetRawText(), jsonOptions) ?? new AppData(); if (MessageBox.Show("Import this backup and replace your current saved website data?", "WebSelec", MessageBoxButtons.YesNo) != DialogResult.Yes) return; data = incoming; SaveData(); ShowPage("settings"); MessageBox.Show("Backup imported.", "WebSelec"); } catch { MessageBox.Show("That file is not a valid WebSelec JSON backup.", "WebSelec"); }
    }
    private void ResetAll()
    {
        if (MessageBox.Show("Reset ALL WebSelec data? Custom websites, favorites, history and preferences will be deleted.", "WebSelec", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        if (MessageBox.Show("Are you absolutely sure? This cannot be undone.", "WebSelec", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        data = new AppData(); SaveData(); dark = SystemUsesDarkMode(); BuildApplication();
    }

    private void BuildArchive()
    {
        AddLabel(content, "Release archive", 0, 0, 25, CurrentText, true); AddLabel(content, "The history of WebSelec.", 0, 38, 10, Muted);
        AddLabel(content, "🌲  WebSelec 1.0.0 — Red Forest  ·  Current release", 0, 85, 15, forest, true);
        AddLabel(content, "The first major release with a complete UI overhaul, expanded website library, categories, favorites, custom shortcuts, backups, and mandatory demo activation.", 0, 115, 10, CurrentText);
        string[] releases = { "v1.0.0 — Red Forest | Completely redesigned interface, additional websites, categories, favorites page, history page, mandatory activation, theme improvements, custom websites and JSON backups.", "v0.1.2 Preview | Introduced the activation screen, eight preset demonstration keys, demo key generation, and locally saved activation.", "v0.1.1 — Emergency Patch | Worked to restore Windows detection and preserve v0.1.0 functionality.", "v0.1.0 — Feature Release | Added custom website management, search, favorites, recent history, random selection, JSON backup and restore, appearance controls, and reset options.", "v0.0.5 — Refinement Update | Refined operating-system detection and compatibility handling.", "v0.0.3 | Introduced the Signed tab, release archive, and planned versions.", "v0.0.2 | Added settings, more websites, and a changelog.", "v0.0.1 | The original basic website selector." };
        int y = 165; foreach (var release in releases) { var p = new Panel { Location = new Point(0, y), Size = new Size(890, 64), BackColor = CurrentPanel, BorderStyle = BorderStyle.FixedSingle }; AddLabel(p, release, 12, 10, 9, CurrentText); content.Controls.Add(p); y += 72; }
    }
    private void BuildIdeas()
    {
        AddLabel(content, "Upcoming / Ideas", 0, 0, 25, CurrentText, true); AddLabel(content, "The future of WebSelec is still being written.", 0, 38, 10, Muted);
        var card = SettingsCard("🧭 Planned major releases", 0, 80, 890, 140); AddLabel(card, "v2.0.0 — Lucerne", 16, 48, 13, CurrentText, true); AddLabel(card, "Named after Lucerne, Switzerland. Features are still to be decided.", 16, 78, 9, Muted);
        var ideas = SettingsCard("💡 Ideas under consideration", 0, 240, 890, 250); string[] lines = { "• More built-in websites and category options.", "• More detailed operating-system and device detection.", "• Real GitHub release checking and downloadable updates.", "• Cryptographic release-signature verification.", "• Major UI redesigns in future versions, if needed.", "• WebSelec Pro as a possible future project.", "• Separate native editions as a long-term idea." }; int y = 48; foreach (var line in lines) { AddLabel(ideas, line, 16, y, 10, CurrentText); y += 25; }
        AddLabel(ideas, "Ideas are not promises or release commitments. Nothing here has an announced delivery date.", 16, 225, 9, Muted);
    }

    private Panel StatCard(string label, string value)
    {
        var p = new Panel { Size = new Size(285, 65), BackColor = CurrentPanel, Margin = new Padding(0, 0, 12, 0), BorderStyle = BorderStyle.FixedSingle };
        AddLabel(p, value, 14, 5, 21, CurrentText, true); AddLabel(p, label, 16, 36, 8, Muted); return p;
    }
    private static void AddLabel(Control parent, string text, int x, int y, float size, Color color, bool bold = false)
    {
        var l = new Label { Text = text, Location = new Point(x, y), AutoSize = true, ForeColor = color, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), BackColor = Color.Transparent }; parent.Controls.Add(l);
    }
    private Button MakeButton(string text, bool primary = false)
    {
        var b = new Button { Text = text, FlatStyle = FlatStyle.Flat, BackColor = primary ? forest : CurrentPanel, ForeColor = primary ? Color.White : CurrentText, Font = new Font("Segoe UI", 9), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
        b.FlatAppearance.BorderColor = primary ? forest : Color.FromArgb(210, 220, 212); b.FlatAppearance.BorderSize = 1; return b;
    }
    private bool ValidUrl(string value) => Uri.TryCreate(value, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    private static bool SystemUsesDarkMode()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("AppsUseLightTheme") is int value && value == 0;
        }
        catch { return false; }
    }
    private Color CurrentBg => dark ? darkBg : lightBg;
    private Color CurrentPanel => dark ? darkPanel : Color.White;
    private Color CurrentText => dark ? darkText : Color.FromArgb(25, 37, 29);
    private Color Muted => dark ? Color.FromArgb(160, 176, 162) : Color.FromArgb(106, 120, 109);
    private void ApplyTheme()
    {
        if (root == null) return;
        root.BackColor = CurrentBg; content.BackColor = CurrentBg; nav.BackColor = CurrentPanel;
        foreach (Control c in root.Controls) ApplyThemeRecursive(c);
    }
    private void ApplyThemeRecursive(Control parent)
    {
        foreach (Control c in parent.Controls)
        {
            if (c is Panel p) { if (p.BackColor != Color.FromArgb(21, 60, 40)) p.BackColor = CurrentPanel; }
            if (c is Label l && l.ForeColor != Color.White && l.ForeColor != Color.FromArgb(225, 238, 228)) l.ForeColor = CurrentText;
            if (c is Button b) { bool primary = b.BackColor == forest; b.BackColor = primary ? forest : CurrentPanel; b.ForeColor = primary ? Color.White : CurrentText; }
            if (c is TextBox t) { t.BackColor = CurrentPanel; t.ForeColor = CurrentText; }
            if (c is ComboBox cb) { cb.BackColor = CurrentPanel; cb.ForeColor = CurrentText; }
            ApplyThemeRecursive(c);
        }
    }
}
