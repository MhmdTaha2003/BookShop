﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections;
using System.Collections.Generic;

namespace WebApplication1.Models.ViewModels
{
    public class ProductVM
    {
      public  Product Product { get; set; }
      public  IEnumerable <SelectListItem> CategorySelectList { get; set; }

    }
}
