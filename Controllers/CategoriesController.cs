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
            if (category.Id != 0)
            {
                _logger.LogWarning("[Category Create] Non-zero Id supplied ({Id}) – resetting to 0.", category.Id);
                category.Id = 0;
            }

            _logger.LogInformation("[Category Create] Attempting create. Name={Name} DescriptionLength={DescLen}", category?.Name, category?.Description?.Length);

            if (!ModelState.IsValid)
            {
                foreach (var kvp in ModelState.Where(k => k.Value?.Errors?.Count > 0))
                    foreach (var err in kvp.Value!.Errors)
                        _logger.LogWarning("[Category Create] ModelState error key='{Key}' message='{Error}'", kvp.Key, err.ErrorMessage);
                return View(category);
            }

            try
            {
                // Atomic sequence health check/repair BEFORE insert
                await EnsureCategorySequenceHealthyAsync();

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

                if (IsPrimaryKeyViolation(ex))
                {
                    _logger.LogWarning("[Category Create] PK violation. Re-running sequence repair and retrying once.");
                    try
                    {
                        await EnsureCategorySequenceHealthyAsync();
                        _context.Add(category);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("[Category Create] Success after retry Id={Id} Name={Name}", category.Id, category.Name);
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
            if (id == null) return NotFound();
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category category)
        {
            if (id != category.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                foreach (var kvp in ModelState.Where(k => k.Value?.Errors?.Count > 0))
                    foreach (var err in kvp.Value!.Errors)
                        _logger.LogWarning("[Category Edit] ModelState error key='{Key}' message='{Error}'", kvp.Key, err.ErrorMessage);
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
                    _logger.LogWarning("[Category Edit] Concurrency failed. Not found Id={Id}", category.Id);
                    return NotFound();
                }
                throw;
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
            if (id == null) return NotFound();
            var category = await _context.Categories.FirstOrDefaultAsync(m => m.Id == id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null) _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            _logger.LogInformation("[Category Delete] Deleted Id={Id}", id);
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id) => _context.Categories.Any(e => e.Id == id);

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
                    operation, pg.SqlState, pg.MessageText, pg.Detail, pg.TableName, pg.ColumnName, pg.ConstraintName, category?.Name, entrySummaries);
            }
            else
            {
                _logger.LogError(ex, "[Category {Operation}] DbUpdateException Name={Name} EntryStates={@Entries}", operation, category?.Name, entrySummaries);
            }
        }

        private bool IsPrimaryKeyViolation(DbUpdateException ex)
            => ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation && (pg.ConstraintName?.Equals("PK_Categories", StringComparison.OrdinalIgnoreCase) ?? false);

        // New atomic sequence check/repair using DO block
        private async Task EnsureCategorySequenceHealthyAsync()
        {
            const string sql = @"DO $$
DECLARE
    seq_name text := pg_get_serial_sequence('""Categories""','Id');
    max_id   bigint;
    last_val bigint;
    is_called boolean;
BEGIN
    IF seq_name IS NULL THEN
        RAISE NOTICE 'Sequence not found for Categories.Id';
        RETURN;
    END IF;

    SELECT COALESCE(MAX(""Id""),0) INTO max_id FROM ""Categories"";

    EXECUTE format('SELECT last_value, is_called FROM %I', seq_name)
        INTO last_val, is_called;

    IF last_val <= max_id THEN
        EXECUTE format('SELECT setval(%L, %s, false)', seq_name, (max_id + 1)::text);
    END IF;
END $$;";
            try
            {
                await _context.Database.ExecuteSqlRawAsync(sql);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Category Seq] DO block sequence repair failed (non-fatal).");
            }
        }
    }
}