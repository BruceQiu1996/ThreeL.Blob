namespace ThreeL.Blob.Clients.Win.Dtos
{
    public class UploadFileDto
    {
        public string Name { get; set; }
        public long ParentFolder { get; set; }
        public long Size { get; set; }
        public string Code { get; set; }
    }
}
