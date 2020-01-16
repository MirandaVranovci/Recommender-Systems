
namespace Recommender_system.Models
{
    public class ProfiliViewModel
    {
        public ProfiliViewModel()
        {
            users = new USER();
            psw = new ChangePasswordViewModel();
        }
        public USER users { get; set; }
        public ChangePasswordViewModel psw { get; set; }
    }
}