using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub_IT.Models;
using ServiceHub_IT.Services;

namespace ServiceHub_IT.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("Admin/Assets")]
public class AssetsController : Controller
{
    private readonly AssetService _assetService;
    private readonly PdfProcessingService _pdfProcessingService;

    public AssetsController(AssetService assetService, PdfProcessingService pdfProcessingService)
    {
        _assetService = assetService;
        _pdfProcessingService = pdfProcessingService;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(AssetSearchViewModel search, CancellationToken cancellationToken)
    {
        var assets = await _assetService.GetAssetsAsync(search, cancellationToken);
        return View(assets);
    }

    [HttpGet("Create")]
    public IActionResult Create() => View(new AssetFormViewModel());

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssetFormViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var imageUrl = viewModel.ImageFile is not null && viewModel.ImageFile.Length > 0
            ? await _assetService.UploadFileAsync(viewModel.ImageFile, _assetService.GetAssetImageBucketName(), $"{Guid.NewGuid():N}_{viewModel.ImageFile.FileName}", cancellationToken)
            : null;

        var warrantyUrl = viewModel.WarrantyFile is not null && viewModel.WarrantyFile.Length > 0
            ? await _assetService.UploadFileAsync(viewModel.WarrantyFile, _assetService.GetAssetWarrantyBucketName(), $"{Guid.NewGuid():N}_{viewModel.WarrantyFile.FileName}", cancellationToken)
            : null;

        var asset = new Asset
        {
            asset_tag = viewModel.AssetTag,
            asset_name = viewModel.AssetName,
            brand = viewModel.Brand,
            model = viewModel.Model,
            serial_number = viewModel.SerialNumber,
            category = viewModel.Category,
            purchase_date = viewModel.PurchaseDate,
            warranty_expiry = viewModel.WarrantyExpiry,
            department = viewModel.Department,
            assigned_employee = viewModel.AssignedEmployee,
            location = viewModel.Location,
            status = viewModel.Status,
            image_url = imageUrl,
            warranty_url = warrantyUrl,
            qr_code_url = _assetService.GenerateQrCodeUrl(viewModel.AssetTag)
        };

        var success = await _assetService.CreateAssetAsync(asset, cancellationToken);
        if (!success)
        {
            TempData["ErrorMessage"] = "Unable to create the asset right now.";
            return View(viewModel);
        }

        TempData["SuccessMessage"] = "Asset created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        var asset = await _assetService.GetAssetAsync(id, cancellationToken);
        if (asset is null)
        {
            return NotFound();
        }

        var model = new AssetFormViewModel
        {
            Id = asset.id,
            AssetTag = asset.asset_tag,
            AssetName = asset.asset_name,
            Brand = asset.brand,
            Model = asset.model,
            SerialNumber = asset.serial_number,
            Category = asset.category,
            PurchaseDate = asset.purchase_date,
            WarrantyExpiry = asset.warranty_expiry,
            Department = asset.department,
            AssignedEmployee = asset.assigned_employee,
            Location = asset.location,
            Status = asset.status
        };

        return View(model);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, AssetFormViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var imageUrl = viewModel.ImageFile is not null && viewModel.ImageFile.Length > 0
            ? await _assetService.UploadFileAsync(viewModel.ImageFile, _assetService.GetAssetImageBucketName(), $"{Guid.NewGuid():N}_{viewModel.ImageFile.FileName}", cancellationToken)
            : null;

        var warrantyUrl = viewModel.WarrantyFile is not null && viewModel.WarrantyFile.Length > 0
            ? await _assetService.UploadFileAsync(viewModel.WarrantyFile, _assetService.GetAssetWarrantyBucketName(), $"{Guid.NewGuid():N}_{viewModel.WarrantyFile.FileName}", cancellationToken)
            : null;

        var asset = new Asset
        {
            id = id,
            asset_tag = viewModel.AssetTag,
            asset_name = viewModel.AssetName,
            brand = viewModel.Brand,
            model = viewModel.Model,
            serial_number = viewModel.SerialNumber,
            category = viewModel.Category,
            purchase_date = viewModel.PurchaseDate,
            warranty_expiry = viewModel.WarrantyExpiry,
            department = viewModel.Department,
            assigned_employee = viewModel.AssignedEmployee,
            location = viewModel.Location,
            status = viewModel.Status,
            image_url = imageUrl,
            warranty_url = warrantyUrl
        };

        var success = await _assetService.UpdateAssetAsync(asset, cancellationToken);
        if (!success)
        {
            TempData["ErrorMessage"] = "Unable to update the asset right now.";
            return View(viewModel);
        }

        TempData["SuccessMessage"] = "Asset updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(string id, CancellationToken cancellationToken)
    {
        var asset = await _assetService.GetAssetAsync(id, cancellationToken);
        if (asset is null)
        {
            return NotFound();
        }

        var documents = await _pdfProcessingService.GetDocumentsAsync(id, cancellationToken);
        ViewBag.Documents = documents;
        return View(asset);
    }

    [HttpPost("UploadDocument")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDocument(AssetDocumentUploadViewModel model, CancellationToken cancellationToken)
    {
        if (model.DocumentFile is null || model.DocumentFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Please choose a PDF to upload.";
            return RedirectToAction(nameof(Details), new { id = model.AssetId });
        }

        var (storageUrl, extractedText) = await _pdfProcessingService.ProcessAndUploadAsync(model.DocumentFile, model.AssetId, model.DocumentType, cancellationToken);
        if (string.IsNullOrWhiteSpace(storageUrl))
        {
            TempData["ErrorMessage"] = "Unable to upload the PDF document.";
            return RedirectToAction(nameof(Details), new { id = model.AssetId });
        }

        var document = new AssetDocument
        {
            asset_id = model.AssetId,
            document_type = model.DocumentType,
            original_name = model.DocumentFile.FileName,
            storage_path = model.DocumentFile.FileName,
            storage_url = storageUrl,
            extracted_text = extractedText,
            created_at = DateTime.UtcNow
        };

        var saved = await _pdfProcessingService.SaveDocumentMetadataAsync(document, cancellationToken);
        if (!saved)
        {
            TempData["ErrorMessage"] = "The file was uploaded but metadata could not be saved.";
            return RedirectToAction(nameof(Details), new { id = model.AssetId });
        }

        TempData["SuccessMessage"] = "Document uploaded and processed successfully.";
        return RedirectToAction(nameof(Details), new { id = model.AssetId });
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var success = await _assetService.DeleteAssetAsync(id, cancellationToken);
        if (!success)
        {
            TempData["ErrorMessage"] = "Unable to delete the asset right now.";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Asset deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Assign/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(string id, string employee, CancellationToken cancellationToken)
    {
        var asset = await _assetService.GetAssetAsync(id, cancellationToken);
        if (asset is null)
        {
            return NotFound();
        }

        asset.assigned_employee = employee;
        asset.status = "Assigned";
        await _assetService.UpdateAssetAsync(asset, cancellationToken);
        TempData["SuccessMessage"] = "Asset assigned successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Return/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(string id, CancellationToken cancellationToken)
    {
        var asset = await _assetService.GetAssetAsync(id, cancellationToken);
        if (asset is null)
        {
            return NotFound();
        }

        asset.assigned_employee = string.Empty;
        asset.status = "Available";
        await _assetService.UpdateAssetAsync(asset, cancellationToken);
        TempData["SuccessMessage"] = "Asset returned successfully.";
        return RedirectToAction(nameof(Index));
    }
}
