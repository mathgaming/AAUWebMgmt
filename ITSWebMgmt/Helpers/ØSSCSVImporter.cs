using ITSWebMgmt.Connectors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Helpers
{
    public class ØSSCSVImporter
    {
        public void readCSV()
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
                                numbers.Add(øss.GetTagNumberFromAssetNumber(asset));
                            }

                            prevLine = string.Join(",", numbers);
                        }
                        tagNumbers.Add(prevLine);
                    }
                    else if (assetNumber.Length != 0)
                    {
                        tagNumber = øss.GetTagNumberFromAssetNumber(assetNumber);
                        tagNumbers.Add(tagNumber);
                    }
                    else
                    {
                        tagNumbers.Add("");
                    }

                    prevInvoiceNumber = invoiceNumber;
                }

                save(tagNumbers, fileId);
            }
        }

        public void GetAssetFromCSVIncoice()
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
                            var numbers = øss.GetAssetNumbersFromInvoiceNumber(invoiceNumber);
                            prevLine = string.Join(",", numbers);
                        }
                        assetnumbers.Add(prevLine);
                    }
                    else
                    {
                        if (serialNumber.Length != 0)
                        {
                            assetNumber = øss.GetAssetNumberFromSerialNumber(serialNumber);
                        }

                        if (assetNumber.Length == 0)
                        {
                            assetNumber = øss.GetAssetNumberFromInvoiceNumber(invoiceNumber);
                        }
                        assetnumbers.Add(assetNumber);
                    }

                    prevInvoiceNumber = invoiceNumber;


                    if (assetnumbers.Count == 100)
                    {
                        save(assetnumbers, fileId);
                        assetnumbers = new List<string>();
                        fileId++;
                    }
                }
            }
            save(assetnumbers, fileId);
        }

        private void save(List<string> assetNumbers, int fileId)
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
