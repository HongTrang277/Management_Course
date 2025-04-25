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
    public class registrationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public registrationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: registration
        public async Task<IActionResult> Index()
        {
            var allRegistrations = await _context.registration 
         .Include(r => r.course)         
         .Include(r => r.student)        
             .ThenInclude(s => s.ApplicationUser) 
         .OrderByDescending(r => r.registration_date) 
         .ToListAsync();

            return View(allRegistrations); 
        }

        // GET: registration/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registration = await _context.registration
                .Include(r => r.course)
                .Include(r => r.student)
                .FirstOrDefaultAsync(m => m.registration_id == id);
            if (registration == null)
            {
                return NotFound();
            }

            return View(registration);
        }

        // GET: registration/Create
        public IActionResult Create()
        {
            ViewData["course_id"] = new SelectList(_context.course, "course_id", "course_name");
            ViewData["student_id"] = new SelectList(_context.student, "student_id", "password_hash");
            return View();
        }

        // POST: registration/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("registration_id,student_id,course_id,registration_date,status,completion_date,cancellation_date")] registration registration)
        {
            if (ModelState.IsValid)
            {
                _context.Add(registration);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["course_id"] = new SelectList(_context.course, "course_id", "course_name", registration.course_id);
            ViewData["student_id"] = new SelectList(_context.student, "student_id", "password_hash", registration.student_id);
            return View(registration);
        }

        // GET: registration/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registration = await _context.registration.FindAsync(id);
            if (registration == null)
            {
                return NotFound();
            }
            ViewData["course_id"] = new SelectList(_context.course, "course_id", "course_name", registration.course_id);
            ViewData["student_id"] = new SelectList(_context.student, "student_id", "password_hash", registration.student_id);
            return View(registration);
        }

        // POST: registration/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("registration_id,student_id,course_id,registration_date,status,completion_date,cancellation_date")] registration registration)
        {
            if (id != registration.registration_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(registration);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!registrationExists(registration.registration_id))
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
            ViewData["course_id"] = new SelectList(_context.course, "course_id", "course_name", registration.course_id);
            ViewData["student_id"] = new SelectList(_context.student, "student_id", "password_hash", registration.student_id);
            return View(registration);
        }

        // GET: registration/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registration = await _context.registration
                .Include(r => r.course)
                .Include(r => r.student)
                .FirstOrDefaultAsync(m => m.registration_id == id);
            if (registration == null)
            {
                return NotFound();
            }

            return View(registration);
        }

        // POST: registration/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var registration = await _context.registration.FindAsync(id);
            if (registration != null)
            {
                _context.registration.Remove(registration);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool registrationExists(int id)
        {
            return _context.registration.Any(e => e.registration_id == id);
        }
    }
}
