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
            => o.UseSqlite("Data Source=C:\\Users\\Nastya\\source\\repos\\7sem\\402_dunaeva\\Application\\RecognitionComponent\\images.db");
        public IEnumerable<ImageEntity> GetAllImages()
        {
            return ImageEntities;
        }
        public ImageEntity TryGetImage(int id)
        {
            var queryHash = ImageEntities.Where(entity => entity.ImageEntityId == id);
            if (queryHash.Any())
            {
                return queryHash.ToArray()[0];
            }
            else
            {
                return null;
            }
        }
        public void AddNewImageEntity(ImageEntity newImageEntity)
        {
            ImageEntities.Add(newImageEntity);
            SaveChanges();
        }
        public void AddNewResult(ResultEntity newResultEntity)
        {
            ResultEntities.Add(newResultEntity);
        }
        public void AddNewBox(BBox newBBox)
        {
            BBoxes.Add(newBBox);
        }
        public IEnumerable<ImageEntity> TryGetImageHash(int hash)
        {
            var queryHash = ImageEntities.Where(entity => entity.HashCode == hash);
            if (queryHash.Any())
            {
                return queryHash;
            } 
            else {
                return null;
            }
        }

        public void Remove()
        {
            var query = ImageEntities;
            foreach (var entity in query)
            {
                ImageEntities.Remove(entity);
            }
            SaveChanges();
        }

        public ResultEntity[] GetResultsByImageEntityId(int imageEntityId)
        {
            return ResultEntities.Where(res => res.ImageEntityId == imageEntityId).ToArray();
        }

        public BBox GetBoxByResultEntityId(int resultEntityId)
        {
            return BBoxes.Where(b => b.ResultEntityId == resultEntityId).FirstOrDefault();
        }
    }
}
