using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RecognitionComponent;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Drawing;
using RecognitionComponent;
using System.IO;

namespace Web.Controllers
{
    [ApiController]
    [Route("/api/images")]
    public class ImageController : ControllerBase
    {
        private ImageContext db;
        public ImageController(ImageContext db)
        {
            this.db = db;
        }
        [HttpGet]
        public ImageEntity[] GetImages()
        {
            return db.GetAllImages().ToArray();
        }

        [HttpGet("{id}")]
        public ActionResult<ImageEntity> GetImage(string id)
        {
            int idx = Int32.Parse(id);
            ImageEntity imageEntity = db.TryGetImage(idx);
            if (imageEntity != null)
            {
                return imageEntity;
            }
            else
            {
                return StatusCode(404, "Image with given id is not found");
            }
        }

        [HttpGet("hash/{hash}")]
        public ActionResult<ImageEntity[]> GetImageByHash(string hash)
        {
            int h = Int32.Parse(hash);
            var imageEntity = db.TryGetImageHash(h);
            if (imageEntity != null)
            {
                return imageEntity.ToArray();
            }
            else
            {
                return StatusCode(404, "Image with given hash code is not found");
            }
        }

        [HttpGet("results/{imageEntityId}")]
        public ActionResult<ResultEntity[]> GetResultsByImageEntityId(string imageEntityId)
        {
            int idx = Int32.Parse(imageEntityId);
            return db.GetResultsByImageEntityId(idx);
        }

        [HttpGet("box/{resultEntityId}")]
        public ActionResult<BBox> GetBoxByResultEntityId(string resultEntityId)
        {
            int idx = Int32.Parse(resultEntityId);
            return db.GetBoxByResultEntityId(idx);
        }

        [HttpPost("recognize")]
        public ActionResult<YoloV4Result[]> PostImage([FromBody] byte[] bytes)
        {
            bool isExist = false;
            int hash = ImageEntity.ComputeHashCode(bytes);
            var sameHash = db.TryGetImageHash(hash);
            if (sameHash != null)
            {
                foreach (var entity in sameHash)
                {
                    if (Enumerable.SequenceEqual(entity.Image, bytes))
                    {
                        isExist = true;
                    }
                }
            }
            var ms = new MemoryStream(bytes);
            Bitmap bitmap = new Bitmap(ms);
            var results = ImageRecognition.SingleImageRecognize(bitmap, new System.Threading.CancellationToken());
            if (isExist)
            {
                return StatusCode(200, results.ToArray());
            }
            var imageEntity = new ImageEntity();
            if (results == null)
            {
                return StatusCode(201, new List<YoloV4Result>());
            }
            foreach (var result in results)
            {
                BBox box = new BBox()
                {
                    X1 = result.BBox[0],
                    Y1 = result.BBox[1],
                    X2 = result.BBox[2],
                    Y2 = result.BBox[3]
                };
                ResultEntity resultEntity = new ResultEntity()
                {
                    Confidence = result.Confidence,
                    Label = result.Label
                };
                box.ResultEntity = resultEntity;
                resultEntity.BBox = box;
                imageEntity.Image = bytes;
                imageEntity.HashCode = hash;
                imageEntity.Results.Add(resultEntity);
                db.AddNewBox(box);
                db.AddNewResult(resultEntity);
            }
            db.AddNewImageEntity(imageEntity);
            return StatusCode(201, results.ToArray());
        }

        [HttpDelete("clean")]
        public ActionResult Remove()
        {
            db.Remove();
            return StatusCode(204, "successfully removed");
        }


    }
}
