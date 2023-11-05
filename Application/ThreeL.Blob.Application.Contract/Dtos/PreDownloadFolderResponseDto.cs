namespace ThreeL.Blob.Application.Contract.Dtos
{
    public class PreDownloadFolderResponseDto
    {
        public long Size { get; set; }
        public IEnumerable<PreDownloadFolderFileItemResponseDto> Items { get; set; }
    }

    public class PreDownloadFolderFileItemResponseDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public long ParentFolder { get; set; }
        public bool IsFolder { get; set; }
    }
}
