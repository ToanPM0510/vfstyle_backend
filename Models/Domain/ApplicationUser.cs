using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;

namespace vfstyle_backend.Models.Domain
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool EmailVerified { get; set; } = true;

        // Navigation properties
        public virtual ICollection<CustomerPreference> CustomerPreferences { get; set; }
        public virtual ICollection<ProductRecommendation> ProductRecommendations { get; set; }
        public virtual ICollection<Conversation> Conversations { get; set; }
    }
}
