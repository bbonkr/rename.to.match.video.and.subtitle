using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleFileRename.Services.Models
{
    public class RenameRequestModel
    {
        public string Pattern { get; set; }

        public string Replacement { get; set; }

        public IDictionary<GroupKey, TargetFile[]> Files { get; set; }
    }

    public class GroupKey
    {
        public string Key { get; set; }

        public ContentType ContentType { get; set; }

        public override string ToString()
        {
            return $"{ContentType}: {Key}";
        }
    }

    public class TargetFile
    {
        public string Name { get; set; }

        public string Extension { get; set; }

        public string Path { get; set; }

        public string CandidateName { get; set; }

        public string OriginalName { get; set; }

        public bool? Succeed { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            return System.IO.Path.Join(Path, $"{Name}{Extension}");
        }
    }

    public enum ContentType
    {
        Video,
        Subtitle,
        Unknown,
    }
}
