using System.Collections.Generic;

namespace WebApplication1.Models.ViewModels
{
    public class HomeVM
    {

        public IEnumerable<Product> products { get; set; }

        public IEnumerable<Category> categories { get; set; }



    }
}
