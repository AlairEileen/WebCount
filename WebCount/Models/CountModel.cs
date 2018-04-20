using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools.Models;

namespace WebCount.Models
{
    public class CountModel:BaseModel
    {
        public string AppID { get; set; }
        public string AppSecret { get; set; }
        public string AccessToken { get; set; }
        public int TimeOutLength { get; set; }
    }
}
