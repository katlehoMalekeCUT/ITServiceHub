using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub_IT.Models;
using ServiceHub_IT.Services;

namespace ServiceHub_IT.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class DepartmentsController : Controller
{
    private readonly DepartmentService _departmentService;

    public DepartmentsController(DepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var departments = await _departmentService.GetDepartmentsAsync(cancellationToken);
        return View(departments);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new DepartmentViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DepartmentViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var department = new Department
        {
            name = model.Name,
            description = model.Description,
            employee_count = 0,
            asset_count = 0,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        var created = await _departmentService.CreateDepartmentAsync(department, cancellationToken);
        if (!created)
        {
            TempData["Error"] = "Unable to create the department.";
            return View(model);
        }

        TempData["Success"] = "Department created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        var department = await _departmentService.GetDepartmentAsync(id, cancellationToken);
        if (department is null)
        {
            return NotFound();
        }

        return View(new DepartmentViewModel { Name = department.name, Description = department.description });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, DepartmentViewModel model, CancellationToken cancellationToken)
    {
        var department = await _departmentService.GetDepartmentAsync(id, cancellationToken);
        if (department is null)
        {
            return NotFound();
        }

        department.name = model.Name;
        department.description = model.Description;
        department.updated_at = DateTime.UtcNow;

        var updated = await _departmentService.UpdateDepartmentAsync(department, cancellationToken);
        if (!updated)
        {
            TempData["Error"] = "Unable to update the department.";
            return View(model);
        }

        TempData["Success"] = "Department updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var deleted = await _departmentService.DeleteDepartmentAsync(id, cancellationToken);
        if (!deleted)
        {
            TempData["Error"] = "Unable to delete the department.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Department deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
