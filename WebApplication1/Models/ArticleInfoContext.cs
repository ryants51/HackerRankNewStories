using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
   public class ArticleInfoContext : DbContext
   {
      public ArticleInfoContext(DbContextOptions<ArticleInfoContext> options)
          : base(options)
      {
      }

      public DbSet<ArticleInfo> TodoItems { get; set; }
   }
}
