using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITD.PhuMyPort.API.Helpers;
using ITD.PhuMyPort.Common;
using ITD.PhuMyPort.DataAccess.Dao;
using ITD.PhuMyPort.DataAccess.Data;
using ITD.PhuMyPort.DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITD.PhuMyPort.API.Controllers
{
    public class WorkplacesController : Controller
    {
        public List<Camera> ListAll()
        {
            return _context.Cameras.ToList();
        }
        private readonly ConfigWebContext _context;

        public WorkplacesController(ConfigWebContext context)
        {
            _context = context;
        }

        // GET: Workplaces
      /*  [Authorize]*/
        public async Task<IActionResult> Index()
        {
            var dao = new TransactionDao(_context);
            ViewBag.totalTran = dao.TotalTransection();
            ViewBag.totalPlate = dao.TotalPlate();
            ViewBag.same = dao.Same();
            ViewBag.notsame = dao.Notsame();
            ViewBag.noimg = dao.Noimg();
            ViewBag.unapproved = dao.Unapproved();
            var list = await _context.Workplaces.ToListAsync();
            ViewBag.ListCamera = _context.Cameras.ToList();
            NLogHelper.Info("Workplaces - Show index. Found: " + list.Count + " workplaces.\n");
            return View(list);
        }

        // GET: Workplaces/Create
        [NoDirectAccess]
        public async Task<IActionResult> Create()
        {
            NLogHelper.Info("Workplaces - Create: Show Create popup");
            return View(new Workplace());
        }

        // POST: Workplaces/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Code,Name")] Workplace workplace)
        {
            NLogHelper.Info("Workplaces - Create: Try to create new workplace");
            if (ModelState.IsValid)
            {
                _context.Add(workplace);
                await _context.SaveChangesAsync();
                NLogHelper.Info("Workplaces - Create: Valid infomation. Create new workplace to database.\n");
                return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "_ViewAll", _context.Workplaces.ToList()) });
            }
            NLogHelper.Info("Workplaces - Create: Invalid information");
            return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "Create", workplace) });
        }


        // GET: Workplaces/Edit/5
        [NoDirectAccess]
        public async Task<IActionResult> Edit(string id)
        {
            NLogHelper.Info("Workplaces - Edit: Find camera base on id");
            if (id == null)
            {
                NLogHelper.Info("Workplaces - Edit: Can not find workplace with id: " + id);
                return NotFound();
            }
            var workplace = await _context.Workplaces.FindAsync(id);
            if (workplace == null)
            {
                NLogHelper.Info("Workplaces - Edit: Can not find workplace with id: " + id);
                return NotFound();
            }
            NLogHelper.Info("Workplaces - Edit: Show Edit popup");
            return View(workplace);
        }

        // POST: Workplaces/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Code,Name")] Workplace workplace)
        {
            NLogHelper.Info("Workplaces - Edit: Try to edit workplace");
            if (id != workplace.Code)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(workplace);
                    await _context.SaveChangesAsync();
                    workplace = new Workplace();
                    NLogHelper.Info("Workplaces - Edit: Valid infomation. Update workplace information to database.\n");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkplaceExists(workplace.Code))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "_ViewAll", _context.Workplaces.ToList()) });
            }
            NLogHelper.Info("Workplaces - Edit: InValid infomation.");
            return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "Edit", workplace) });
        }



        // POST: Workplaces/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var workplace = await _context.Workplaces.FindAsync(id);
                _context.Workplaces.Remove(workplace);
                await _context.SaveChangesAsync();
                NLogHelper.Info("Workplaces - Delete: Removed workplace with id: " + id + "\n");
                return Json(new { html = Helper.RenderRazorViewToString(this, "_ViewAll", _context.Workplaces.ToList()) });
            }
            catch
            {
                return NotFound();
            }
        }

        private bool WorkplaceExists(string id)
        {
            return _context.Workplaces.Any(e => e.Code == id);
        }
    }
}