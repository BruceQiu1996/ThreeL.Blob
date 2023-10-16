namespace ThreeL.Blob.Application.Contract.Dtos
{
    public class FileObjDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long? Size { get; set; }
        public long? ParentFolder { get; set; }
        public DateTime CreateTime { get; set; }
        public bool IsFolder { get; set; }
    }
}
