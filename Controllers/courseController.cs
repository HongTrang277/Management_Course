using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ManagementCenter.Data;
using ManagementCenter.Models;
using Microsoft.AspNetCore.Authorization;

namespace ManagementCenter.Controllers
{
    [Authorize(Roles = "Admin")]
    public class courseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public courseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: course
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string searchString)
        {
            var coursesQuery = _context.course.Select(c => c);
            if (!String.IsNullOrEmpty(searchString))
            {
                coursesQuery = coursesQuery.Where(c => c.course_name.ToUpper().Contains(searchString.ToUpper())
                                                    || c.tutor.ToUpper().Contains(searchString.ToUpper()));
            }
            return View(await coursesQuery.ToListAsync());
        }

        // GET: course/Details/5
        [AllowAnonymous]
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
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: course/Create
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("course_id,tutor,course_name,start_date,fee,max_capacity,schedule,ImageUrl")] course course)
        {
            ModelState.Remove(nameof(course.registrations));
            if (ModelState.IsValid) // Kiểm tra validation cơ bản trước
            {
                // --- BẮT ĐẦU KIỂM TRA TRÙNG LẶP ---
                bool courseExists = await _context.course.AnyAsync(c =>
                    // So sánh tên khóa học không phân biệt hoa thường
                    c.course_name.ToUpper() == course.course_name.ToUpper() &&
                    // So sánh tên giảng viên không phân biệt hoa thường, xử lý cả trường hợp null
                    (c.tutor ?? "").ToUpper() == (course.tutor ?? "").ToUpper()
                );

                if (courseExists)
                {
                    // Nếu tìm thấy trùng lặp, thêm lỗi vào ModelState
                    // dùng string.Empty để lỗi hiển thị ở ValidationSummary chung
                    ModelState.AddModelError(string.Empty, "Khóa học này đã có trên hệ thống!");
                    // Trả về View để người dùng sửa lại, giữ nguyên dữ liệu đã nhập
                    return View(course);
                }
                // --- KẾT THÚC KIỂM TRA TRÙNG LẶP ---

                // Nếu không trùng lặp, tiến hành thêm và lưu
                _context.Add(course);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo khóa học thành công!"; // Thông báo thành công (tùy chọn)
                return RedirectToAction(nameof(Index)); // Chuyển hướng về trang danh sách
            }

            return View(course);
        }

        // GET: course/Edit/5
        [Authorize(Roles = "Admin")]
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
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("course_id,tutor,course_name,start_date,fee,max_capacity,schedule,ImageUrl")] course course)
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
