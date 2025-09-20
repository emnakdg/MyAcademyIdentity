using EmailApp.Context;
using EmailApp.Entities;
using EmailApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmailApp.Controllers
{
    [Authorize]
    public class MessageController(AppDbContext _context, UserManager<AppUser> _userManager) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var messages = _context.Messages.Include(x => x.Sender).Where(x => x.ReceiverId == user.Id && x.IsDeleted == false).ToList();
            ViewBag.UserFirstName = user.FirstName;
            ViewBag.UserLastName = user.LastName;
            return View(messages);
        }
        public async Task<IActionResult> Sendbox()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            ViewBag.UserFirstName = user.FirstName;
            ViewBag.UserLastName = user.LastName;
            var messages = _context.Messages.Include(x => x.Receiver).Where(x => x.SenderId == user.Id).ToList();
            return View(messages);
        }

        public IActionResult MessageDetail(int id)
        {
            var message = _context.Messages.Include(x => x.Sender).FirstOrDefault(x => x.MessageId == id);
            return View(message);
        }

        public async Task<IActionResult> SendMessage()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            ViewBag.UserFirstName = user.FirstName;
            ViewBag.UserLastName = user.LastName;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageViewModel model)
        {
            var sender = await _userManager.FindByNameAsync(User.Identity.Name);
            var receiver = await _userManager.FindByEmailAsync(model.ReceiverEmail);

            var message = new Message
            {
                Body = model.Body,
                Subject = model.Subject,
                ReceiverId = receiver.Id,
                SenderId = sender.Id,
                SendDate = DateTime.Now,
            };

            _context.Messages.Add(message);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> DraftMessage(SendMessageViewModel model)
        {
            var sender = await _userManager.FindByNameAsync(User.Identity.Name);
            var receiver = await _userManager.FindByEmailAsync(model.ReceiverEmail);
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            ViewBag.UserFirstName = user.FirstName;
            ViewBag.UserLastName = user.LastName;

            // Null kontrolü ekleyin
            if (receiver == null)
            {
                ModelState.AddModelError("ReceiverEmail", "Bu email adresine sahip kullanıcı bulunamadı.");
                return View(model);
            }

            var message = new Message
            {
                Body = model.Body,
                Subject = model.Subject,
                ReceiverId = receiver.Id,
                SenderId = sender.Id,
                SendDate = DateTime.Now,
                IsDraft = true,
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Message");
        }

        public async Task<IActionResult> DraftList()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            ViewBag.UserFirstName = user.FirstName;
            ViewBag.UserLastName = user.LastName;
            string Emailvalue = user.Email;
            var messages = _context.Messages.Include(x => x.Receiver).Where(x => x.SenderId == user.Id && x.IsDraft == true).ToList();
            return View(messages);
        }
        public async Task<IActionResult> ReadList()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            ViewBag.UserFirstName = user.FirstName;
            ViewBag.UserLastName = user.LastName;
            string Emailvalue = user.Email;
            var messages = _context.Messages.Include(x => x.Sender).Where(x => x.ReceiverId == user.Id && x.IsRead == true).ToList();
            return View(messages);
        }
        public async Task<IActionResult> UnReadList()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            ViewBag.UserFirstName = user.FirstName;
            ViewBag.UserLastName = user.LastName;
            string Emailvalue = user.Email;
            var messages = _context.Messages.Include(x => x.Sender).Where(x => x.ReceiverId == user.Id && x.IsRead == false).ToList();
            return View(messages);
        }


        public IActionResult ChangeMessageIsReadToDeleted(int id)
        {
            var value = _context.Messages.Find(id);
            value.IsDeleted = true;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult ChangeMessageIsReadToDeletedFalse(int id)
        {
            var value = _context.Messages.Find(id);
            value.IsDeleted = false;
            _context.SaveChanges();
            return RedirectToAction("DeletedList", "Message");
        }

        public IActionResult ChangeMessageIsReadToTrue(int id)
        {
            var value = _context.Messages.Find(id);
            value.IsRead = true;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult ChangeMessageIsReadToFalse(int id)
        {
            var value = _context.Messages.Find(id);
            value.IsRead = false;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeletedList()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            ViewBag.UserFirstName = user.FirstName;
            ViewBag.UserLastName = user.LastName;
            string Emailvalue = user.Email;
            var messages = _context.Messages.Include(x => x.Sender).Where(x => x.ReceiverId == user.Id && x.IsDeleted == true).ToList();
            return View(messages);
        }

    }
}
