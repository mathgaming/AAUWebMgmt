using ITSWebMgmt.Connectors;
using ITSWebMgmt.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Helpers
{
    public class ØSSCSVImporter
    {
        // Remember to add this to the database from a controller
        public List<MacCSVInfo> Import()
        {
            List<MacCSVInfo> data = new List<MacCSVInfo>();
            List<MacCSVInfo> remove = new List<MacCSVInfo>();
            int id = 0;

            using (var reader = new StreamReader(@"C:\webmgmtlog\macs-invoice-aau-number.csv"))
            {
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    bool notDeleted  = int.Parse(values[9]) > 0;
                    bool missingSerialNumber = values[27] == "";
                    bool foundInØSS = values[65] != "";

                    if (missingSerialNumber)
                    {
                        values[27] = $"Unknown {id++}";
                    }

                    MacCSVInfo info = new MacCSVInfo()
                    {
                        Name = values[5],
                        Specs = values[6],
                        ComputerType = values[7],
                        InvoiceNumber = values[12],
                        SerialNumber = values[27],
                        OESSAssetNumber = values[65],
                        AAUNumber = values[66]
                    };

                    if (notDeleted)
                    {
                        data.Add(info);
                    }
                    else
                    {
                        remove.Add(info);
                    }
                }
            }

            foreach (var r in remove)
            {
                data.RemoveAll(x => x.SerialNumber == r.SerialNumber);
            }

            data = data.GroupBy(x => x.SerialNumber).Select(g => g.First()).ToList();

            return data;
        }

        public async Task ReadCSVAsync()
        {
            ØSSConnector øss = new ØSSConnector();
            string prevInvoiceNumber = "";
            string prevLine = "";
            int fileId = 0;
            List<string> tagNumbers = new List<string>();
            using (var reader = new StreamReader(@"C:\webmgmtlog\macs-asset.csv"))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    string invoiceNumber = values[12];
                    bool dulicate = values[64] == "TRUE";
                    string assetNumber = values[65];
                    string tagNumber = "";

                    if (dulicate)
                    {
                        if (prevInvoiceNumber != invoiceNumber)
                        {
                            List<string> numbers = new List<string>();
                            foreach (string asset in assetNumber.Split(","))
                            {
                                numbers.Add(await øss.GetTagNumberFromAssetNumberAsync(asset));
                            }

                            prevLine = string.Join(",", numbers);
                        }
                        tagNumbers.Add(prevLine);
                    }
                    else if (assetNumber.Length != 0)
                    {
                        tagNumber = await øss.GetTagNumberFromAssetNumberAsync(assetNumber);
                        tagNumbers.Add(tagNumber);
                    }
                    else
                    {
                        tagNumbers.Add("");
                    }

                    prevInvoiceNumber = invoiceNumber;
                }

                Save(tagNumbers, fileId);
            }
        }

        public async Task GetAssetFromCSVIncoiceAsync()
        {
            ØSSConnector øss = new ØSSConnector();
            string prevInvoiceNumber = "";
            string prevLine = "";
            int fileId = 0;
            List<string> assetnumbers = new List<string>();
            using (var reader = new StreamReader(@"C:\webmgmtlog\macs.csv"))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    string invoiceNumber = values[12];
                    string serialNumber = values[27];
                    bool dulicate = values[64] == "TRUE";

                    string assetNumber = "";

                    if (dulicate)
                    {
                        if (prevInvoiceNumber != invoiceNumber)
                        {
                            var numbers = øss.GetAssetNumbersFromInvoiceNumberAsync(invoiceNumber);
                            prevLine = string.Join(",", numbers);
                        }
                        assetnumbers.Add(prevLine);
                    }
                    else
                    {
                        if (serialNumber.Length != 0)
                        {
                            assetNumber = await øss.GetAssetNumberFromSerialNumberAsync(serialNumber);
                        }

                        if (assetNumber.Length == 0)
                        {
                            assetNumber = await øss.GetAssetNumberFromInvoiceNumberAsync(invoiceNumber);
                        }
                        assetnumbers.Add(assetNumber);
                    }

                    prevInvoiceNumber = invoiceNumber;


                    if (assetnumbers.Count == 100)
                    {
                        Save(assetnumbers, fileId);
                        assetnumbers = new List<string>();
                        fileId++;
                    }
                }
            }
            Save(assetnumbers, fileId);
        }

        private void Save(List<string> assetNumbers, int fileId)
        {
            using (StreamWriter file = new StreamWriter(@"C:\webmgmtlog\macs-assets-" + fileId.ToString() + ".txt"))
            {
                foreach (string line in assetNumbers)
                {
                    file.WriteLine(line);
                }
            }
        }
    }
}
