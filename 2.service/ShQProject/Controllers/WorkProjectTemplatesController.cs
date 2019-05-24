using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Dxc.Shq.WebApi.Core;
using Dxc.Shq.WebApi.Models;

namespace Dxc.Shq.WebApi.Controllers
{
    public class WorkProjectTemplatesController : ApiController
    {
        private ShqContext db = new ShqContext();

        // GET: api/WorkProjectTemplates
        public IQueryable<WorkProjectTemplate> GetWorkProjectTemplates()
        {
            return db.WorkProjectTemplates;
        }

        // GET: api/WorkProjectTemplates/5
        [ResponseType(typeof(WorkProjectTemplate))]
        public async Task<IHttpActionResult> GetWorkProjectTemplate(Guid id)
        {
            WorkProjectTemplate workProjectTemplate = await db.WorkProjectTemplates.FindAsync(id);
            if (workProjectTemplate == null)
            {
                return NotFound();
            }

            return Ok(workProjectTemplate);
        }

        //// PUT: api/WorkProjectTemplates/5
        //[ResponseType(typeof(void))]
        //public async Task<IHttpActionResult> PutWorkProjectTemplate(Guid id, WorkProjectTemplate workProjectTemplate)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != workProjectTemplate.Id)
        //    {
        //        return BadRequest();
        //    }

        //    db.Entry(workProjectTemplate).State = EntityState.Modified;

        //    try
        //    {
        //        await db.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!WorkProjectTemplateExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return StatusCode(HttpStatusCode.NoContent);
        //}

        //// POST: api/WorkProjectTemplates
        //[ResponseType(typeof(WorkProjectTemplate))]
        //public async Task<IHttpActionResult> PostWorkProjectTemplate(WorkProjectTemplate workProjectTemplate)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    db.WorkProjectTemplates.Add(workProjectTemplate);

        //    try
        //    {
        //        await db.SaveChangesAsync();
        //    }
        //    catch (DbUpdateException)
        //    {
        //        if (WorkProjectTemplateExists(workProjectTemplate.Id))
        //        {
        //            return Conflict();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return CreatedAtRoute("DefaultApi", new { id = workProjectTemplate.Id }, workProjectTemplate);
        //}

        //// DELETE: api/WorkProjectTemplates/5
        //[ResponseType(typeof(WorkProjectTemplate))]
        //public async Task<IHttpActionResult> DeleteWorkProjectTemplate(Guid id)
        //{
        //    WorkProjectTemplate workProjectTemplate = await db.WorkProjectTemplates.FindAsync(id);
        //    if (workProjectTemplate == null)
        //    {
        //        return NotFound();
        //    }

        //    db.WorkProjectTemplates.Remove(workProjectTemplate);
        //    await db.SaveChangesAsync();

        //    return Ok(workProjectTemplate);
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool WorkProjectTemplateExists(Guid id)
        {
            return db.WorkProjectTemplates.Count(e => e.Id == id) > 0;
        }
    }
}