namespace ThreeL.Blob.Infra.Repository.Entities
{
    public interface IBasicAuditInfo<TKey>
    {
        /// <summary>
        /// 创建人
        /// </summary>
        public TKey CreateBy { get; set; }

        /// <summary>
        /// 创建时间/注册时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
