using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITSWebMgmt.Models;
using ITSWebMgmt.Models.Log;
using ITSWebMgmt.Connectors;
using ITSWebMgmt.Helpers;

namespace ITSWebMgmt.Controllers
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
        public async Task<IActionResult> Create(string data)
        {
            TrashRequest trashRequest = new TrashRequest
            {
                CreatedBy = HttpContext.User.Identity.Name
            };

            if (data is not null && data != "")
            {
                trashRequest.ComputerName = data;

                trashRequest = await AddOESSInfoAsync(trashRequest);
            }

            return View(trashRequest);
        }

        public async Task<TrashRequest> AddOESSInfoAsync(TrashRequest trashRequest)
        {
            if (trashRequest.ComputerName is not null && trashRequest.ComputerName != "")
            {
                ØSSConnector Øss = new ØSSConnector();
                ComputerModel model = new ComputerModel(trashRequest.ComputerName, trashRequest);
                var assetNumber = await model.GetØSSAssetnumberAsync(trashRequest.ComputerName);
                ØSSInfo info = await Øss.GetØSSInfoAsync(assetNumber);

                trashRequest.Desciption = $"{info.Manufacturer} {info.ModelNumber}";
                var (email, first_name, last_name) = await Øss.GetResponsiblePersonAsync(await model.GetØSSSegmentAsync(assetNumber));
                trashRequest.EquipmentManager = $"{first_name} {last_name}";
                trashRequest.EquipmentManagerEmail = email;
            }

            return trashRequest;
        }

        // POST: TrashRequests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TimeStamp,ComputerName,Desciption,Comment,RequestedBy,CreatedBy,EquipmentManager,EquipmentManagerEmail,Status")] TrashRequest trashRequest, string command)
        {
            if (command.Equals("Autofill"))
            {
                ModelState.Clear();
                trashRequest = await AddOESSInfoAsync(trashRequest);

                return View("Create", trashRequest);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    trashRequest.TimeStamp = DateTime.Now;
                    trashRequest.Status = TrashRequestStatus.NotConfirmed;
                    _context.Add(trashRequest);
                    await _context.SaveChangesAsync();
                    SendTrashComputerEmail(trashRequest);
                    return RedirectToAction(nameof(Index));
                }
                return View(trashRequest);
            }
        }

        private void SendTrashComputerEmail(TrashRequest trashRequest)
        {
            string subject = $"Kvittering for afleveringsnummer {trashRequest.Id}";
            string body = $"(Brugers navn) har i dag afleveret {trashRequest.Desciption} med AAU-nummer {trashRequest.ComputerName} til {trashRequest.CreatedBy} hos ITS.\n\n" +
                          $"Kvittering er sendt til {trashRequest.EquipmentManager}({trashRequest.EquipmentManagerEmail})\n\n" +
                          $"Ekstra kvittering er sendt til {trashRequest.RequestedBy}";

            // EmailHelper.SendEmail(subject, body, trashRequest.EquipmentManagerEmail, trashRequest.RequestedBy);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,TimeStamp,ComputerName,Desciption,Comment,RequestedBy,CreatedBy,EquipmentManager,EquipmentManagerEmail,Status")] TrashRequest trashRequest)
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
