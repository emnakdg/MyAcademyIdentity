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

        private async Task SetMessageCounts()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user != null)
            {
                ViewBag.InboxMessage = _context.Messages.Count(x => x.ReceiverId == user.Id && x.IsDeleted == false);
                ViewBag.sendboxMessage = _context.Messages.Count(x => x.SenderId == user.Id);
                ViewBag.readMessage = _context.Messages.Count(x => x.ReceiverId == user.Id && x.IsRead == true);
                ViewBag.unreadMessage = _context.Messages.Count(x => x.ReceiverId == user.Id && x.IsRead == false);
                ViewBag.deletedMessage = _context.Messages.Count(x => x.ReceiverId == user.Id && x.IsDeleted == true);
                ViewBag.draftMessage = _context.Messages.Count(x => x.SenderId == user.Id && x.IsDraft == true);
                ViewBag.importantMessage = _context.Messages.Count(x => x.ReceiverId == user.Id && x.IsImportant == true && x.IsDeleted == false);
            }
        }

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

        public IActionResult MarkAsImportant(int id)
        {
            var message = _context.Messages.Find(id);
            if (message is not null)
            {
                message.IsImportant = true;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult MarkAsNotImportant(int id)
        {
            var message = _context.Messages.Find(id);
            if (message is not null)
            {
                message.IsImportant = false;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ImportantMessages(int page = 1)
        {
            await SetMessageCounts();

            var userName = User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await _userManager.FindByNameAsync(userName);
            ViewBag.UserFirstName = user.FirstName;
            ViewBag.UserLastName = user.LastName;
            if (user == null)
            {
                return View(new List<Message>());
            }

            // Important count'u ekle
            ViewBag.importantCount = _context.Messages.Count(x => x.ReceiverId == user.Id && x.IsImportant == true && x.IsDeleted == false);

            int pageSize = 5;
            int skip = (page - 1) * pageSize;

            var totalMessages = await _context.Messages
                .Where(x => x.ReceiverId == user.Id && x.IsImportant == true && x.IsDeleted == false)
                .CountAsync();

            var messages = await _context.Messages
                .AsNoTracking()
                .Include(x => x.Sender)
                .Where(x => x.ReceiverId == user.Id && x.IsImportant == true && x.IsDeleted == false)
                .OrderByDescending(x => x.SendDate)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalMessages / pageSize);
            ViewBag.TotalMessages = totalMessages;

            return View(messages);
        }

    }
}
