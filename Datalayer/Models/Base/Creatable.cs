namespace Datalayer.Models.Base
{
    public abstract class Creatable : BaseEntity
    {
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }

        public Creatable()
        {
            CreatedAt = DateTime.Now;
            CreatedBy = Guid.Parse("");
        }
    }
}
