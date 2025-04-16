using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ManagementCenter.Data;
using ManagementCenter.Models;

namespace ManagementCenter.Controllers
{
    public class courseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public courseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: course
        public async Task<IActionResult> Index()
        {
            return View(await _context.course.ToListAsync());
        }

        // GET: course/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.course
                .FirstOrDefaultAsync(m => m.course_id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: course/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: course/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("course_id,tutor,course_name,start_date,fee,max_capacity,schedule")] course course)
        {
            ModelState.Remove(nameof(course.registrations));
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        // GET: course/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.course.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // POST: course/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("course_id,tutor,course_name,start_date,fee,max_capacity,schedule")] course course)
        {
            if (id != course.course_id)
            {
                return NotFound();
            }
            ModelState.Remove(nameof(course.registrations));
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!courseExists(course.course_id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        // GET: course/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.course
                .FirstOrDefaultAsync(m => m.course_id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: course/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.course.FindAsync(id);
            if (course != null)
            {
                _context.course.Remove(course);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool courseExists(int id)
        {
            return _context.course.Any(e => e.course_id == id);
        }
    }
}
