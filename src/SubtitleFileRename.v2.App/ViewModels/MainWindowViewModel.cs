using Microsoft.Win32;

using SubtitleFileRename.Services;
using SubtitleFileRename.Services.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SubtitleFileRename.v2.App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(RenameService renameService)
        {
            this.renameService = renameService;

            Title = "File Rename v2";
        }

        private RenameRequestModel data;
        public RenameRequestModel Data
        {
            get => data;
            set => SetProperty(ref data, value, overwrite:true);
        }

        private bool isBusy = false;
        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        private string pattern = string.Empty;
        public string Pattern
        {
            get => pattern;
            set => SetProperty(ref pattern, value, onChanged: (changedValue) =>
            {
                PreviewEnabled = !string.IsNullOrWhiteSpace(changedValue);
                RenameEnabled = false;
                RestoreEnabled = false;
            });
        }

        private string replacement = string.Empty;
        public string Replacement
        {
            get => replacement;
            set => SetProperty(ref replacement, value, onChanged: (changedValue) =>
            {
                RenameEnabled = false;
                RestoreEnabled = false;
            });
        }

        private string title = "App";
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        private bool previewEnabled = true;
        public bool PreviewEnabled
        {
            get => previewEnabled;
            set => SetProperty(ref previewEnabled, value, onChanged:(changedValue)=> {
                if (!changedValue)
                {
                    RenameEnabled = false;
                    RestoreEnabled = false;
                }
            });
        }

        private bool renameEnabled = true;
        public bool RenameEnabled
        {
            get => renameEnabled;
            set => SetProperty(ref renameEnabled, value);
        }

        private bool restoreEnabled = true;
        public bool RestoreEnabled
        {
            get => restoreEnabled;
            set => SetProperty(ref restoreEnabled, value);
        }

        public ICommand OpenFileCommand { get => new BindableCommand(OpenFile); }

        public ICommand PreviewCommand { get => new BindableCommand(Preview, CanPreview); }

        public ICommand RenameCommand { get => new BindableCommand(Rename, CanRename); }

        public ICommand RestoreCommand { get => new BindableCommand(Restore, CanRestore); }

        private async void OpenFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.ShowReadOnly = true;
            dialog.ReadOnlyChecked = true;

             var result = dialog.ShowDialog();

            if(result.HasValue && result.Value)
            {
                if (dialog.FileNames.Length > 0)
                {
                    var files = dialog.FileNames.Select(f => new FileInfo(f));
                    var renameRequestModel = await renameService.LoadFilesAsync(files).ConfigureAwait(false);

                    if (renameRequestModel != null)
                    {
                        Data = renameRequestModel;
                    }

                    PreviewEnabled = !string.IsNullOrWhiteSpace(Pattern);
                    RenameEnabled = false;
                    RestoreEnabled = false;
                }
            }

        }

        private async void Preview()
        {
            try
            {
                IsBusy = true;

                var updated = await renameService.PreviewAsync(Data, Pattern, Replacement).ConfigureAwait(false);
                
                Data = updated;

                RenameEnabled = true;
            }
            finally
            {
                IsBusy = false;
            }          
        }

        private bool CanPreview()
        {
            if (Data == null)
            {
                return false;
            }

            if( Data.Files.Count == 0)
            {
                return false;
            }

            if (!PreviewEnabled)
            {
                return false;
            }

            return true;
        }

        private async void Rename()
        {
            try
            {
                IsBusy = true;

                var updated = await renameService.RenameAsync(Data);

                Data = updated;

                RenameEnabled = false;
                RestoreEnabled = true;
            }
            finally
            {
                IsBusy = false;
            }       
        }

        private bool CanRename()
        {
            if(Data == null)
            {
                return false;
            }

            if(data.Files.Count == 0)
            {
                return false;
            }

            if (!RenameEnabled)
            {
                return false;
            }

            return true;
        }

        private async void Restore()
        {
            IsBusy = true;
            try
            {
                var updated = await renameService.RollbackAsync(Data);

                Data = updated;

                RenameEnabled = true;
                RestoreEnabled = false;
            }
            finally
            {
                IsBusy = false;
            }       
        }
        
        private bool CanRestore()
        {
            if (Data == null)
            {
                return false;
            }

            if (data.Files.Count == 0)
            {
                return false;
            }

            if (!RestoreEnabled)
            {
                return false;
            }

            return true;
        }

        private readonly RenameService renameService;
    }
}
