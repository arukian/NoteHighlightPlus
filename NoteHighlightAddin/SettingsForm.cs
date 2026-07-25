using Infrastructure.Core;
using NoteHighlightAddin.Highlighting.KeywordGroups;
using NoteHighlightAddin.Highlighting.KeywordGroups.Services;
using NoteHighlightAddin.Highlighting.KeywordGroups.Testing;
using NoteHighlightAddin.Highlighting.KeywordGroups.ViewModels;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace NoteHighlightAddin
{
    public partial class SettingsForm : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private LanguageManager _languageManager;
        private readonly LanguageEditorViewModel _languageEditor;
        private bool _isLoadingGroupDetails;
        private Button _btnAddKeywordGroup;
        private Button _btnRemoveKeywordGroup;

        public SettingsForm()
        {
            InitializeComponent();

            _languageEditor =
                new LanguageEditorViewModel(
                    new LanguageEditorService());

            // Connect the word editor events explicitly. This avoids depending
            // on the WinForms designer event wiring.
            btnAddGroupWord.Click +=
                btnAddGroupWord_Click;

            btnRemoveGroupWord.Click +=
                btnRemoveGroupWord_Click;

            lbxGroupWords.SelectedIndexChanged +=
                lbxGroupWords_SelectedIndexChanged;

            txtNewGroupWord.KeyDown +=
                txtNewGroupWord_KeyDown;

            InitializeGroupManagementControls();

            fontDialog1.Font =
                new Font(
                    NoteHighlightForm.Properties.Settings.Default.Font,
                    NoteHighlightForm.Properties.Settings.Default.FontSize);

            btnFont.Text =
                "Font:" +
                fontDialog1.Font.Name +
                ", Size:" +
                fontDialog1.Font.Size;

            btnFont.Font = fontDialog1.Font;

            cbShowTableBorder.Checked =
                NoteHighlightForm.Properties.Settings.Default.ShowTableBorder;

            InitializeLanguageManager();
            UpdateWordEditorState();
        }

        private void InitializeGroupManagementControls()
        {
            int availableWidth =
                lbxKeywordGroups.Width;

            int buttonSpacing = 6;

            int buttonWidth =
                (availableWidth - buttonSpacing) / 2;

            _btnAddKeywordGroup =
                new Button
                {
                    Name = "btnAddKeywordGroup",
                    Text = "Add Group",
                    AutoSize = false,
                    Width = buttonWidth,
                    Height = 27,
                    Left = lbxKeywordGroups.Left,
                    Top = lbxKeywordGroups.Bottom + 6,
                    Anchor =
                        lbxKeywordGroups.Anchor
                };

            _btnRemoveKeywordGroup =
                new Button
                {
                    Name = "btnRemoveKeywordGroup",
                    Text = "Remove Group",
                    AutoSize = false,
                    Width = buttonWidth,
                    Height = 27,
                    Left =
                        lbxKeywordGroups.Left
                        + buttonWidth
                        + buttonSpacing,
                    Top = lbxKeywordGroups.Bottom + 6,
                    Anchor =
                        lbxKeywordGroups.Anchor,
                    Enabled = false
                };

            _btnAddKeywordGroup.Click +=
                btnAddKeywordGroup_Click;

            _btnRemoveKeywordGroup.Click +=
                btnRemoveKeywordGroup_Click;

            Control parent =
                lbxKeywordGroups.Parent;

            parent.Controls.Add(
                _btnAddKeywordGroup);

            parent.Controls.Add(
                _btnRemoveKeywordGroup);

            _btnAddKeywordGroup.BringToFront();
            _btnRemoveKeywordGroup.BringToFront();
        }

        private void btnAddKeywordGroup_Click(
            object sender,
            EventArgs e)
        {
            KeywordGroupConfiguration newGroup =
                _languageEditor.AddGroup();

            if (newGroup == null)
            {
                MessageBox.Show(
                    "Load a language configuration before adding a group.",
                    "Keyword Group",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            RefreshKeywordGroups(
                newGroup.Id);

            UpdateWindowTitle();

            txtGroupName.Focus();
            txtGroupName.SelectAll();
        }

        private void btnRemoveKeywordGroup_Click(
            object sender,
            EventArgs e)
        {
            KeywordGroupConfiguration selectedGroup =
                _languageEditor.SelectedGroup;

            if (selectedGroup == null)
            {
                return;
            }

            string groupName =
                string.IsNullOrWhiteSpace(
                    selectedGroup.DisplayName)
                    ? "Group " + selectedGroup.Id
                    : selectedGroup.DisplayName;

            int wordCount =
                selectedGroup.Words?.Count ?? 0;

            int regexCount =
                selectedGroup.Regex?.Count ?? 0;

            DialogResult result =
                MessageBox.Show(
                    "Remove the selected keyword group?"
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Group: "
                    + groupName
                    + Environment.NewLine
                    + "ID: "
                    + selectedGroup.Id
                    + Environment.NewLine
                    + "Words: "
                    + wordCount
                    + Environment.NewLine
                    + "Regex: "
                    + regexCount
                    + Environment.NewLine
                    + Environment.NewLine
                    + "This action cannot be undone.",
                    "Remove Group",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return;
            }

            KeywordGroupConfiguration nextGroup =
                _languageEditor.RemoveSelectedGroup();

            RefreshKeywordGroups(
                nextGroup?.Id);

            UpdateWindowTitle();
            UpdateGroupManagementState();
        }

        private void UpdateGroupManagementState()
        {
            if (_btnAddKeywordGroup != null)
            {
                _btnAddKeywordGroup.Enabled =
                    _languageEditor.HasConfiguration;
            }

            if (_btnRemoveKeywordGroup != null)
            {
                _btnRemoveKeywordGroup.Enabled =
                    _languageEditor.HasSelectedGroup;
            }
        }

        private void btnTestRoundTrip_Click(
            object sender,
            EventArgs e)
        {
            TestPythonLanguageRoundTrip();
        }

        /// <summary>
        /// Executes a read-map-write-read round-trip test for python.lang
        /// without modifying the original language definition file.
        /// </summary>
        private void TestPythonLanguageRoundTrip()
        {
            string pythonLanguagePath =
                Path.Combine(
                    PathManager.LanguagesFolder,
                    "python.lang");

            var tester =
                new HighlightLanguageRoundTripTester();

            try
            {
                RoundTripTestResult result =
                    tester.Test(
                        pythonLanguagePath);

                if (result.IsEquivalent)
                {
                    MessageBox.Show(
                        "Round trip completed successfully."
                        + Environment.NewLine
                        + Environment.NewLine
                        + "Generated file:"
                        + Environment.NewLine
                        + result.GeneratedFilePath,
                        "Language Round Trip",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }

                string differences =
                    result.Differences == null ||
                    result.Differences.Count == 0
                        ? "No detailed differences were reported."
                        : string.Join(
                            Environment.NewLine,
                            result.Differences);

                MessageBox.Show(
                    "The round trip completed, but differences were found."
                    + Environment.NewLine
                    + Environment.NewLine
                    + differences
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Generated file:"
                    + Environment.NewLine
                    + result.GeneratedFilePath,
                    "Language Round Trip",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "The round trip test failed."
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception,
                    "Language Round Trip",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // Metodo temporal 

        private void TestLanguageEditorService()
        {
            var service =
                new NoteHighlightAddin.Highlighting.KeywordGroups.Services
                    .LanguageEditorService();

            try
            {
                EditableLanguageConfiguration configuration =
                    service.Load(
                        "python");

                string destinationPath =
                    Path.Combine(
                        Path.GetTempPath(),
                        "python.editor-service-test.lang");

                service.SaveAs(
                    configuration,
                    destinationPath);

                MessageBox.Show(
                    "LanguageEditorService test completed successfully."
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Language:"
                    + Environment.NewLine
                    + configuration.Language
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Groups loaded:"
                    + Environment.NewLine
                    + configuration.Groups.Count
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Generated file:"
                    + Environment.NewLine
                    + destinationPath,
                    "Language Editor Service",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "LanguageEditorService test failed."
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception,
                    "Language Editor Service",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static string GetLanguageDefinitionName(
    LanguageInfo language)
        {
            if (language == null)
            {
                throw new ArgumentNullException(
                    nameof(language));
            }

            string tag =
                language.Tag?.Trim();

            if (!string.IsNullOrWhiteSpace(tag))
            {
                string tagFilePath =
                    Path.Combine(
                        PathManager.LanguagesFolder,
                        Path.GetFileNameWithoutExtension(tag) + ".lang");

                if (File.Exists(tagFilePath))
                {
                    return tag;
                }
            }

            string label =
                language.Label?.Trim();

            if (!string.IsNullOrWhiteSpace(label))
            {
                string normalizedLabel =
                    label.ToLowerInvariant();

                string labelFilePath =
                    Path.Combine(
                        PathManager.LanguagesFolder,
                        normalizedLabel + ".lang");

                if (File.Exists(labelFilePath))
                {
                    return normalizedLabel;
                }
            }

            throw new FileNotFoundException(
                "No matching .lang file was found for the selected language.");
        }
        private void LoadSelectedLanguageConfiguration()
        {
            var selectedLanguage =
                lbxLanguages.SelectedItem as LanguageInfo;

            if (selectedLanguage == null)
            {
                _languageEditor.Clear();
                RefreshKeywordGroups();

                return;
            }

            try
            {
                string languageName =
                    GetLanguageDefinitionName(
                        selectedLanguage);

                _languageEditor.Load(languageName);

                RefreshKeywordGroups();
            }
            catch (Exception exception)
            {
                _languageEditor.Clear();
                RefreshKeywordGroups();

                MessageBox.Show(
                    "The selected language configuration could not be loaded."
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Label: "
                    + selectedLanguage.Label
                    + Environment.NewLine
                    + "Tag: "
                    + selectedLanguage.Tag
                    + Environment.NewLine
                    + "Languages folder: "
                    + PathManager.LanguagesFolder
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception.Message,
                    "Language Configuration",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /*private void LoadSelectedLanguageConfiguration()
        {
            var selectedLanguage =
                lbxLanguages.SelectedItem as LanguageInfo;

            if (selectedLanguage == null)
            {
                _currentLanguageConfiguration = null;
                RefreshKeywordGroups();

                return;
            }

            try
            {
                _currentLanguageConfiguration =
                    _languageEditorService.Load(
                        selectedLanguage.Tag);
                RefreshKeywordGroups();

                MessageBox.Show(
                    "Language configuration loaded successfully."
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Language:"
                    + Environment.NewLine
                    + _currentLanguageConfiguration.Language
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Description:"
                    + Environment.NewLine
                    + _currentLanguageConfiguration.Description
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Groups:"
                    + Environment.NewLine
                    + _currentLanguageConfiguration.Groups.Count,
                    "Language Configuration",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                _currentLanguageConfiguration = null;
                RefreshKeywordGroups();

                MessageBox.Show(
                    "The selected language configuration could not be loaded."
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception.Message,
                    "Language Configuration",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        */
        private void lbxLanguages_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSelectedLanguageConfiguration();
        }

        // fin del metodo temporal 

        // load groups 

        private void RefreshKeywordGroups(
            int? groupIdToSelect = null)
        {
            lbxKeywordGroups.BeginUpdate();

            try
            {
                lbxKeywordGroups.Items.Clear();
                lbxGroupWords.Items.Clear();

                _languageEditor.SelectGroup(-1);
                RefreshSelectedGroupDetails();

                if (!_languageEditor.HasConfiguration)
                {
                    return;
                }

                foreach (KeywordGroupConfiguration group
                in _languageEditor.GetOrderedGroups())
                {
                    lbxKeywordGroups.Items.Add(
                        new KeywordGroupListItem(group));
                }

                if (lbxKeywordGroups.Items.Count == 0)
                {
                    return;
                }

                int selectedIndex = 0;

                if (groupIdToSelect.HasValue)
                {
                    for (int index = 0;
                        index < lbxKeywordGroups.Items.Count;
                        index++)
                    {
                        var item =
                            lbxKeywordGroups.Items[index]
                            as KeywordGroupListItem;

                        if (item?.Group.Id ==
                            groupIdToSelect.Value)
                        {
                            selectedIndex = index;
                            break;
                        }
                    }
                }

                lbxKeywordGroups.SelectedIndex =
                    selectedIndex;
            }
            finally
            {
                lbxKeywordGroups.EndUpdate();

                UpdateGroupManagementState();
            }
        }

        private void InitializeLanguageManager()
        {
            try
            {
                _languageManager =
                    new LanguageManager(
                        PathManager.Ribbon,
                        PathManager.LanguagesFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error initializing language manager: {ex.Message}");
            }
        }



        private void BtnFont_Click(
            object sender,
            EventArgs e)
        {
            fontDialog1.Font =
                new Font(
                    NoteHighlightForm.Properties.Settings.Default.Font,
                    NoteHighlightForm.Properties.Settings.Default.FontSize);

            if (fontDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            btnFont.Text =
                "Font:" +
                fontDialog1.Font.Name +
                ", Size:" +
                fontDialog1.Font.Size;

            btnFont.Font =
                fontDialog1.Font;

            NoteHighlightForm.Properties.Settings.Default.Font =
                fontDialog1.Font.Name;

            NoteHighlightForm.Properties.Settings.Default.FontSize =
                (int)Math.Round(
                    fontDialog1.Font.Size);

            NoteHighlightForm.Properties.Settings.Default.Save();
        }

        private void ChShowTableBorder_CheckedChanged(
            object sender,
            EventArgs e)
        {
            NoteHighlightForm.Properties.Settings.Default.ShowTableBorder =
                cbShowTableBorder.Checked;

            NoteHighlightForm.Properties.Settings.Default.Save();
        }

        private void SettingsForm_Shown(
            object sender,
            EventArgs e)
        {
            // Required for SetForegroundWindow to work consistently.
            WindowState =
                FormWindowState.Minimized;

            WindowState =
                FormWindowState.Normal;

            SetForegroundWindow(
                Handle);

            RefreshLanguageList();
        }

        private void RefreshLanguageList()
        {
            if (_languageManager == null)
            {
                return;
            }

            try
            {
                lbxLanguages.Items.Clear();

                var visibleLanguages =
                    _languageManager.GetVisibleLanguages();

                foreach (var language in visibleLanguages)
                {
                    lbxLanguages.Items.Add(
                        language);
                }

                cmbAvailableLanguages.Items.Clear();

                var availableLanguages =
                    _languageManager.GetAvailableLanguages();

                var visibleTags =
                    visibleLanguages
                        .Select(language => language.Tag)
                        .ToList();

                foreach (var language in availableLanguages)
                {
                    if (!visibleTags.Contains(language.Tag))
                    {
                        cmbAvailableLanguages.Items.Add(
                            language);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error refreshing language list: {ex.Message}");
            }
        }

        private void BtnRemoveLanguage_Click(
            object sender,
            EventArgs e)
        {
            var selectedLanguage =
                lbxLanguages.SelectedItem as LanguageInfo;

            if (selectedLanguage == null)
            {
                MessageBox.Show(
                    "Please select a language to remove.");

                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    $"Remove '{selectedLanguage.Label}' from the ribbon?",
                    "Confirm Removal",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            if (_languageManager.RemoveLanguage(selectedLanguage.Tag))
            {
                MessageBox.Show(
                    "Language removed. Please restart OneNote to see changes.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                RefreshLanguageList();

                return;
            }

            MessageBox.Show(
                "Failed to remove language.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private void BtnAddLanguage_Click(
            object sender,
            EventArgs e)
        {
            var selectedLanguage =
                cmbAvailableLanguages.SelectedItem as LanguageInfo;

            if (selectedLanguage == null)
            {
                MessageBox.Show(
                    "Please select a language to add.");

                return;
            }

            if (_languageManager.AddLanguage(
                selectedLanguage.Tag,
                selectedLanguage.Label))
            {
                MessageBox.Show(
                    "Language added to ribbon. Please restart OneNote to see changes.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                RefreshLanguageList();

                return;
            }

            MessageBox.Show(
                "Failed to add language.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private void lblAddLanguage_Click(
            object sender,
            EventArgs e)
        {
        }

        private void lbxKeywordGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshSelectedGroupWords();
        }

        private void RefreshSelectedGroupWords()
        {
            lbxGroupWords.Items.Clear();

            var selectedItem =
                lbxKeywordGroups.SelectedItem
                as KeywordGroupListItem;

            if (selectedItem == null)
            {
                _languageEditor.SelectGroup(
                    -1);

                RefreshSelectedGroupDetails();
                UpdateWordEditorState();
                UpdateGroupManagementState();

                return;
            }

            _languageEditor.SelectGroup(
                selectedItem.Group.Id);

            foreach (string word
                in _languageEditor.GetSelectedGroupWords())
            {
                lbxGroupWords.Items.Add(
                    word);
            }

            RefreshSelectedGroupDetails();
            UpdateWordEditorState();
            UpdateGroupManagementState();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void RefreshSelectedGroupDetails()
        {
            _isLoadingGroupDetails =
                true;

            try
            {
                KeywordGroupConfiguration group =
                    _languageEditor.SelectedGroup;

                bool hasGroup =
                    group != null;

                txtGroupName.Enabled =
                    hasGroup;

                txtGroupDescription.Enabled =
                    hasGroup;

                nudGroupPriority.Enabled =
                    hasGroup;

                chkGroupVisible.Enabled =
                    hasGroup;

                chkGroupBold.Enabled =
                    hasGroup;

                chkGroupItalic.Enabled =
                    hasGroup;

                txtGroupColour.Enabled =
                    hasGroup;

                if (!hasGroup)
                {
                    txtGroupName.Clear();
                    txtGroupDescription.Clear();

                    nudGroupPriority.Value = 0;

                    chkGroupVisible.Checked = false;
                    chkGroupBold.Checked = false;
                    chkGroupItalic.Checked = false;

                    txtGroupColour.Clear();

                    return;
                }

                txtGroupName.Text =
                    group.DisplayName ?? string.Empty;

                txtGroupDescription.Text =
                    group.Description ?? string.Empty;

                decimal priority =
                    group.Priority;

                if (priority < nudGroupPriority.Minimum)
                {
                    priority =
                        nudGroupPriority.Minimum;
                }

                if (priority > nudGroupPriority.Maximum)
                {
                    priority =
                        nudGroupPriority.Maximum;
                }

                nudGroupPriority.Value =
                    priority;

                chkGroupVisible.Checked =
                    group.Visible;

                chkGroupBold.Checked =
                    group.Bold;

                chkGroupItalic.Checked =
                    group.Italic;

                txtGroupColour.Text =
                    group.Colour ?? string.Empty;
            }
            finally
            {
                _isLoadingGroupDetails =
                    false;
            }
        }

        private void ApplySelectedGroupChanges()
        {
            if (_isLoadingGroupDetails)
            {
                return;
            }

            KeywordGroupConfiguration group =
                _languageEditor.SelectedGroup;

            if (group == null)
            {
                return;
            }

            group.DisplayName =
                txtGroupName.Text.Trim();

            group.Description =
                string.IsNullOrWhiteSpace(
                    txtGroupDescription.Text)
                    ? null
                    : txtGroupDescription.Text.Trim();

            group.Priority =
                Decimal.ToInt32(
                    nudGroupPriority.Value);

            group.Visible =
                chkGroupVisible.Checked;

            group.Bold =
                chkGroupBold.Checked;

            group.Italic =
                chkGroupItalic.Checked;

            group.Colour =
                string.IsNullOrWhiteSpace(
                    txtGroupColour.Text)
                    ? null
                    : txtGroupColour.Text.Trim();

            _languageEditor.MarkAsModified();

            RefreshSelectedGroupListItem();
            UpdateWindowTitle();
        }

        private void RefreshSelectedGroupListItem()
        {
            int selectedIndex =
                lbxKeywordGroups.SelectedIndex;

            if (selectedIndex < 0)
            {
                return;
            }

            var selectedItem =
                lbxKeywordGroups.SelectedItem
                as KeywordGroupListItem;

            if (selectedItem == null)
            {
                return;
            }

            lbxKeywordGroups.Items[selectedIndex] =
                new KeywordGroupListItem(
                    selectedItem.Group);

            lbxKeywordGroups.SelectedIndex =
                selectedIndex;
        }

        private void UpdateWindowTitle()
        {
            Text =
                _languageEditor.HasUnsavedChanges
                    ? "SettingsForm *"
                    : "SettingsForm";
        }

        private void txtGroupName_TextChanged(object sender, EventArgs e)
        {
            ApplySelectedGroupChanges();
        }

        private void txtGroupDescription_TextChanged(object sender, EventArgs e)
        {
            ApplySelectedGroupChanges();
        }

        private void nudGroupPriority_ValueChanged(object sender, EventArgs e)
        {
            ApplySelectedGroupChanges();
        }

        private void chkGroupVisible_CheckedChanged(object sender, EventArgs e)
        {
            ApplySelectedGroupChanges();
        }

        private void chkGroupBold_CheckedChanged(object sender, EventArgs e)
        {
            ApplySelectedGroupChanges();
        }

        private void chkGroupItalic_CheckedChanged(object sender, EventArgs e)
        {
            ApplySelectedGroupChanges();
        }

        private void txtGroupColour_TextChanged(object sender, EventArgs e)
        {
            ApplySelectedGroupChanges();
        }

        // Add Word button 

        private void btnAddGroupWord_Click(
    object sender,
    EventArgs e)
        {
            if (!_languageEditor.HasSelectedGroup)
            {
                MessageBox.Show(
                    "Select a keyword group first.",
                    "Keyword Group",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            string word =
                txtNewGroupWord.Text.Trim();

            if (string.IsNullOrWhiteSpace(word))
            {
                return;
            }

            WordLocationResult location =
                _languageEditor.FindWord(
                    word);

            if (location.Exists)
            {
                if (ReferenceEquals(
                    location.Group,
                    _languageEditor.SelectedGroup))
                {
                    MessageBox.Show(
                        "The word already exists in the selected group.",
                        "Keyword Group",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    SelectWordInList(
                        word);

                    return;
                }

                string sourceGroupName =
                    string.IsNullOrWhiteSpace(
                        location.Group.DisplayName)
                        ? "Group " + location.Group.Id
                        : location.Group.DisplayName;

                string destinationGroupName =
                    string.IsNullOrWhiteSpace(
                        _languageEditor.SelectedGroup.DisplayName)
                        ? "Group "
                            + _languageEditor.SelectedGroup.Id
                        : _languageEditor.SelectedGroup.DisplayName;

                DialogResult result =
                    MessageBox.Show(
                        "The word already exists in another group."
                        + Environment.NewLine
                        + Environment.NewLine
                        + "Word: "
                        + word
                        + Environment.NewLine
                        + "Current group: "
                        + sourceGroupName
                        + Environment.NewLine
                        + "Destination group: "
                        + destinationGroupName
                        + Environment.NewLine
                        + Environment.NewLine
                        + "Do you want to move it?",
                        "Move Word",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    return;
                }

                bool moved =
                    _languageEditor.MoveWordToSelectedGroup(
                        word,
                        location.Group);

                if (!moved)
                {
                    MessageBox.Show(
                        "The word could not be moved.",
                        "Move Word",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                txtNewGroupWord.Clear();

                RefreshSelectedGroupWords();

                SelectWordInList(
                    word);

                UpdateWindowTitle();

                return;
            }

            bool added =
                _languageEditor.AddWord(
                    word);

            if (!added)
            {
                return;
            }

            txtNewGroupWord.Clear();

            RefreshSelectedGroupWords();

            SelectWordInList(
                word);

            UpdateWindowTitle();
        }

        // Remove Word button

        private void btnRemoveGroupWord_Click(
    object sender,
    EventArgs e)
        {
            string selectedWord =
                lbxGroupWords.SelectedItem
                as string;

            if (string.IsNullOrWhiteSpace(selectedWord))
            {
                return;
            }

            DialogResult result =
                MessageBox.Show(
                    "Remove the selected word?"
                    + Environment.NewLine
                    + Environment.NewLine
                    + selectedWord,
                    "Remove Word",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            bool removed =
                _languageEditor.RemoveWord(
                    selectedWord);

            if (!removed)
            {
                return;
            }

            RefreshSelectedGroupWords();
        }

        // Choose added word method 

        private void SelectWordInList(string word)
        {
            for (int index = 0;
                index < lbxGroupWords.Items.Count;
                index++)
            {
                string currentWord =
                    lbxGroupWords.Items[index]
                    as string;

                if (string.Equals(
                    currentWord,
                    word,
                    StringComparison.Ordinal))
                {
                    lbxGroupWords.SelectedIndex =
                        index;

                    return;
                }
            }
        }

        // Enables and Disabled buttons 

        private void UpdateWordEditorState()
        {
            bool hasGroup =
                _languageEditor != null &&
                _languageEditor.HasSelectedGroup;

            txtNewGroupWord.Enabled =
                hasGroup;

            btnAddGroupWord.Enabled =
                hasGroup;

            btnRemoveGroupWord.Enabled =
                hasGroup &&
                lbxGroupWords.SelectedIndex >= 0;
        }

        private void lbxGroupWords_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateWordEditorState();
        }

        // Use enter to add word 

        private void txtNewGroupWord_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            btnAddGroupWord.PerformClick();

            e.SuppressKeyPress = true;
            e.Handled = true;
        }

    }
}