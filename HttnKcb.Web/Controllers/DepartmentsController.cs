using HttnKcb.Web.ApiClients;
using HttnKcb.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HttnKcb.Web.Controllers
{
  public class DepartmentsController : Controller
  {
    private readonly DepartmentApiClient _api;

    public DepartmentsController(DepartmentApiClient api)
    {
      _api = api;
    }

    // GET: /Departments
    public async Task<IActionResult> Index(string? keyword)
    {
      var list = await _api.GetAllAsync(keyword);
      ViewBag.Keyword = keyword;
      return View(list);
    }

    // GET: /Departments/Create
    [HttpGet]
    public IActionResult Create()
    {
      return View();
    }

    // POST: /Departments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DepartmentViewModel vm)
    {
      if (!ModelState.IsValid)
        return View(vm);

      var ok = await _api.CreateAsync(vm);
      if (!ok)
      {
        ModelState.AddModelError("", "Cannot create department.");
        return View(vm);
      }

      return RedirectToAction(nameof(Index));
    }

    // GET: /Departments/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
      var vm = await _api.GetByIdAsync(id);
      if (vm == null)
        return NotFound();

      return View(vm);
    }

    // POST: /Departments/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DepartmentViewModel vm)
    {
      if (!ModelState.IsValid)
        return View(vm);

      var ok = await _api.UpdateAsync(id, vm);
      if (!ok)
      {
        ModelState.AddModelError("", "Cannot update department.");
        return View(vm);
      }

      return RedirectToAction(nameof(Index));
    }

    // GET: /Departments/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
      var vm = await _api.GetByIdAsync(id);
      if (vm == null)
        return NotFound();

      return View(vm);
    }

    // POST: /Departments/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      await _api.DeleteAsync(id);
      return RedirectToAction(nameof(Index));
    }
  }
}
