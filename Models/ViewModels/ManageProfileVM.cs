namespace MovieMart.Models.ViewModels
{
    public class ManageProfileVM
    {
        public string Id { get; set; }


        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }


        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }


        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string UserName { get; set; }


        [Display(Name = "Address")]
        public string Address { get; set; }


        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

    }
}
