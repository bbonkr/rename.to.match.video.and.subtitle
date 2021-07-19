using Microsoft.Extensions.Logging;
using SubtitleFileRename.Services.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;

namespace SubtitleFileRename.Services
{
    public class RenameService
    {
        public RenameService(ILogger<RenameService> logger)
        {
            this.logger = logger;
        }

        public Task<RenameRequestModel> LoadFilesAsync(IEnumerable<FileInfo> files)
        {            
            var targetFiles = files.Select(x => MapFromFileInfoToTargetFile(x))
                .GroupBy(x => x.Extension)
                .ToDictionary(x => new GroupKey { 
                    Key=  x.Key ,
                    ContentType = GetContentType(x.Key)
                }, x => x.Select(y => y).OrderBy(y => y.Name).ToArray());

            return Task.FromResult(new RenameRequestModel
            {
                Files = targetFiles,
            });
        }

        private TargetFile MapFromFileInfoToTargetFile(FileInfo fileInfo)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName);
            return new TargetFile
            {
                Name = fileNameWithoutExtension,
                Extension = fileInfo.Extension,
                Path = fileInfo.DirectoryName,
                OriginalName = fileNameWithoutExtension,
            };
        }

        public Task<RenameRequestModel> PreviewAsync(RenameRequestModel model, string pattern, string replacement, CancellationToken cancellationToken = default)
        {
            var message = string.Empty;

            Validate(model);

            var movieFileItem = model.Files.Where(x => x.Key.ContentType == ContentType.Video).FirstOrDefault();

            if (movieFileItem.Key == null)
            {
                message = $"Does not load movie file. Rename is based on movie file name.";
                throw new Exception(message);
            }

            var fileCount = movieFileItem.Value.Count();

            for (var i = 0; i < fileCount; i++)
            {
                var movieFile = movieFileItem.Value[i];

                var newFileName = Regex.Replace(movieFile.Name, pattern, replacement);

                movieFile.CandidateName = newFileName;
                movieFile.Succeed = null;
                movieFile.Message = string.Empty;

                foreach (var item in model.Files.Where(x => x.Key.ContentType != ContentType.Video))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return Task.FromCanceled<RenameRequestModel>(cancellationToken);
                    }

                    var someFile = item.Value[i];

                    someFile.CandidateName = newFileName;
                    someFile.Succeed = null;
                    someFile.Message = string.Empty;
                }
            }

            model.Pattern = pattern;
            model.Replacement = replacement;

            return Task.FromResult(model);
        }

        public Task<RenameRequestModel> ResetPreviewAsync(RenameRequestModel model, CancellationToken cancellationToken = default)
        {
            model.Pattern = string.Empty;
            model.Replacement = string.Empty;

            foreach (var item in model.Files)
            {
                foreach (var value in item.Value)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return Task.FromCanceled<RenameRequestModel>(cancellationToken);
                    }
                    
                    value.Succeed = null;
                    value.Message = null;
                }
            }

            return Task.FromResult(model);
        }

        public Task<RenameRequestModel> RenameAsync(RenameRequestModel model, CancellationToken cancellationToken=default)
        {
            RenameValidate(model);

            foreach (var item in model.Files)
            {
                foreach (var value in item.Value)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return Task.FromCanceled<RenameRequestModel>(cancellationToken);
                    }

                    try
                    {
                        var source = Path.Join(value.Path, $"{value.Name}{value.Extension}");
                        var destination = Path.Join(value.Path, $"{value.CandidateName}{value.Extension}");

                        File.Move(source, destination);

                        var renamedFileInfo = new FileInfo(destination);
                        var renameTargetFile = MapFromFileInfoToTargetFile(renamedFileInfo);

                        value.Name = renameTargetFile.Name;
                        value.CandidateName = string.Empty;
                        value.Succeed = true;
                        value.Message = "Renamed ✔️";
                    }
                    catch (Exception ex)
                    {
                        value.Succeed = false;
                        value.Message = ex.Message;
                    }
                }
            }

            return Task.FromResult(model);
        }

        public Task<RenameRequestModel> RollbackAsync(RenameRequestModel model, CancellationToken cancellationToken = default)
        {
            RollbackValidate(model);

            foreach (var item in model.Files)
            {
                foreach (var value in item.Value)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return Task.FromCanceled<RenameRequestModel>(cancellationToken);
                    }

                    try
                    {
                        var source = Path.Join(value.Path, $"{value.Name}{value.Extension}");
                        var destination = Path.Join(value.Path, $"{value.OriginalName}{value.Extension}");

                        File.Move(source, destination);

                        var renamedFileInfo = new FileInfo(destination);
                        var renameTargetFile = MapFromFileInfoToTargetFile(renamedFileInfo);

                        value.Name = renameTargetFile.Name;
                        value.CandidateName = string.Empty;
                        value.Succeed = true;
                        value.Message = "Restored ✔️";
                    }
                    catch (Exception ex)
                    {
                        value.Succeed = false;
                        value.Message = ex.Message;
                    }
                }
            }

            return Task.FromResult(model);
        }

        public void Validate(RenameRequestModel model)
        {
            var message = string.Empty;

            var movieFiles = model.Files.Where(x => x.Key.ContentType == ContentType.Video).FirstOrDefault();
           
            if(movieFiles.Key == null)
            {
                message = $"Does not load movie file. Rename is based on movie file name.";
                throw new Exception(message);
                
            }
                var basisCount = movieFiles.Value.Count();

            foreach (var item in model.Files.Where(x => x.Key.ContentType != ContentType.Video))
            {
                if (basisCount != item.Value.Count())
                {
                    message = $"There is item that count does not match. ({item.Key})";
                    throw new Exception(message);
                }
            }            
        }

        public void RenameValidate(RenameRequestModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Pattern))
            {
                throw new Exception("Run preview first.");
            }

            foreach(var item in model.Files)
            {
                foreach (var value in item.Value)
                {
                    if (string.IsNullOrWhiteSpace(value.CandidateName))
                    {
                        throw new Exception("Run preview first.");
                    }
                }
            }   
        }

        public void RollbackValidate(RenameRequestModel model)
        {
            foreach (var item in model.Files)
            {
                foreach (var value in item.Value)
                {
                    if (string.IsNullOrWhiteSpace(value.OriginalName))
                    {
                        throw new Exception("Run rename first.");
                    }
                }
            }
        }

        private ContentType GetContentType(string extension)
        {
            switch (extension.ToLower())
            {
                case ".mp4":
                case ".mkv":
                case ".avi":
                    return ContentType.Video;
                case ".smi":
                case ".srt":
                    return ContentType.Subtitle;
                default:
                    return ContentType.Unknown;
            }
        }

        private readonly ILogger logger;
    }
}
