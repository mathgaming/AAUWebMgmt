using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class SimpleModel
    {
        public SimpleModel(string text)
        {
            Text = text;
        }
        public string Text { get; set; }
    }
}
