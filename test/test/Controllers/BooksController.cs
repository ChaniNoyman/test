using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using test.Models;

namespace test.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BooksController> _logger;

        public BooksController(AppDbContext context, ILogger<BooksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        [Authorize] // דורש אימות
        public IActionResult AddBook(BookModel book)
        {
            _logger.LogInformation($"Adding book: {book.Title} by {book.Author}");

            _context.Books.Add(book);
            _context.SaveChanges();

            _logger.LogInformation($"Book added successfully: {book.Title} (ID: {book.Id})");
            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
        }

        [HttpGet]
        [Authorize] // דורש אימות
        public ActionResult<IEnumerable<BookModel>> GetBooks()
        {
            _logger.LogInformation("Getting all books."); 

            var books = _context.Books.ToList();

            _logger.LogInformation($"Found {books.Count} books.");
            return books;
        }

        [HttpGet("{id}")]
        [Authorize] // דורש אימות
        public ActionResult<BookModel> GetBook(int id)
        {
            _logger.LogInformation($"Getting book with ID: {id}");
            var book = _context.Books.Find(id);
            if (book == null)
            {
                _logger.LogWarning($"Book with ID: {id} not found.");
                return NotFound();
            }
            _logger.LogInformation($"Book with ID: {id} found: {book.Title}");
            return book;
        }
    }
}
