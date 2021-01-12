using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITSWebMgmt.Models;
using ITSWebMgmt.Models.Log;

namespace ITSWebMgmt.Views
{
    public class TrashRequestsController : Controller
    {
        private readonly WebMgmtContext _context;

        public TrashRequestsController(WebMgmtContext context)
        {
            _context = context;
        }

        // GET: TrashRequests
        public async Task<IActionResult> Index()
        {
            return View(await _context.TrashRequests.ToListAsync());
        }

        // GET: TrashRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trashRequest = await _context.TrashRequests
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trashRequest == null)
            {
                return NotFound();
            }

            return View(trashRequest);
        }

        // GET: TrashRequests/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TrashRequests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TimeStamp,RequestedBy,CreatedBy,ØSSEmployeeName,ØSSEmployeeId,EquipmentManager,Status")] TrashRequest trashRequest)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trashRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(trashRequest);
        }

        // GET: TrashRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trashRequest = await _context.TrashRequests.FindAsync(id);
            if (trashRequest == null)
            {
                return NotFound();
            }
            return View(trashRequest);
        }

        // POST: TrashRequests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TimeStamp,RequestedBy,CreatedBy,ØSSEmployeeName,ØSSEmployeeId,EquipmentManager,Status")] TrashRequest trashRequest)
        {
            if (id != trashRequest.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trashRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrashRequestExists(trashRequest.Id))
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
            return View(trashRequest);
        }

        // GET: TrashRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trashRequest = await _context.TrashRequests
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trashRequest == null)
            {
                return NotFound();
            }

            return View(trashRequest);
        }

        // POST: TrashRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trashRequest = await _context.TrashRequests.FindAsync(id);
            _context.TrashRequests.Remove(trashRequest);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TrashRequestExists(int id)
        {
            return _context.TrashRequests.Any(e => e.Id == id);
        }
    }
}
