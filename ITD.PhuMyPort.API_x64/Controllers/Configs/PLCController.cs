using ITD.PhuMyPort.API.Helpers;
using ITD.PhuMyPort.API.ViewModels;
using ITD.PhuMyPort.Common;
using ITD.PhuMyPort.DataAccess.Data;
using ITD.PhuMyPort.DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ITD.PhuMyPort.API.Controllers
{
    public class PLCController : Controller
    {
        private readonly ConfigWebContext _context;
        private readonly IWebHostEnvironment _env;
        public PLCController(ConfigWebContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Cameras
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var configWebContext = _context.PLCs.Include(c => c.Workplace);
            var list = await configWebContext.ToListAsync();
            NLogHelper.Info("PLC - Show index: Found " + list.Count + " PLCs.\n");
            return View(list);
        }

        // GET: Cameras/Create
        [NoDirectAccess]
        public async Task<IActionResult> Create()
        {
            PLCViewModel plcViewModel = new PLCViewModel();
            plcViewModel.PLC = new PLC();
            plcViewModel.WorkplaceList = new SelectList(_context.Workplaces, "Code", "Code");
            NLogHelper.Info("PLC - Create: Show Create popup");
            return View(plcViewModel);
        }

        // POST: Cameras/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Code,IP,Barrier,WorkplaceCode")] PLC plc)
        {
            NLogHelper.Info("PLC - Create: Try to create new PLC");
            if (ModelState.IsValid)
            {
                _context.Add(plc);
                await _context.SaveChangesAsync();
                NLogHelper.Info("PLC - Create: Valid infomation. Create new PLC to database.\n");

                var configWebContext = _context.PLCs.Include(c => c.Workplace);
                return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "_ViewAll", await configWebContext.ToListAsync()) });
            }

            NLogHelper.Info("PLC - Create: InValid infomation.");
            PLCViewModel plcViewModel = new PLCViewModel();
            plcViewModel.PLC = plc;
            plcViewModel.WorkplaceList = new SelectList(_context.Workplaces, "Code", "Code");
            return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "Create", plcViewModel) });
        }


        // GET: Cameras/Edit/5
        [NoDirectAccess]
        public async Task<IActionResult> Edit(string id)
        {
            NLogHelper.Info("PLC - Edit: Find PLC base on id");
            if (id == null)
            {
                NLogHelper.Info("PLC - Edit: Can not find PLC with id: " + id);
                return NotFound();
            }

            var plc = await _context.PLCs.FindAsync(id);
            if (plc == null)
            {
                NLogHelper.Info("PLC - Edit: Can not find PLC with id: " + id);
                return NotFound();
            }

            NLogHelper.Info("PLC - Edit: Found PLC with id: " + id); 
            PLCViewModel pLCViewModel = new PLCViewModel();
            pLCViewModel.PLC = plc;
            pLCViewModel.WorkplaceList = new SelectList(_context.Workplaces, "Code", "Code", plc.WorkplaceCode);
            NLogHelper.Info("PLC - Edit: Show Edit popup");
            return View(pLCViewModel);
        }

        // POST: Cameras/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Code,IP,Barrier,WorkplaceCode")] PLC plc)
        {
            NLogHelper.Info("PLC - Edit: Try to edit PLC");
            if (id != plc.Code)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(plc);
                    await _context.SaveChangesAsync();
                    NLogHelper.Info("PLC - Edit: Valid infomation. Update PLC information to database.\n");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PLCExists(plc.Code))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                var configWebContext = _context.PLCs.Include(c => c.Workplace);
                return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "_ViewAll", await configWebContext.ToListAsync()) });
            }
            NLogHelper.Info("PLC - Edit: InValid infomation.");
            PLCViewModel plcViewModel = new PLCViewModel();
            plcViewModel.PLC = plc;
            plcViewModel.WorkplaceList = new SelectList(_context.Workplaces, "Code", "Code", plc.WorkplaceCode);
            return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "Edit", plcViewModel) });
        }


        // POST: Cameras/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var plc = await _context.PLCs.FindAsync(id);
            _context.PLCs.Remove(plc);
            await _context.SaveChangesAsync();
            NLogHelper.Info("PLC - Delete: Removed PLC with id: " + id + "\n");
            var configWebContext = _context.PLCs.Include(c => c.Workplace);
            return Json(new { html = Helper.RenderRazorViewToString(this, "_ViewAll", await configWebContext.ToListAsync()) });
        }

        private bool PLCExists(string id)
        {
            return _context.PLCs.Any(e => e.Code == id);
        }
    }
}
