namespace ThreeL.Blob.Clients.Win.Dtos
{
    public class UpdateFileObjectLocationDto
    {
        public long[] FileIds { get; set; }
        public long ParentFolder { get; set; }
    }
}
