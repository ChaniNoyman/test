namespace test.Models
{
    public class ReviewModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}
