namespace ThreeL.Blob.Application.Contract.Dtos
{
    public class FolderTreeCreationDto
    {
        public string Id { get; set; }
        public string FolderName { get; set; }
        public long ParentId { get; set; }
        public string ParentFolderId { get; set; }
    }
}
