using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITSWebMgmt.Models;
using ITSWebMgmt.Models.Log;
using ITSWebMgmt.Helpers;

namespace ITSWebMgmt.Controllers
{
    public class KnownIssuesController : WebMgmtController
    {
        public KnownIssuesController(LogEntryContext context) : base(context) { }

        // GET: KnownIssues
        public async Task<IActionResult> Index()
        {
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }
            return View(await _context.KnownIssues.ToListAsync());
        }

        // GET: KnownIssues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }

            if (id == null)
            {
                return NotFound();
            }

            var knownIssue = await _context.KnownIssues
                .FirstOrDefaultAsync(m => m.Id == id);
            if (knownIssue == null)
            {
                return NotFound();
            }

            return View(knownIssue);
        }

        // GET: KnownIssues/Create
        public IActionResult Create()
        {
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }

            return View();
        }

        // POST: KnownIssues/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,OneNoteLink,CaseLink,Active")] KnownIssue knownIssue)
        {
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }

            if (ModelState.IsValid)
            {
                _context.Add(knownIssue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(knownIssue);
        }

        // GET: KnownIssues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }

            if (id == null)
            {
                return NotFound();
            }

            var knownIssue = await _context.KnownIssues.FindAsync(id);
            if (knownIssue == null)
            {
                return NotFound();
            }
            return View(knownIssue);
        }

        // POST: KnownIssues/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,OneNoteLink,CaseLink,Active")] KnownIssue knownIssue)
        {
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }

            if (id != knownIssue.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(knownIssue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KnownIssueExists(knownIssue.Id))
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
            return View(knownIssue);
        }

        // GET: KnownIssues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }

            if (id == null)
            {
                return NotFound();
            }

            var knownIssue = await _context.KnownIssues
                .FirstOrDefaultAsync(m => m.Id == id);
            if (knownIssue == null)
            {
                return NotFound();
            }

            return View(knownIssue);
        }

        // POST: KnownIssues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }

            var knownIssue = await _context.KnownIssues.FindAsync(id);
            _context.KnownIssues.Remove(knownIssue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KnownIssueExists(int id)
        {
            return _context.KnownIssues.Any(e => e.Id == id);
        }
    }
}
