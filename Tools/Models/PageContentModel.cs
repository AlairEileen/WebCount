using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Models
{
    public class PageContentModel<T>
    {
        public int PageIndex { get; set; }
        public int CurrentPage { get; set; }
        public int[] PageNumber { get; set; }
        public T PageData { get; set; }
    }
}
