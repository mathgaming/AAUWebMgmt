using ITSWebMgmt.Connectors;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using ITSWebMgmt.Models.Log;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ITSWebMgmt.Controllers
{
    public class PlatformController : WebMgmtController
    {
        public PlatformController(WebMgmtContext context) : base(context) { }

        public IActionResult Index()
        {
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateList()
        {
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }
            try
            {
                await UpdateØSSStatusAsync();
                return Success();
            }
            catch (Exception e)
            {
                return Error(e.Message);
            }

        }

        [HttpPost]
        public async Task<IActionResult> UpdateBestAAUGuess()
        {
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }
            try
            {
                await UpdateBestAAUGuessJamfAsync();
                return Success();
            }
            catch (Exception e)
            {
                return Error(e.Message);
            }

        }

        public async Task UpdateBestAAUGuessJamfAsync()
        {
            JamfConnector jc = new JamfConnector();
            foreach (var computer in await jc.GetAllComputersAsync())
            {
                ComputerModel model = new ComputerModel(computer.name, null);
                model.Mac = new MacComputerModel(model, computer.id);
                await model.Mac.InitModelAsync();
                string number = model.Mac.ComputerName;

                if (model.Mac.AssetTag != null && model.Mac.AssetTag != "")
                {
                    number = model.Mac.AssetTag;
                }

                string sn = model.Mac.SerialNumber;
                if (sn.Length > 0 && sn[0] != 'S' && sn[0] != 's')
                {
                    sn = "S" + sn;
                }
                MacCSVInfo info = _context.MacCSVInfos.FirstOrDefault(x => x.SerialNumber == sn);
                if (info != null && !info.AAUNumber.Contains(','))
                {
                    number = info.AAUNumber;
                }
                else
                {
                    ØSSConnector øss = new ØSSConnector();
                    string assetNumber = await øss.GetAssetNumberFromSerialNumberAsync(sn);
                    if (assetNumber != "")
                    {
                        string tagNumber = await øss.GetTagNumberFromAssetNumberAsync(assetNumber);
                        if (tagNumber != "")
                        {
                            number = tagNumber;
                        }
                    }
                }

                string xml = $"<?xml version=\"1.0\"?><computer><extension_attributes><extension_attribute><name>AAUNumber</name> <value>{number}</value> </extension_attribute></extension_attributes></computer>";
                _ = await jc.SendUpdateReuestAsync($"computers/id/{computer.id}/subset/ExtensionAttributes", xml);

                Thread.Sleep(5000);
            }
        }
        public async Task UpdateØSSStatusAsync()
        {
            JamfConnector jc = new JamfConnector();
            foreach (var computer in await jc.GetAllComputersAsync())
            {
                ComputerModel model = new ComputerModel(computer.name, null);
                model.Mac = new MacComputerModel(model, computer.id);
                await model.Mac.InitModelAsync();

                string sn = model.Mac.SerialNumber;
                if (sn.Length > 0 && sn[0] != 'S' && sn[0] != 's')
                {
                    sn = "S" + sn;
                }
                MacCSVInfo info = _context.MacCSVInfos.FirstOrDefault(x => x.SerialNumber == sn);
                if (info != null && !info.OESSAssetNumber.Contains(','))
                {
                    model.SetØSSAssetnumber(info.OESSAssetNumber);
                }

                (string status, string comment) = await new ØSSConnector().GetOESSStatusAsync(await model.GetØSSAssetnumberAsync());

                string xml = $"<?xml version=\"1.0\"?><computer><extension_attributes><extension_attribute><name>F-status</name> <value>{status}</value> </extension_attribute><extension_attribute><name>F-comment</name> <value>{comment}</value> </extension_attribute></extension_attributes></computer>";
                _ = await jc.SendUpdateReuestAsync($"computers/id/{computer.id}/subset/ExtensionAttributes", xml);

                Thread.Sleep(5000);
            }
        }

        public void StopMaintenance()
        {
            if (Authentication.IsPlatform(HttpContext.User.Identity.Name))
            {
                MaintenanceHelper.IsDownForMaintenance = false;
                Response.Redirect("/");
            }
        }

        [HttpPost]
        public IActionResult StartMaintenance([FromBody] string message)
        {
            if (Authentication.IsPlatform(HttpContext.User.Identity.Name))
            {
                MaintenanceHelper.IsDownForMaintenance = true;
                MaintenanceHelper.Message = message;
                return Success("WebMgmt is now down for maintenance");
            }

            return Error("You do no have access to do this");
        }
    }
}
