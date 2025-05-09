using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repository.IRepository;

namespace WebApplication1.Controllers
{

    [Authorize(Roles = WC.AdminRole)]
    public class CategoryController : Controller
	{
		private readonly ICategoryRepository _catReop;

		public CategoryController(ICategoryRepository catReop)
		{
            _catReop = catReop;

        }

        //All Data
		public IActionResult Index()
		{
			IEnumerable<Category> objList = _catReop.GetAll();
			return View(objList);
		}

		// GET - Create
        public IActionResult Create()
        {       
            return View();
        }

		// POST - Create
		[HttpPost]
		[ValidateAntiForgeryToken]
        public IActionResult Create( Category obj)
        {
			if (ModelState.IsValid)
			{
                _catReop.Add(obj);
                _catReop.Save();
              //  TempData[WC.Success] ="Category created Successfully";
                return RedirectToAction("Index");
            }
          //  TempData[WC.Error] = "Error while creating Category";
            return View(obj);
        }

        // GET - Edit
        public IActionResult Edit(int? id)
        {
			if(id == null || id==0)
			{
				return NotFound();
			}
			var obj = _catReop.Find(id.GetValueOrDefault()) ;
			if(obj == null)
			{
				return NotFound();
			}

            return View(obj);
        }

        // POST - Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _catReop.Update(obj);
                _catReop.Save();
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        //--------------------------------

        // GET - Delete
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _catReop.Find(id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }

        // POST - Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _catReop.Find(id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }
            _catReop.Remove(obj);
            _catReop.Save();
             return RedirectToAction("Index");           
        }

    }
}
