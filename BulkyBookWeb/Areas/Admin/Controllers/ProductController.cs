using BulkyBook.Business.Services.IServices;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IProductService productService, ICategoryService categoryService, IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _categoryService = categoryService;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            //var products = await _productService.GetAllProductAsync();
            return View();
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            var categories = await _categoryService.GetAllCategoryAsync();
            ProductVM productVM = new()
            {
                CategoryList = categories.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }),
                Product = new Product()
            };
            if (id == null || id == 0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = await _productService.GetProductByIdAsync(id.Value);
                if (productVM.Product == null)
                {
                    return NotFound();
                }
                return View(productVM);
            }


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Upsert")]
        public async Task<IActionResult> UpsertPOST(ProductVM productVM, IFormFile? file)
        {

            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    //string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.Name);
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine("images", "products");
                    string finalPath = Path.Combine(wwwRootPath, productPath);
                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        // delete the old image
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\', '/'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Save the new Image

                    using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    productVM.Product.ImageUrl = Path.Combine(@"\", productPath, fileName).Replace("\\", "/");
                }

                //if (file != null)
                //{
                //    string wwwRootPath = _webHostEnvironment.WebRootPath;
                //    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                //    string productPath = Path.Combine(wwwRootPath, "images", "products");

                //    if (!Directory.Exists(productPath))
                //    {
                //        Directory.CreateDirectory(productPath);
                //    }

                //    // Delete old image if updating
                //    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                //    {
                //        var oldImagePath = Path.Combine(
                //            wwwRootPath,
                //            productVM.Product.ImageUrl.TrimStart('\\', '/'));

                //        if (System.IO.File.Exists(oldImagePath))
                //        {
                //            System.IO.File.Delete(oldImagePath);
                //        }
                //    }

                //    // Save new image
                //    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                //    {
                //        await file.CopyToAsync(fileStream);
                //    }

                //    productVM.Product.ImageUrl = $"/images/products/{fileName}";
                //}

                if (productVM.Product.Id == null || productVM.Product.Id == 0)
                {
                    await _productService.CreateProductAsync(productVM.Product);
                    TempData["success"] = "Product created successfully";
                }
                else
                {
                    await _productService.UpdateProductAsync(productVM.Product);
                    TempData["success"] = "Product Updated successfully";


                }



                return RedirectToAction("Index");
            }
            else
            {
                var categories = await _categoryService.GetAllCategoryAsync();
                productVM = new()
                {
                    CategoryList = categories.Select(c => new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString()
                    }),
                    Product = new Product()
                };

                return View(productVM);
            }
        }


        // Update Product

        // GET
        //public async Task<IActionResult> Update(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }

        //    var product = await _productService.GetProductByIdAsync(id.Value);

        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(product);
        //}

        //// POST
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[ActionName("Update")]
        //public async Task<IActionResult> UpdatePOST(Product product)
        //{

        //    if (ModelState.IsValid)
        //    {
        //        await _productService.UpdateProductAsync(product);
        //        TempData["success"] = "Product updated successfully";
        //        return RedirectToAction("Index");
        //    }

        //    return View(product);
        //}

        // Delete Product





        #region API CALLS
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllProductAsync(true);
            return Json(new { data = products });
        }

        // Delete
        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)

        {
            if (id == 0 || id == null)
            {
                return Json(new { success = false, message = "Invalid Id" });
            }
            var productToBeDeleted = await _productService.GetProductByIdAsync(id.Value);

            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            // Delete product image if exists
            if (!string.IsNullOrEmpty(productToBeDeleted.ImageUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\', '/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            await _productService.DeleteProductAsync(id.Value);

            //TempData["success"] = "Product deleted successfully";

            return Json(new { success = true, message = "Product deleted successfully" });
        }

        #endregion
    }
}
