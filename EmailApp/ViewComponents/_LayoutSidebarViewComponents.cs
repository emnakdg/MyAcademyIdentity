using EmailApp.Context;
using EmailApp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EmailApp.ViewComponents
{
    public class _LayoutSidebarViewComponents(UserManager<AppUser> _userManager, AppDbContext _context) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            ViewBag.InboxMessage = _context.Messages.Include(x => x.Sender).Where(x => x.ReceiverId == user.Id && x.IsDeleted == false).Count();
            ViewBag.sendboxMessage = _context.Messages.Include(x => x.Receiver).Where(x => x.SenderId == user.Id).Count();

            ViewBag.readMessage = _context.Messages.Include(x => x.Sender).Where(x => x.ReceiverId == user.Id && x.IsRead == true).Count();
            ViewBag.unreadMessage = _context.Messages.Include(x => x.Sender).Where(x => x.ReceiverId == user.Id && x.IsRead == false).Count();

            ViewBag.deletedMessage = _context.Messages.Include(x => x.Sender).Where(x => x.ReceiverId == user.Id && x.IsDeleted == true).Count();

            ViewBag.draftMessage = _context.Messages.Include(x => x.Receiver).Where(x => x.SenderId == user.Id && x.IsDraft == true).Count();

            return View();
        }
    }
}
