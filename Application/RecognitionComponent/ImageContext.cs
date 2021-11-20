using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RecognitionComponent
{
    public class ImageContext : DbContext
    {
        public DbSet<ImageEntity> ImageEntities { get; set; }
        public DbSet<ResultEntity> ResultEntities { get; set; }
        public DbSet<BBox> BBoxes { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder o)
            => o.UseSqlite("Data Source=images.db");
    }
}
