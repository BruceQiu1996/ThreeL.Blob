namespace ThreeL.Blob.Application.Contract.Dtos
{
    public class UpdateFileObjectLocationDto
    {
        public long[] FileIds { get; set; }
        public long ParentFolder { get; set; }
    }
}
