namespace ThreeL.Blob.Application.Contract.Dtos
{
    public class FolderTreeCreationDto
    {
        public long ParentId { get; set; }
        public IEnumerable<FolderTreeCreationItemDto> Items { get; set; }
    }

    public class FolderTreeCreationItemDto
    {
        public string ClientId { get; set; }
        public long RemoteId { get; set; }
        public string? ParentFolderLocation { get; set; }
        public string FolderName { get; set; }
        public string ParentClientId { get; set; }
        public string? Location { get; set; }
        public string TrackPath { get; set; }
        public long ParentId { get; set; }
    }
}
