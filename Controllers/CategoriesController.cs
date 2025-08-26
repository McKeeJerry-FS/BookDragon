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
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Category category)
        {
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
    }
}