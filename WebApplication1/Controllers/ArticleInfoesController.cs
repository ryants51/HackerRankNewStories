using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using WebApplication1.Models;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleInfoesController : ControllerBase
    {
        private const string ArticleCacheKey = "article-cache-key";
        private readonly IMemoryCache _cache;
        private readonly ArticleInfoContext _context;

      public ArticleInfoesController(ArticleInfoContext context, IMemoryCache cache)
      {
         _context = context;
         _cache = cache;
      }

      // GET: api/ArticleInfoes
      [HttpGet]
      public IEnumerable<ArticleInfo> Get()
      {
         // If the articles are cached, retrieve from cache
         if (_cache.TryGetValue(ArticleCacheKey, out IEnumerable<ArticleInfo> articles))
         {
            return articles;
         }

         string responseBody = String.Empty;
         string[] charsToRemove = new string[] { "[", "]", "," };
         string url = @"https://hacker-news.firebaseio.com/v0/newstories.json?print=pretty";

         // Get the new story IDs from HackerRank
         HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
         request.AutomaticDecompression = DecompressionMethods.GZip;

         using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
         using (Stream stream = response.GetResponseStream())
         using (StreamReader reader = new StreamReader(stream))
         {
            responseBody = reader.ReadToEnd();
         }

         // Scrub the response of all characters but numbers and spaces
         foreach (var c in charsToRemove)
         {
            responseBody = responseBody.Replace(c, string.Empty);
         }

         // Puts all the numbers individually into a list and Removes the empty first and last elements
         List<string> articleIDs = responseBody.Split(' ').ToList();
         articleIDs.RemoveAt(0);
         articleIDs.RemoveAt(articleIDs.Count - 1);

         var result = Enumerable.Range(0, articleIDs.Count).Select(index => new ArticleInfo
         {
            Id = Convert.ToInt64(articleIDs[index])
         })
         .ToArray();

         // Cache the result
         _cache.Set(ArticleCacheKey, result);

         return result;
      }

      // GET: api/ArticleInfoes/5
      [HttpGet("{id}")]
        public async Task<ActionResult<ArticleInfo>> GetArticleInfo(long id)
        {
            var articleInfo = await _context.TodoItems.FindAsync(id);

            if (articleInfo == null)
            {
                return NotFound();
            }

            return articleInfo;
        }

        // PUT: api/ArticleInfoes/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticleInfo(long id, ArticleInfo articleInfo)
        {
            if (id != articleInfo.Id)
            {
                return BadRequest();
            }

            _context.Entry(articleInfo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleInfoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ArticleInfoes
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<ArticleInfo>> PostArticleInfo(ArticleInfo articleInfo)
        {
            _context.TodoItems.Add(articleInfo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetArticleInfo", new { id = articleInfo.Id }, articleInfo);
        }

        // DELETE: api/ArticleInfoes/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ArticleInfo>> DeleteArticleInfo(long id)
        {
            var articleInfo = await _context.TodoItems.FindAsync(id);
            if (articleInfo == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(articleInfo);
            await _context.SaveChangesAsync();

            return articleInfo;
        }

        private bool ArticleInfoExists(long id)
        {
            return _context.TodoItems.Any(e => e.Id == id);
        }
    }
}
