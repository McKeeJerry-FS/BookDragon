using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookDragon.Data;
using BookDragon.Models;
using Microsoft.Extensions.Logging;
using Npgsql; // for Postgres specific exception details

namespace BookDragon.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ApplicationDbContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Category category)
        {
            // Force new entity (prevent client-supplied Id collisions)
            if (category.Id != 0)
            {
                _logger.LogWarning("[Category Create] Non-zero Id supplied ({Id}) – resetting to 0.", category.Id);
                category.Id = 0;
            }

            _logger.LogInformation("[Category Create] Attempting create. Name={Name} DescriptionLength={DescLen}", category?.Name, category?.Description?.Length);

            if (!ModelState.IsValid)
            {
                foreach (var kvp in ModelState.Where(k => k.Value?.Errors?.Count > 0))
                {
                    foreach (var err in kvp.Value!.Errors)
                    {
                        _logger.LogWarning("[Category Create] ModelState error key='{Key}' message='{Error}'", kvp.Key, err.ErrorMessage);
                    }
                }
                return View(category);
            }

            try
            {
                // Check and repair sequence BEFORE insert
                await EnsureCategorySequenceHealthyAsync();

                // Optional normalization
                category.Name = category.Name?.Trim();
                category.Description = category.Description?.Trim();

                _context.Add(category);
                await _context.SaveChangesAsync();
                _logger.LogInformation("[Category Create] Success Id={Id} Name={Name}", category.Id, category.Name);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                LogDbUpdateException("Create", ex, category);

                // If duplicate PK, attempt one automatic sequence repair & retry once
                if (IsPrimaryKeyViolation(ex))
                {
                    _logger.LogWarning("[Category Create] Detected PK violation. Attempting sequence repair and retry.");
                    try
                    {
                        await RepairCategorySequenceAsync(force: true);
                        _context.Add(category);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("[Category Create] Success after sequence repair. Id={Id} Name={Name}", category.Id, category.Name);
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception retryEx)
                    {
                        _logger.LogError(retryEx, "[Category Create] Retry after sequence repair failed.");
                    }
                }

                ModelState.AddModelError(string.Empty, "A database error occurred saving the category. Check logs for details.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Category Create] Unexpected exception Name={Name}", category.Name);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            }

            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                foreach (var kvp in ModelState.Where(k => k.Value?.Errors?.Count > 0))
                {
                    foreach (var err in kvp.Value!.Errors)
                    {
                        _logger.LogWarning("[Category Edit] ModelState error key='{Key}' message='{Error}'", kvp.Key, err.ErrorMessage);
                    }
                }
                return View(category);
            }

            try
            {
                category.Name = category.Name?.Trim();
                category.Description = category.Description?.Trim();

                _context.Update(category);
                await _context.SaveChangesAsync();
                _logger.LogInformation("[Category Edit] Success Id={Id} Name={Name}", category.Id, category.Name);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(category.Id))
                {
                    _logger.LogWarning("[Category Edit] Concurrency failed. Category not found Id={Id}", category.Id);
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (DbUpdateException ex)
            {
                LogDbUpdateException("Edit", ex, category);
                ModelState.AddModelError(string.Empty, "A database error occurred updating the category. Check logs for details.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Category Edit] Unexpected exception Id={Id} Name={Name}", category.Id, category.Name);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            }

            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("[Category Delete] Deleted Id={Id}", id);
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }

        private void LogDbUpdateException(string operation, DbUpdateException ex, Category category)
        {
            var entrySummaries = ex.Entries.Select(e => new
            {
                Entity = e.Entity.GetType().Name,
                State = e.State.ToString(),
                CurrentValues = e.CurrentValues.Properties.ToDictionary(p => p.Name, p => e.CurrentValues[p]?.ToString())
            });

            if (ex.InnerException is PostgresException pg)
            {
                _logger.LogError(ex,
                    "[Category {Operation}] PostgresException Code={Code} Message={MessageText} Detail={Detail} Table={Table} Column={Column} Constraint={Constraint} Name={Name} EntryStates={@Entries}",
                    operation,
                    pg.SqlState,
                    pg.MessageText,
                    pg.Detail,
                    pg.TableName,
                    pg.ColumnName,
                    pg.ConstraintName,
                    category?.Name,
                    entrySummaries);
            }
            else
            {
                _logger.LogError(ex, "[Category {Operation}] DbUpdateException Name={Name} EntryStates={@Entries}", operation, category?.Name, entrySummaries);
            }
        }

        private bool IsPrimaryKeyViolation(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pg)
            {
                // 23505 unique_violation
                return pg.SqlState == PostgresErrorCodes.UniqueViolation && (pg.ConstraintName?.Equals("PK_Categories", StringComparison.OrdinalIgnoreCase) ?? false);
            }
            return false;
        }

        private async Task EnsureCategorySequenceHealthyAsync()
        {
            try
            {
                await RepairCategorySequenceAsync(force: false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Category Seq] Sequence health check failed (non-fatal)." );
            }
        }

        private async Task RepairCategorySequenceAsync(bool force)
        {
            // Find sequence name dynamically (handles future refactors)
            var seqName = await _context.Database.SqlQueryRaw<string>("SELECT pg_get_serial_sequence('\"Categories\"','Id')").FirstAsync();
            if (string.IsNullOrWhiteSpace(seqName))
            {
                _logger.LogWarning("[Category Seq] Could not resolve sequence name for Categories.Id");
                return;
            }

            var maxId = await _context.Categories.MaxAsync(c => (int?)c.Id) ?? 0;

            // Get current sequence info
            var seqInfo = await _context.Database
                .SqlQueryRaw<(long last_value, bool is_called)>($"SELECT last_value, is_called FROM {seqName}")
                .FirstAsync();

            var lastValue = seqInfo.last_value;
            // If sequence not yet called after creation, treat last_value as (start - 1)
            if (!seqInfo.is_called && lastValue == 1 && maxId == 0)
            {
                _logger.LogDebug("[Category Seq] Sequence untouched and table empty - no action needed.");
                return;
            }

            if (force || lastValue <= maxId)
            {
                var newVal = maxId + 1;
                _logger.LogWarning("[Category Seq] Repairing sequence {Seq} last_value={Last} maxId={Max} setting to {New}", seqName, lastValue, maxId, newVal);
                // setval(seq, value, is_called=false) so next nextval returns value
                await _context.Database.ExecuteSqlRawAsync($"SELECT setval('{{0}}', {{1}}, false)", seqName, newVal);
            }
            else
            {
                _logger.LogDebug("[Category Seq] Sequence healthy. seq={Seq} last={Last} maxId={Max}", seqName, lastValue, maxId);
            }
        }
    }
}