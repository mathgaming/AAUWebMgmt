using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITSWebMgmt.Models;
using ITSWebMgmt.Models.Log;
using ITSWebMgmt.Helpers;

namespace ITSWebMgmt.Controllers
{
    public class MissingGroupsController : WebMgmtController
    {
        public MissingGroupsController(LogEntryContext context) : base(context)
        {
        }

        // GET: MissingGroups
        public async Task<IActionResult> Index()
        {
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }

            return View(await _context.MacErrors.ToListAsync());
        }

        // GET: MissingGroups/Details/5
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
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }

            return View();
        }

        // POST: MissingGroups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GroupName,CaseLink,Active,Heading,Description,Severeness")] MissingGroup missingGroup)
        {
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }

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
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }

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
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }

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
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }

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
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }

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
