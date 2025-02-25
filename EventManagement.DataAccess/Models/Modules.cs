namespace EventManagement.DataAccess.Models
{
    public class Modules
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool Add { get; set; }

        public bool Delete { get; set; }

        public bool Update { get; set; }

        public bool View { get; set; }
    }
}
