using System.Web.Http;
using System.Web.Http.Description;
using BeatsApi.Models;
using System.Collections.Generic;

namespace BeatsApi.Controllers
{
    public class BeatsDataController : ApiController
    {
        public static List<BeatsData> staticBeatsDataCollections = new List<BeatsData>();

        // GET: api/BeatsData
        [ResponseType(typeof(BeatsData))]
        public IHttpActionResult GetSentimentData(string id)
        {
            var thisBeatsData = staticBeatsDataCollections.Find(o => o.Name.ToLower() == id.ToLower());
            if (thisBeatsData == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(thisBeatsData);
            }
        }

        // POST: api/BeatsData
        [ResponseType(typeof(BeatsData))]
        public IHttpActionResult PostSentimentData(BeatsData data)
        {
            var thisBeatsData = staticBeatsDataCollections.Find(o => o.Name == data.Name);
            if (thisBeatsData == null)
            {
                //create it
                staticBeatsDataCollections.Add(data);
            }
            else
            {
                //update it
                thisBeatsData.Heart = data.Heart;
                thisBeatsData.Temp = data.Temp;
                thisBeatsData.Steps = data.Steps;
            }

            return Created("/api/BeatsData", data);
        }
    }
}