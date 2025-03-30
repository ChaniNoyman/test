using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using test.Models;
using Microsoft.Extensions.Logging;

namespace test.Controllers
{
    [ApiController]
    [Route("api/books/{bookId}/reviews")]
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(AppDbContext context, ILogger<ReviewsController>logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        [Authorize] // דורש אימות
        public IActionResult AddReview(int bookId, ReviewModel review)
        {
            _logger.LogInformation($"Adding review for book ID: {bookId}");
            var book = _context.Books.Find(bookId);
            if (book == null)
            {
                _logger.LogWarning($"Book with ID: {bookId} not found.");
                return NotFound("Book not found");
            }

            review.BookId = bookId;
            _context.Reviews.Add(review);
            _context.SaveChanges();
            _logger.LogInformation($"Review added successfully for book ID: {bookId}");
            return CreatedAtAction(nameof(GetReviews), new { bookId = bookId }, review);
        }

        [HttpGet]
        public ActionResult<IEnumerable<ReviewModel>> GetReviews(int bookId)
        {
            _logger.LogInformation($"Getting reviews for book ID: {bookId}");
            var book = _context.Books.Find(bookId);
            if (book == null)
            {
                _logger.LogWarning($"Book with ID: {bookId} not found.");
                return NotFound("Book not found");
            }
            
            var reviews = _context.Reviews.Where(r => r.BookId == bookId).ToList();
            _logger.LogInformation($"Found {reviews.Count} reviews for book ID: {bookId}");
            
            return _context.Reviews.Where(r => r.BookId == bookId).ToList();
        }
    }
}
