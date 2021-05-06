using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITSWebMgmt.Models;
using ITSWebMgmt.Models.Log;
using ITSWebMgmt.Helpers;
using System.Net;

namespace ITSWebMgmt.Controllers
{
    public class ErrorCodesController : WebMgmtController
    {
        public ErrorCodesController(WebMgmtContext context) : base(context)
        {
        }

        public async Task<IActionResult> Search(string currentFilter, string searchString, int? pageNumber)
        {
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var errorCodes = _context.ErrorCodes.AsNoTracking();

            if (!string.IsNullOrEmpty(searchString))
            {
                errorCodes = errorCodes.Where(s => s.ErrorCodeName.Contains(searchString));
            }

            int pageSize = 20;
            return View(await PaginatedList<ErrorCode>.CreateAsync(errorCodes.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        public async Task<IActionResult> GetErrorCodesByName([FromBody] string name)
        {
            var names = new List<string>();
            try
            {
                names = await _context.ErrorCodes.Where(x => x.ErrorCodeName.Contains(name)).Take(10).Select(x => x.ErrorCodeName).ToListAsync();
            }
            catch (Exception)
            {
            }
            Response.StatusCode = (int)HttpStatusCode.OK;
            return Json(new { success = true, names });
        }

        // GET: ErrorCodes
        public async Task<IActionResult> Index()
        {
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }

            return View(await _context.ErrorCodes.ToListAsync());
        }

        // GET: ErrorCodes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var errorCode = await _context.ErrorCodes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (errorCode == null)
            {
                return NotFound();
            }

            return View(errorCode);
        }

        // GET: ErrorCodes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ErrorCodes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ErrorCodeName,Description,OneNoteLink")] ErrorCode errorCode)
        {
            if (ModelState.IsValid)
            {
                _context.Add(errorCode);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(errorCode);
        }

        // GET: ErrorCodes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var errorCode = await _context.ErrorCodes.FindAsync(id);
            if (errorCode == null)
            {
                return NotFound();
            }
            return View(errorCode);
        }

        // POST: ErrorCodes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ErrorCodeName,Description,OneNoteLink")] ErrorCode errorCode)
        {
            if (id != errorCode.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(errorCode);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ErrorCodeExists(errorCode.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(errorCode);
        }

        // GET: ErrorCodes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var errorCode = await _context.ErrorCodes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (errorCode == null)
            {
                return NotFound();
            }

            return View(errorCode);
        }

        // POST: ErrorCodes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var errorCode = await _context.ErrorCodes.FindAsync(id);
            _context.ErrorCodes.Remove(errorCode);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ErrorCodeExists(int id)
        {
            return _context.ErrorCodes.Any(e => e.Id == id);
        }
    }
}
