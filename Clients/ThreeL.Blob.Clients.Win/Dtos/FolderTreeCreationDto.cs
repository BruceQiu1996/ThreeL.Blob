using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ThreeL.Blob.Clients.Win.Dtos
{
    public class FolderTreeCreationDto
    {
        public long ParentId { get; set; }
        public List<FolderTreeCreationItemDto> Items { get; set; }
        public FolderTreeCreationDto()
        {
            Items = new List<FolderTreeCreationItemDto>();
        }
    }

    public class FolderTreeCreationItemDto
    {
        public string ClientId { get; set; } = Guid.NewGuid().ToString();
        public string FolderName { get; set; }
        public string ParentClientId { get; set; }
        [JsonIgnore]
        public string Location { get; set; }
        [JsonIgnore]
        public long RemoteId { get; set; }
        [JsonIgnore]
        public List<string> Files { get; set; }
        public FolderTreeCreationItemDto()
        {
            Files = new List<string>();
        }
    }
}
