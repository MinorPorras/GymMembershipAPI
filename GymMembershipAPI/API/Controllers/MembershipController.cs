using Microsoft.AspNetCore.Mvc;

namespace GymMembershipAPI.API.Controllers;

public class MembershipController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
    
    //TODO: Add al method for respective service
}