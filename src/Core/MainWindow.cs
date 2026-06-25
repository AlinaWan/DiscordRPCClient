using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DiscordRPC.Models;

using Brushes = System.Windows.Media.Brushes;
using CheckBox = System.Windows.Controls.CheckBox;
using Color = System.Windows.Media.Color;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Panel = System.Windows.Controls.Panel;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using TextBox = System.Windows.Controls.TextBox;
using WpfButton = System.Windows.Controls.Button;

namespace DiscordRPC.Core
{
    public partial class MainWindow : Window
    {
        private DiscordRpcClient? _rpcClient;
        private RpcProfile _currentProfile = new();
        private string _currentFilePath = "";
        private bool _isDirty = false;

        // UI Core Components
        private TextBlock _statusLabel = null!;
        private Border _statusBorder = null!;
        private ScrollViewer _scrollContainer = null!;

        // Form Fields references for binding data inputs dynamically
        private TextBox txtClientId = null!, txtDetails = null!, txtState = null!;
        private CheckBox chkUseTime = null!, chkAsRemaining = null!;
        private TextBox txtDuration = null!;
        private TextBox txtLargeKey = null!, txtLargeText = null!, txtSmallKey = null!, txtSmallText = null!;
        private TextBox txtPartyId = null!, txtPartyCur = null!, txtPartyMax = null!;
        private TextBox txtJoinSecret = null!, txtSpectateSecret = null!;
        private TextBox txtBtn1Label = null!, txtBtn1Url = null!, txtBtn2Label = null!, txtBtn2Url = null!;

        public MainWindow()
        {
            Width = 460;
            Height = 700;
            Background = new SolidColorBrush(Color.FromRgb(25, 25, 25));
            Foreground = Brushes.White;
            Title = "Discord RPC Client";
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UI.WindowStyle.SetTitleBarColor(this, Color.FromRgb(0x58, 0x65, 0xF2)); // Discord Blurple
            LoadSavedProfileOnStartup();
            BuildUI();
        }

        private void LoadSavedProfileOnStartup()
        {
            try
            {
                // Accessing settings directly via absolute root to bypass .Core collision
                string lastSavedPath = DiscordRPC.Properties.Settings.Default.LastOpenProfilePath;

                if (!string.IsNullOrEmpty(lastSavedPath) && File.Exists(lastSavedPath))
                {
                    string json = File.ReadAllText(lastSavedPath);
                    _currentProfile = JsonSerializer.Deserialize<RpcProfile>(json) ?? new RpcProfile();
                    _currentFilePath = lastSavedPath;
                    return;
                }
            }
            catch { /* Silently fallback if settings or file corrupted */ }

            // Silently ignore and load blank
            _currentProfile = new RpcProfile();
            _currentFilePath = "";
        }

        private void SaveProfile(bool promptLocation)
        {
            UpdateProfileFromUiValues();

            if (promptLocation || string.IsNullOrEmpty(_currentFilePath))
            {
                // Generate the default filename with the current timestamp
                string defaultFileName = $"RPC_Profile_{DateTime.Now:yyMMdd_HHmmss}.json";

                var sfd = new SaveFileDialog
                {
                    Filter = "RPC Profile (*.json)|*.json",
                    FileName = defaultFileName // Sets the initial file name in the dialog
                };

                if (sfd.ShowDialog() == true)
                {
                    _currentFilePath = sfd.FileName;
                }
                else
                    return;
            }

            try
            {
                string json = JsonSerializer.Serialize(_currentProfile, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_currentFilePath, json);

                // Persist the workspace path back to application Settings storage cleanly 
                DiscordRPC.Properties.Settings.Default.LastOpenProfilePath = _currentFilePath;
                DiscordRPC.Properties.Settings.Default.Save();
                _isDirty = false;
                SetStatus($"Profile Saved: {Path.GetFileName(_currentFilePath)}", System.Windows.Media.Color.FromRgb(30, 120, 60));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving profile: {ex.Message}");
            }
        }


        private void LoadProfileFromFile()
        {
            var ofd = new OpenFileDialog { Filter = "RPC Profile (*.json)|*.json" };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    string json = File.ReadAllText(ofd.FileName);
                    _currentProfile = JsonSerializer.Deserialize<RpcProfile>(json) ?? new RpcProfile();
                    _currentFilePath = ofd.FileName;

                    // Persist the workspace path back to application Settings storage cleanly
                    DiscordRPC.Properties.Settings.Default.LastOpenProfilePath = _currentFilePath;
                    DiscordRPC.Properties.Settings.Default.Save();

                    _isDirty = false;
                    BuildUI();
                    SetStatus($"Loaded: {Path.GetFileName(_currentFilePath)}", Color.FromRgb(30, 80, 120));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading profile: {ex.Message}");
                }
            }
        }

        private void ClearToNewBlankProfile()
        {
            _currentProfile = new RpcProfile();
            _currentFilePath = "";
            _isDirty = false;
            BuildUI();
            SetStatus("New Blank Profile Created", Color.FromRgb(70, 70, 70));
        }

        private void UpdateRpc()
        {
            UpdateProfileFromUiValues();

            if (string.IsNullOrWhiteSpace(_currentProfile.ClientId))
            {
                SetStatus("Error: Client ID cannot be empty!", Color.FromRgb(150, 40, 40));
                return;
            }

            try
            {
                // Reinitialize client if ID changed
                if (_rpcClient == null || _rpcClient.ApplicationID != _currentProfile.ClientId)
                {
                    _rpcClient?.Dispose();
                    _rpcClient = new DiscordRpcClient(_currentProfile.ClientId);
                    _rpcClient.Initialize();
                }

                var presence = new RichPresence
                {
                    Details = string.IsNullOrEmpty(_currentProfile.Details) ? null : _currentProfile.Details,
                    State = string.IsNullOrEmpty(_currentProfile.State) ? null : _currentProfile.State
                };

                // Timestamps configuration
                if (_currentProfile.UseTimestamps)
                {
                    if (_currentProfile.AsTimeRemaining)
                    {
                        presence.Timestamps = new Timestamps
                        {
                            Start = Timestamps.Now.Start,
                            End = DateTime.UtcNow.AddMinutes(_currentProfile.TotalDurationMinutes)
                        };
                    }
                    else
                    {
                        presence.Timestamps = Timestamps.Now;
                    }
                }

                // Assets setup
                if (!string.IsNullOrEmpty(_currentProfile.LargeImageKey) || !string.IsNullOrEmpty(_currentProfile.SmallImageKey))
                {
                    presence.Assets = new Assets
                    {
                        LargeImageKey = string.IsNullOrEmpty(_currentProfile.LargeImageKey) ? null : _currentProfile.LargeImageKey,
                        LargeImageText = string.IsNullOrEmpty(_currentProfile.LargeImageText) ? null : _currentProfile.LargeImageText,
                        SmallImageKey = string.IsNullOrEmpty(_currentProfile.SmallImageKey) ? null : _currentProfile.SmallImageKey,
                        SmallImageText = string.IsNullOrEmpty(_currentProfile.SmallImageText) ? null : _currentProfile.SmallImageText
                    };
                }

                // Party configuration
                if (!string.IsNullOrEmpty(_currentProfile.PartyId))
                {
                    presence.Party = new Party
                    {
                        ID = _currentProfile.PartyId,
                        Size = _currentProfile.PartyCurrentSize,
                        Max = _currentProfile.PartyMaxSize
                    };
                }

                // Secrets configurations
                if (!string.IsNullOrEmpty(_currentProfile.JoinSecret) || !string.IsNullOrEmpty(_currentProfile.SpectateSecret))
                {
                    presence.Secrets = new Secrets
                    {
                        JoinSecret = string.IsNullOrEmpty(_currentProfile.JoinSecret) ? null : _currentProfile.JoinSecret,
                        SpectateSecret = string.IsNullOrEmpty(_currentProfile.SpectateSecret) ? null : _currentProfile.SpectateSecret
                    };
                }

                // Custom Rich Buttons 
                var dynamicButtons = new System.Collections.Generic.List<DiscordRPC.Button>();
                if (!string.IsNullOrWhiteSpace(_currentProfile.Button1Label) && !string.IsNullOrWhiteSpace(_currentProfile.Button1Url))
                    dynamicButtons.Add(new DiscordRPC.Button { Label = _currentProfile.Button1Label, Url = _currentProfile.Button1Url });
                if (!string.IsNullOrWhiteSpace(_currentProfile.Button2Label) && !string.IsNullOrWhiteSpace(_currentProfile.Button2Url))
                    dynamicButtons.Add(new DiscordRPC.Button { Label = _currentProfile.Button2Label, Url = _currentProfile.Button2Url });

                if (dynamicButtons.Count > 0)
                    presence.Buttons = dynamicButtons.ToArray();

                _rpcClient.SetPresence(presence);
                SetStatus("Discord Rich Presence Active!", Color.FromRgb(40, 130, 90));
            }
            catch (Exception ex)
            {
                SetStatus($"RPC Engine Error: {ex.Message}", Color.FromRgb(150, 40, 40));
            }
        }

        private void UpdateProfileFromUiValues()
        {
            // Clean helper checks to prevent ghost watermarks saving out directly to profile variables
            Func<TextBox, string> getCleanText = (tb) => (tb.Text == tb.Tag as string) ? "" : tb.Text;

            _currentProfile.ClientId = getCleanText(txtClientId);
            _currentProfile.Details = getCleanText(txtDetails);
            _currentProfile.State = getCleanText(txtState);
            _currentProfile.UseTimestamps = chkUseTime.IsChecked == true;
            _currentProfile.AsTimeRemaining = chkAsRemaining.IsChecked == true;

            string durationText = getCleanText(txtDuration);
            int.TryParse(string.IsNullOrEmpty(durationText) ? "0" : durationText, out int dur);
            _currentProfile.TotalDurationMinutes = dur;

            _currentProfile.LargeImageKey = getCleanText(txtLargeKey);
            _currentProfile.LargeImageText = getCleanText(txtLargeText);
            _currentProfile.SmallImageKey = getCleanText(txtSmallKey);
            _currentProfile.SmallImageText = getCleanText(txtSmallText);

            _currentProfile.PartyId = getCleanText(txtPartyId);

            string pCurText = getCleanText(txtPartyCur);
            int.TryParse(string.IsNullOrEmpty(pCurText) ? "0" : pCurText, out int pCur);
            _currentProfile.PartyCurrentSize = pCur;

            string pMaxText = getCleanText(txtPartyMax);
            int.TryParse(string.IsNullOrEmpty(pMaxText) ? "0" : pMaxText, out int pMax);
            _currentProfile.PartyMaxSize = pMax;

            _currentProfile.JoinSecret = getCleanText(txtJoinSecret);
            _currentProfile.SpectateSecret = getCleanText(txtSpectateSecret);

            _currentProfile.Button1Label = getCleanText(txtBtn1Label);
            _currentProfile.Button1Url = getCleanText(txtBtn1Url);
            _currentProfile.Button2Label = getCleanText(txtBtn2Label);
            _currentProfile.Button2Url = getCleanText(txtBtn2Url);
        }

        internal void BuildUI()
        {
            var mainLayout = new Grid();
            UI.ScrollbarStyle.Apply(mainLayout);

            mainLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            _scrollContainer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            var panelStack = new StackPanel { Margin = new Thickness(15) };

            // Profile File Storage Management Header
            panelStack.Children.Add(new TextBlock { Text = "PROFILE MANAGEMENT", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = Brushes.DimGray, Margin = new Thickness(0, 0, 0, 6) });
            var profileControlGrid = new Grid { Margin = new Thickness(0, 0, 0, 15) };
            profileControlGrid.ColumnDefinitions.Add(new ColumnDefinition());
            profileControlGrid.ColumnDefinitions.Add(new ColumnDefinition());
            profileControlGrid.ColumnDefinitions.Add(new ColumnDefinition());

            var btnNew = CreateStyledButton("New", () => ClearToNewBlankProfile());
            Grid.SetColumn(btnNew, 0);
            btnNew.Margin = new Thickness(0, 0, 2, 0);
            var btnLoad = CreateStyledButton("Open...", () => LoadProfileFromFile());
            Grid.SetColumn(btnLoad, 1);
            btnLoad.Margin = new Thickness(2, 0, 2, 0);
            var btnSave = CreateStyledButton("Save Profile", () => SaveProfile(false));
            Grid.SetColumn(btnSave, 2);
            btnSave.Margin = new Thickness(2, 0, 0, 0);
            profileControlGrid.Children.Add(btnNew);
            profileControlGrid.Children.Add(btnLoad);
            profileControlGrid.Children.Add(btnSave);
            panelStack.Children.Add(profileControlGrid);

            // Base Parameters
            panelStack.Children.Add(new TextBlock { Text = "CORE CONFIGURATION", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = Brushes.DimGray, Margin = new Thickness(0, 5, 0, 6) });
            panelStack.Children.Add(CreateFormInputRow("Application Client ID:", txtClientId = new TextBox { Text = _currentProfile.ClientId, Tag = "Snowflake ID (e.g. 1234567890123456)" }));
            panelStack.Children.Add(CreateFormInputRow("Activity Details:", txtDetails = new TextBox { Text = _currentProfile.Details, Tag = "What the user is doing (Max 128 chars)", MaxLength = 128 }));
            panelStack.Children.Add(CreateFormInputRow("Activity State:", txtState = new TextBox { Text = _currentProfile.State, Tag = "The user's current party status (Max 128 chars)", MaxLength = 128 }));

            // Time Config Row
            panelStack.Children.Add(new TextBlock { Text = "TIMESTAMP METRICS", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = Brushes.DimGray, Margin = new Thickness(0, 12, 0, 6) });
            var timestampContainer = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };
            chkUseTime = new CheckBox { Content = "Display Elapsed Activity Time", IsChecked = _currentProfile.UseTimestamps, Foreground = Brushes.LightGray, Margin = new Thickness(5, 2, 0, 4) };
            chkAsRemaining = new CheckBox { Content = "Display Dynamic Countdown Style Remaining", IsChecked = _currentProfile.AsTimeRemaining, Foreground = Brushes.LightGray, Margin = new Thickness(5, 2, 0, 4) };
            timestampContainer.Children.Add(chkUseTime);
            timestampContainer.Children.Add(chkAsRemaining);
            panelStack.Children.Add(timestampContainer);
            panelStack.Children.Add(CreateFormInputRow("Remaining Duration (Mins):", txtDuration = new TextBox { Text = _currentProfile.TotalDurationMinutes.ToString(), Width = 60, HorizontalAlignment = HorizontalAlignment.Left, Tag = "Integer" }));

            // Asset Keys Visual Settings
            panelStack.Children.Add(new TextBlock { Text = "ASSET ASSETS IMAGES", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = Brushes.DimGray, Margin = new Thickness(0, 12, 0, 6) });
            panelStack.Children.Add(CreateFormInputRow("Large Image Key String:", txtLargeKey = new TextBox { Text = _currentProfile.LargeImageKey, Tag = "asset_name_or_url (Max 32 chars)", MaxLength = 32 }));
            panelStack.Children.Add(CreateFormInputRow("Large Image Hover Text:", txtLargeText = new TextBox { Text = _currentProfile.LargeImageText, Tag = "Tooltip text on hover (Max 128 chars)", MaxLength = 128 }));
            panelStack.Children.Add(CreateFormInputRow("Small Image Key String:", txtSmallKey = new TextBox { Text = _currentProfile.SmallImageKey, Tag = "asset_name_or_url (Max 32 chars)", MaxLength = 32 }));
            panelStack.Children.Add(CreateFormInputRow("Small Image Hover Text:", txtSmallText = new TextBox { Text = _currentProfile.SmallImageText, Tag = "Tooltip text on hover (Max 128 chars)", MaxLength = 128 }));

            // Party Configuration
            panelStack.Children.Add(new TextBlock { Text = "MATCHMAKING PARTY LOBBY", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = Brushes.DimGray, Margin = new Thickness(0, 12, 0, 6) });
            panelStack.Children.Add(CreateFormInputRow("Unique Party ID Group:", txtPartyId = new TextBox { Text = _currentProfile.PartyId, Tag = "id_string (Max 128 chars)", MaxLength = 128 }));
            panelStack.Children.Add(CreateFormInputRow("Lobby Current Slots Filled:", txtPartyCur = new TextBox { Text = _currentProfile.PartyCurrentSize.ToString(), Width = 50, HorizontalAlignment = HorizontalAlignment.Left, Tag = "Int" }));
            panelStack.Children.Add(CreateFormInputRow("Lobby Maximum Cap Slots:", txtPartyMax = new TextBox { Text = _currentProfile.PartyMaxSize.ToString(), Width = 50, HorizontalAlignment = HorizontalAlignment.Left, Tag = "Int" }));

            // Secrets Secure Links
            panelStack.Children.Add(new TextBlock { Text = "RPC CLIENT SECRETS ENGINES", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = Brushes.DimGray, Margin = new Thickness(0, 12, 0, 6) });
            panelStack.Children.Add(CreateFormInputRow("Join Application Secret:", txtJoinSecret = new TextBox { Text = _currentProfile.JoinSecret, Tag = "Secret for join connection (Max 128)", MaxLength = 128 }));
            panelStack.Children.Add(CreateFormInputRow("Spectate Profile Secret:", txtSpectateSecret = new TextBox { Text = _currentProfile.SpectateSecret, Tag = "Secret for spectate connection (Max 128)", MaxLength = 128 }));

            // Dual Custom Interact Buttons Parameters
            panelStack.Children.Add(new TextBlock { Text = "CUSTOM ACTIONS BUTTONS LINKS (MAX 2)", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = Brushes.DimGray, Margin = new Thickness(0, 12, 0, 6) });
            panelStack.Children.Add(CreateFormInputRow("Button #1 Text Label:", txtBtn1Label = new TextBox { Text = _currentProfile.Button1Label, Tag = "Button title (Max 32 chars)", MaxLength = 32 }));
            panelStack.Children.Add(CreateFormInputRow("Button #1 Target URL:", txtBtn1Url = new TextBox { Text = _currentProfile.Button1Url, Tag = "https://example.com (Max 512)", MaxLength = 512 }));
            panelStack.Children.Add(CreateFormInputRow("Button #2 Text Label:", txtBtn2Label = new TextBox { Text = _currentProfile.Button2Label, Tag = "Button title (Max 32 chars)", MaxLength = 32 }));
            panelStack.Children.Add(CreateFormInputRow("Button #2 Target URL:", txtBtn2Url = new TextBox { Text = _currentProfile.Button2Url, Tag = "https://example.com (Max 512)", MaxLength = 512 }));

            // Submission Execution Trigger
            var btnFireUpdate = new WpfButton
            {
                Content = "SYNC RPC TO DISCORD",
                Height = 40,
                FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush(Color.FromRgb(0x58, 0x65, 0xF2)), // Discord Blurple
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 15, 0, 5)
            };
            btnFireUpdate.Click += (s, e) => UpdateRpc();
            panelStack.Children.Add(btnFireUpdate);

            // Register Dynamic Ghost Text/Watermark Functionality Programmatically
            InitializeGhostTextEffects(panelStack);

            // Hook Event Listeners for modification tracking flags
            HookInputFieldsChangeDirtyTracking(panelStack);

            _scrollContainer.Content = panelStack;
            mainLayout.Children.Add(_scrollContainer);

            // Status Frame Base UI
            _statusLabel = new TextBlock { FontSize = 11, FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.White };
            _statusBorder = new Border { Height = 32, Padding = new Thickness(12, 0, 12, 0), Child = _statusLabel };
            SetStatus(string.IsNullOrEmpty(_currentFilePath) ? "Ready (Unsaved Workspace)" : $"Active Config: {Path.GetFileName(_currentFilePath)}", Color.FromRgb(45, 45, 45));

            Grid.SetRow(_statusBorder, 1);
            mainLayout.Children.Add(_statusBorder);

            Content = mainLayout;

            // --- Footer Link Construction ---
            var footerText = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0),
                FontSize = 11
            };
            var hl = new System.Windows.Documents.Hyperlink(new System.Windows.Documents.Run("Made with ♡ by Riri"))
            {
                Foreground = Brushes.DarkGray,
                TextDecorations = null, // Removes the underline
                NavigateUri = new Uri("https://github.com/AlinaWan/DiscordRPCClient")
            };
            hl.RequestNavigate += (s, e) => {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
                e.Handled = true;
            };
            footerText.Inlines.Add(hl);

            // Overlay it on top of the status bar grid row (aligned to the right side)
            Grid.SetRow(footerText, 1);
            mainLayout.Children.Add(footerText);
        }

        private void InitializeGhostTextEffects(Panel container)
        {
            Action<TextBox> applyGhost = (tb) =>
            {
                if (string.IsNullOrEmpty(tb.Text) && tb.Tag is string hint)
                {
                    tb.Text = hint;
                    tb.Foreground = Brushes.Gray;
                }
            };

            foreach (var element in container.Children)
            {
                if (element is Grid g)
                {
                    foreach (var sub in g.Children)
                    {
                        if (sub is TextBox tb)
                        {
                            applyGhost(tb);

                            tb.GotFocus += (s, e) =>
                            {
                                if (tb.Tag is string hint && tb.Text == hint)
                                {
                                    tb.Text = "";
                                    tb.Foreground = Brushes.White;
                                }
                            };

                            tb.LostFocus += (s, e) => applyGhost(tb);
                        }
                    }
                }
            }
        }

        private void HookInputFieldsChangeDirtyTracking(Panel mainStack)
        {
            foreach (var element in mainStack.Children)
            {
                if (element is Grid g)
                {
                    foreach (var sub in g.Children)
                    {
                        if (sub is TextBox tb)
                            tb.TextChanged += (s, e) => _isDirty = true;
                    }
                }
                if (element is StackPanel sp)
                {
                    foreach (var sub in sp.Children)
                    {
                        if (sub is CheckBox cb)
                            cb.Checked += (s, e) => _isDirty = true;
                        if (sub is CheckBox cb2)
                            cb2.Unchecked += (s, e) => _isDirty = true;
                    }
                }
            }
        }

        private UIElement CreateFormInputRow(string labelText, TextBox inputControl)
        {
            var gridContainer = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            gridContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            gridContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            gridContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var lb = new TextBlock { Text = labelText, Foreground = Brushes.LightGray, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0), FontSize = 11 };
            Grid.SetColumn(lb, 0);
            gridContainer.Children.Add(lb);

            var dottedConnector = new System.Windows.Shapes.Rectangle { Height = 1, Fill = new SolidColorBrush(Color.FromRgb(55, 55, 55)), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 1, 8, 0) };
            Grid.SetColumn(dottedConnector, 1);
            gridContainer.Children.Add(dottedConnector);

            inputControl.Height = 24;
            if (double.IsNaN(inputControl.Width))
                inputControl.Width = 240;
            inputControl.Background = new SolidColorBrush(Color.FromRgb(45, 45, 45));
            inputControl.Foreground = Brushes.White;
            inputControl.BorderThickness = new Thickness(0);
            inputControl.VerticalContentAlignment = VerticalAlignment.Center;
            inputControl.Padding = new Thickness(4, 0, 4, 0);

            Grid.SetColumn(inputControl, 2);
            gridContainer.Children.Add(inputControl);
            return gridContainer;
        }

        private WpfButton CreateStyledButton(string text, Action clickAction)
        {
            var btn = new WpfButton { Content = text, Height = 32, Focusable = false, Background = new SolidColorBrush(Color.FromRgb(45, 45, 45)), Foreground = Brushes.White, BorderThickness = new Thickness(0) };
            btn.Click += (s, e) => clickAction();
            return btn;
        }

        private void SetStatus(string text, System.Windows.Media.Color baseColor)
        {
            _statusLabel.Text = text.ToUpper();
            _statusBorder.Background = new SolidColorBrush(baseColor);
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_isDirty)
            {
                var result = MessageBox.Show("You have unsaved changes to your current profile. Save changes before exiting?", "Unsaved Changes Found", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    SaveProfile(false);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
            _rpcClient?.Dispose();
        }
    }
}
