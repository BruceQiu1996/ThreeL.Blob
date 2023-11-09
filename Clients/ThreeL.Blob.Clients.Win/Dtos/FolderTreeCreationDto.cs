using System;

namespace ThreeL.Blob.Clients.Win.Dtos
{
    public class FolderTreeCreationDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Location { get; set; }
        public string FolderName { get; set; }
        public long ParentId { get; set; }
        public string ParentFolderId { get; set; }
    }
}
