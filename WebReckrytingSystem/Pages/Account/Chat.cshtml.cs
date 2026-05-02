using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Pages.Account
{
    [Authorize]
    public class ChatModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ChatModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string? Peer { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CompanyName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Title { get; set; }

        [BindProperty]
        public string MessageText { get; set; } = string.Empty;

        public string CurrentUserEmail { get; private set; } = string.Empty;
        public List<ConversationItem> Conversations { get; private set; } = new();
        public List<ChatMessage> Messages { get; private set; } = new();

        public class ConversationItem
        {
            public string PeerEmail { get; set; } = string.Empty;
            public string PeerAvatarUrl { get; set; } = "/images/student.png";
            public string? VacancyCompanyName { get; set; }
            public string? VacancyTitle { get; set; }
            public string LastMessage { get; set; } = string.Empty;
            public DateTime LastSentAt { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            CurrentUserEmail = userEmail;
            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSendAsync()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            CurrentUserEmail = userEmail;

            if (!string.IsNullOrWhiteSpace(Peer) && !string.IsNullOrWhiteSpace(MessageText))
            {
                _context.ChatMessages.Add(new ChatMessage
                {
                    SenderEmail = userEmail,
                    RecipientEmail = Peer,
                    Message = MessageText.Trim(),
                    VacancyCompanyName = CompanyName,
                    VacancyTitle = Title,
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { peer = Peer, companyName = CompanyName, title = Title });
        }

        public async Task<IActionResult> OnGetUpdatesAsync(string? peer, string? companyName, string? title)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return new UnauthorizedResult();
            }

            CurrentUserEmail = userEmail;
            Peer = peer;
            CompanyName = companyName;
            Title = title;

            await LoadDataAsync();

            var conversations = Conversations.Select(c => new
            {
                peerEmail = c.PeerEmail,
                peerAvatarUrl = c.PeerAvatarUrl,
                vacancyCompanyName = c.VacancyCompanyName,
                vacancyTitle = c.VacancyTitle,
                lastMessage = c.LastMessage,
                lastSentAt = c.LastSentAt
            });

            var messages = Messages.Select(m => new
            {
                senderEmail = m.SenderEmail,
                message = m.Message,
                sentAt = m.SentAt
            });

            return new JsonResult(new
            {
                currentUserEmail = CurrentUserEmail,
                selectedPeer = Peer,
                selectedCompanyName = CompanyName,
                selectedTitle = Title,
                conversations,
                messages
            });
        }

        private async Task LoadDataAsync()
        {
            var allRelated = await _context.ChatMessages
                .Where(m => m.SenderEmail == CurrentUserEmail || m.RecipientEmail == CurrentUserEmail)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            Conversations = allRelated
                .GroupBy(m => new
                {
                    PeerEmail = m.SenderEmail == CurrentUserEmail ? m.RecipientEmail : m.SenderEmail,
                    m.VacancyCompanyName,
                    m.VacancyTitle
                })
                .Select(g => new ConversationItem
                {
                    PeerEmail = g.Key.PeerEmail,
                    VacancyCompanyName = g.Key.VacancyCompanyName,
                    VacancyTitle = g.Key.VacancyTitle,
                    LastMessage = g.First().Message,
                    LastSentAt = g.First().SentAt
                })
                .OrderByDescending(x => x.LastSentAt)
                .ToList();

            var peerEmails = Conversations.Select(c => c.PeerEmail).Distinct().ToList();
            var users = await _context.Users
                .Where(u => peerEmails.Contains(u.Email))
                .ToDictionaryAsync(u => u.Email, u => u);

            foreach (var conversation in Conversations)
            {
                if (users.TryGetValue(conversation.PeerEmail, out var peerUser))
                {
                    conversation.PeerAvatarUrl = !string.IsNullOrWhiteSpace(peerUser.AvatarUrl)
                        ? peerUser.AvatarUrl
                        : GetDefaultAvatar(peerUser.Role);
                }
            }

            if (string.IsNullOrWhiteSpace(Peer) && Conversations.Any())
            {
                var first = Conversations.First();
                Peer = first.PeerEmail;
                CompanyName = first.VacancyCompanyName;
                Title = first.VacancyTitle;
            }

            if (!string.IsNullOrWhiteSpace(Peer))
            {
                var query = _context.ChatMessages.Where(m =>
                    ((m.SenderEmail == CurrentUserEmail && m.RecipientEmail == Peer) ||
                     (m.SenderEmail == Peer && m.RecipientEmail == CurrentUserEmail)));

                if (!string.IsNullOrWhiteSpace(CompanyName) || !string.IsNullOrWhiteSpace(Title))
                {
                    query = query.Where(m => m.VacancyCompanyName == CompanyName && m.VacancyTitle == Title);
                }

                Messages = await query.OrderBy(m => m.SentAt).ToListAsync();
            }
        }

        private static string GetDefaultAvatar(string? role) =>
            role == User.ROLE_EMPLOYER ? "/images/rabotodatel.jpg" : "/images/student.png";
    }
}
