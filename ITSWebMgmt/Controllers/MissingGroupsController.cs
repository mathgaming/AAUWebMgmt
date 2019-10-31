using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITSWebMgmt.Models;
using ITSWebMgmt.Models.Log;

namespace ITSWebMgmt.Controllers
{
    public class MissingGroupsController : Controller
    {
        private readonly LogEntryContext _context;

        public MissingGroupsController(LogEntryContext context)
        {
            _context = context;
        }

        // GET: MissingGroups
        public async Task<IActionResult> Index()
        {
            return View(await _context.MacErrors.ToListAsync());
        }

        // GET: MissingGroups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var missingGroup = await _context.MacErrors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (missingGroup == null)
            {
                return NotFound();
            }

            return View(missingGroup);
        }

        // GET: MissingGroups/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MissingGroups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GroupName,CaseLink,Active,Heading,Description,Severeness")] MissingGroup missingGroup)
        {
            if (ModelState.IsValid)
            {
                _context.Add(missingGroup);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(missingGroup);
        }

        // GET: MissingGroups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var missingGroup = await _context.MacErrors.FindAsync(id);
            if (missingGroup == null)
            {
                return NotFound();
            }
            return View(missingGroup);
        }

        // POST: MissingGroups/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GroupName,CaseLink,Active,Heading,Description,Severeness")] MissingGroup missingGroup)
        {
            if (id != missingGroup.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(missingGroup);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MissingGroupExists(missingGroup.Id))
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
            return View(missingGroup);
        }

        // GET: MissingGroups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var missingGroup = await _context.MacErrors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (missingGroup == null)
            {
                return NotFound();
            }

            return View(missingGroup);
        }

        // POST: MissingGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var missingGroup = await _context.MacErrors.FindAsync(id);
            _context.MacErrors.Remove(missingGroup);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MissingGroupExists(int id)
        {
            return _context.MacErrors.Any(e => e.Id == id);
        }
    }
}
