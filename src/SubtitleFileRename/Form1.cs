using Microsoft.Extensions.Logging;
using SubtitleFileRename.Services;
using SubtitleFileRename.Services.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SubtitleFileRename
{
    public partial class MainForm : Form
    {
        public MainForm(RenameService renameService, ILogger<MainForm> logger)
        {
            this.renameService = renameService;
            this.logger = logger;

            InitializeComponent();

            openFileButton.Click += OpenFileButton_Click;

            patternTextBox.TextChanged += PatternTextBox_TextChanged;

            previewButton.Click += PreviewButton_Click;
            renameButton.Click += RenameButton_Click;
            rollbackButton.Click += RollbackButton_Click;

            SetPreviewButtonState(false);
            SetRenameButtonState(false);
            SetRollbackButtonState(false);
        }

        private async void RollbackButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (data == null)
                {
                    MessageBox.Show("Please Load files first using Open files button");
                    return;
                }

                if (data != null)
                {
                    renameService.RollbackValidate(data);

                    SetIsLoading(true);

                    data = await renameService.RollbackAsync(data);
                    UpdateList(data);

                    SetPreviewButtonState(false);
                    SetRenameButtonState(false);
                    SetRollbackButtonState(false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            finally
            {
                SetIsLoading(false);
            }
        }

        private async void RenameButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (data == null)
                {
                    MessageBox.Show("Please Load files first using Open files button");
                    return;
                }

                if (data != null)
                {
                    renameService.RenameValidate(data);

                    SetIsLoading(true);

                    data = await renameService.RenameAsync(data);
                    UpdateList(data);

                    SetPreviewButtonState(false);
                    SetRenameButtonState(false);
                    SetRollbackButtonState(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            finally
            {
                SetIsLoading(false);
            }
        }

        private async void PreviewButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (data == null)
                {
                    MessageBox.Show("Please Load files first using Open files button");
                    return;
                }

                if (data != null && patternTextBox.Text.Trim().Length > 0)
                {
                    SetIsLoading(true);

                    data = await renameService.PreviewAsync(data, patternTextBox.Text.Trim(), replacementTextBox.Text.Trim());
                    UpdateList(data);
                    SetRenameButtonState(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            finally
            {
                SetIsLoading(false);
            }
        }

        private void PatternTextBox_TextChanged(object sender, EventArgs e)
        {
            if (sender is TextBox thisControl)
            {
                SetPreviewButtonState(thisControl.Text.Trim().Length > 0);
            }
        }

        private async void OpenFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;

            try
            {
                if (DialogResult.OK == openFileDialog.ShowDialog(this))
                {
                    SetIsLoading(true);

                    ClearRowsTableLayoutPanel();

                    var loadedFiles = await renameService.LoadFilesAsync(openFileDialog.FileNames.Select(x => new FileInfo(x)));

                    renameService.Validate(loadedFiles);

                    var vidoeItem = loadedFiles.Files.Where(x => x.Key.ContentType == Services.Models.ContentType.Video).FirstOrDefault();

                    if (vidoeItem.Key == null)
                    {
                        throw new Exception("You should select video file(s).");
                    }

                    var videoItemListView = CreateListView(vidoeItem);

                    tableLayoutPanel1.Controls.Add(videoItemListView);

                    foreach (var item in loadedFiles.Files.Where(x => x.Key.ContentType != Services.Models.ContentType.Video))
                    {
                        AddRowTableLayoutPanel();

                        var listViewSubtitle = CreateListView(item);

                        tableLayoutPanel1.Controls.Add(listViewSubtitle);
                    }

                    data = loadedFiles;

                    SetPreviewButtonState(false);
                    SetRenameButtonState(false);
                    SetRollbackButtonState(false);
                }
            }
            catch (Exception ex)
            {
                data = null;
                MessageBox.Show(ex.Message, "Error");
            }
            finally
            {
                SetIsLoading(false);
            }
        }

        private ListView CreateListView(KeyValuePair<GroupKey, TargetFile[]>  item)
        {
            var listView = new ListView();
            
            listView.BeginUpdate();

            listView.Name = $"listView_{item.Key.Key}";
            listView.View = View.Details;
            listView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listView.FullRowSelect = true;
            listView.MultiSelect = false;

            listView.Columns.Add("Name", 520);
            listView.Columns.Add("Ext.", 80);
            listView.Columns.Add("Path", 520);
            listView.Columns.Add("Rename Candidate", 520);
            listView.Columns.Add("Original name", 520);
            listView.Columns.Add("Message", 300);

            listView.SelectedIndexChanged += (s, e) =>
            {
                if (s is ListView thisControl)
                {
                    if (thisControl.Focused)
                    {
                        if (thisControl.SelectedItems.Count > 0)
                        {
                            var selectedIndex = thisControl.SelectedItems[0].Index;

                            foreach (var control in tableLayoutPanel1.Controls)
                            {
                                if (control is ListView thatControl)
                                {
                                    if (thatControl.Name != thisControl.Name)
                                    {
                                        thatControl.SelectedItems.Clear();
                                        thatControl.Items[selectedIndex].Selected = true;
                                    }
                                }
                            }
                        }
                    }
                }
            };

            listView.Dock = DockStyle.Fill;
            listView.Scrollable = true;

            UpdateListViewItems(listView, item);

            listView.EndUpdate();
            listView.Refresh();

            return listView;
        }

        private void UpdateListViewItems(ListView listView, KeyValuePair<GroupKey, TargetFile[]> item)
        {
            listView.Items.Clear();

            foreach (var value in item.Value)
            {
                var listitem = new ListViewItem(value.Name);
                
                listitem.SubItems.Add(value.Extension);
                listitem.SubItems.Add(value.Path);
                listitem.SubItems.Add(value.CandidateName);
                listitem.SubItems.Add(value.OriginalName);
                listitem.SubItems.Add(value.Message);

                if (value.Succeed.HasValue && !value.Succeed.Value)
                {
                    listitem.BackColor = Color.Red;
                    listitem.ForeColor = Color.White;
                    listitem.ToolTipText = value.Message;
                }
                else if (value.Succeed.HasValue && value.Succeed.Value)
                {
                    listitem.BackColor = Color.Green;
                    listitem.ForeColor = Color.White;
                }

                listView.Items.Add(listitem);
            }
        }

        private void UpdateList(RenameRequestModel model)
        {
            var movieFiles = model.Files.Where(x => x.Key.ContentType == ContentType.Video).FirstOrDefault();

            if(movieFiles.Key != null)
            {
                foreach(Control control in tableLayoutPanel1.Controls)
                {
                    if(control is ListView listView)
                    {
                        if (listView.Name.EndsWith(movieFiles.Key.Key))
                        {
                            UpdateListViewItems(listView, movieFiles);
                        }
                    }
                }
            }

            foreach(var files in model.Files.Where(x => x.Key.ContentType != ContentType.Video))
            {
                if(files.Key != null)
                {
                    foreach (Control control in tableLayoutPanel1.Controls)
                    {
                        if (control is ListView listView)
                        {
                            if (listView.Name.EndsWith(files.Key.Key))
                            {
                                UpdateListViewItems(listView, files);
                            }
                        }
                    }
                }
            }
        }

        private void ClearRowsTableLayoutPanel()
        {
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.RowCount = 1;
        }

        private void AddRowTableLayoutPanel()
        {
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100.0F / tableLayoutPanel1.RowCount));
            foreach (RowStyle style in tableLayoutPanel1.RowStyles)
            {
                style.SizeType = SizeType.Percent;
                style.Height = 100.0F / tableLayoutPanel1.ColumnCount;
            }
        }

        private void SetPreviewButtonState(bool enabled)
        {
            previewButton.Enabled = enabled;
        }

        private void SetRenameButtonState(bool enabled)
        {
            renameButton.Enabled = enabled;
        }

        private void SetRollbackButtonState(bool enabled)
        {
            rollbackButton.Enabled = enabled;
        }

        private void SetIsLoading(bool isLoading)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetIsLoading(isLoading)));
            }
            else
            {
                if (isLoading)
                {
                    Cursor = Cursors.WaitCursor;
                }
                else
                {
                    Cursor = Cursors.Default;
                }
            }

        }

        private readonly RenameService renameService;
        private readonly ILogger logger;

        private RenameRequestModel data = null;
    }
}
