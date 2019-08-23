using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using belt_exam.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace belt_exam.Controllers
{
    
    public class HomeController : Controller
    {
        private MyContext dbContext;

        // here we can "inject" our context service into the constructor
        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("/register")]
        public IActionResult Register(User adduser){
        if(ModelState.IsValid){
                if(dbContext.Users.Any(u => u.Email == adduser.Email))
        {
            // Manually add a ModelState error to the Email field, with provided
            // error message
            ModelState.AddModelError("Email", "Email already in use!");
            return View("Index");
            
        }
         // Initializing a PasswordHasher object, providing our User class as its
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                adduser.Password = Hasher.HashPassword(adduser, adduser.Password);
                //Save your user object to the database
                dbContext.Add(adduser);
                dbContext.SaveChanges();

                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == adduser.Email);
                HttpContext.Session.SetInt32("ID",userInDb.UserId);
                return RedirectToAction("Dashboard",new{id = userInDb.UserId});
            }
            return View("Index");
        }
        
        [HttpPost("LogIn")]
        public IActionResult LogIn(Login thisuser){
        if(ModelState.IsValid)
        {
            // If inital ModelState is valid, query for a user with provided email
            var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == thisuser.Email);
            // If no user exists with provided email
            if(userInDb == null)
            {
                // Add an error to ModelState and return to View!
                ModelState.AddModelError("Email", "Invalid Email/Password");
                return View("Index");
            }
            
            // Initialize hasher object
            var hasher = new PasswordHasher<Login>();
            
            // verify provided password against hash stored in db
            var result = hasher.VerifyHashedPassword(thisuser, userInDb.Password, thisuser.Password);
            
            // result can be compared to 0 for failure
            if(result == 0)
            {
                ModelState.AddModelError("Password","Invalid Email/Passwordz");
                return View("Index");
                // handle failure (this should be similar to how "existing email" is handled)
            }
            else{
                HttpContext.Session.SetInt32("ID",userInDb.UserId);
                return RedirectToAction("Dashboard",new{id = userInDb.UserId});
            }
        }
        return View("Index");
        }
        
        [HttpGet("Dashboard/{id}")]
        public IActionResult Dashboard(int id){
        int? myid = HttpContext.Session.GetInt32("ID");
            if( myid != id ){
                return View("Index");
            }
            if(HttpContext.Session.GetString("Error")=="stuff"){
                ViewBag.Error = true;
            }
            var stuff = dbContext.Users.FirstOrDefault(x => x.UserId ==id);
            ViewBag.Name = stuff.Name;
            ViewBag.Id = myid;
            //Show All Weddings
            // List <Wedding> mystuff = dbContext.Weddings.Where(x => x.WeddingId !=0).ToList();
            List <Act> mystuff = dbContext.Activities.Include(z=> z.ThisUser).Include(x => x.Participants).ThenInclude(p => p.MyUser).OrderBy(c => c.Date.Date).Where(x=>x.Date>=DateTime.Now).ToList();
            ViewBag.theId = id;
            
            HttpContext.Session.SetString("Error","Not Error");
            
            return View("Dashboard",mystuff);
        }
        [HttpGet("/join/{id}")]
        public IActionResult Join(int id){
            //  the id is the Parti ID
            int? myid = HttpContext.Session.GetInt32("ID");
            ViewBag.myidzz = myid;
            var myguy = dbContext.Users.FirstOrDefault(x => x.UserId ==myid);
            var abc = dbContext.Activities.Where(x=>x.UserId ==myid).ToList();
            string myname = myguy.Name;
            var theguy = dbContext.Activities.FirstOrDefault(x => x.ActivityId ==id);
                for(var i =0; i < abc.Count; i ++){
                    if(abc[i].Date == theguy.Date){
                        HttpContext.Session.SetString("Error","stuff");
                        return RedirectToAction("Dashboard",new{id = myid});
                    }
                }
            int Userid = theguy.UserId;
            Response myresponse = new Response();
            myresponse.Guests = myname;
            myresponse.UserId = ViewBag.myidzz;
            myresponse.ActivityId = id;
            dbContext.Add(myresponse);
            dbContext.SaveChanges();

            return RedirectToAction("Dashboard",new{id = myid});
        }
        [HttpGet("/cancel/{id}")]
        public IActionResult Cancel(int id){
             //id is Wedding Id
            int? myid = HttpContext.Session.GetInt32("ID");
            if(myid ==null){
                return View("Index");
            }
            Response myresponse = dbContext.Responses.FirstOrDefault(x => x.ActivityId == id && x.UserId ==myid);
            dbContext.Remove(myresponse);
            dbContext.SaveChanges();

            return RedirectToAction("Dashboard",new{id = myid});
        }
        [HttpGet("/delete/{id}")]
        public IActionResult Delete(int id){
            int? myid = HttpContext.Session.GetInt32("ID");
            if(myid ==null){
                return View("Index");
            }
              //id is WeddingId
            Act CancelthePlan = dbContext.Activities.FirstOrDefault(x=>x.ActivityId == id);
            dbContext.Remove(CancelthePlan);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard",new{id = myid });
        }
        [HttpGet("/actdetail/{id}")]
        public IActionResult ActDetail(int id){
            int? myid = HttpContext.Session.GetInt32("ID");
            if(myid ==null){
                return View("Index");
            }
            // Activity ID is id 

                ViewBag.Id = HttpContext.Session.GetInt32("ID");
                    Act stuff = dbContext.Activities.Include(x => x.Participants).FirstOrDefault(x=> x.ActivityId==id);
                    int thecreater = stuff.UserId;
                    User theuser = dbContext.Users.FirstOrDefault(x=> x.UserId == stuff.UserId);
                    ViewBag.Name = theuser.Name;

            // List <Act> mystuff = dbContext.Activities.Include(z=> z.ThisUser).Include(x => x.Participants).ThenInclude(p => p.MyUser).OrderBy(c => c.Date.Date).Where(x=>x.Date>=DateTime.Now).ToList();
                    
            return View("TheActivity",stuff);
        }

        [HttpPost("/theactivity")]
        public IActionResult Activities(Act mywedding){
            if(HttpContext.Session.GetInt32("ID")==null){
                return View("Index");
            }
            if(ModelState.IsValid){
                ViewBag.Id = HttpContext.Session.GetInt32("ID");
                mywedding.UserId = ViewBag.Id;
                mywedding.Duration = mywedding.Duration + " " + mywedding.Hours;

                // Pure LogIC
                string now = DateTime.Now.ToString("yyyyMMdd");
                string dobchef = mywedding.Date.ToString("yyyyMMdd");
                
                string dobchefyear="";
                string dobchefmonth="";
                string dobchefdate="";
                string nowofyear="";
                string nowofmonth="";
                string nowofdate="";


                for(int i =0; i <4; i ++){
                    dobchefyear += dobchef[i];
                }
                for(int i =4; i<6;i++){
                    dobchefmonth +=dobchef[i];
                }
                for(int i =6; i<8;i++){
                    dobchefdate +=dobchef[i];
                }
                //----------------------------------------------------------------
                for(int i =0; i <4; i ++){
                    nowofyear += now[i];
                }
                for(int i =4; i<6;i++){
                    nowofmonth +=now[i];
                }
                for(int i =6; i<8;i++){
                    nowofdate +=now[i];
                }

                int nowofyear2 = Int32.Parse(nowofyear);
                int dobchefyear2 = Int32.Parse(dobchefyear);

                int nowofmonth2 = Int32.Parse(nowofmonth);
                int dobchefmonth2 = Int32.Parse(dobchefmonth);

                int nowofdate2 = Int32.Parse(nowofdate);
                int dobchefdate2 = Int32.Parse(dobchefdate);

                if(dobchefdate2<=nowofdate2){
                    if(dobchefmonth2<=nowofmonth2){
                        if(dobchefyear2<=nowofyear2){
                            ModelState.AddModelError("Date","Date Must Be In The Future");
                                return View("MyActivity");
                        }
                    }
                }
                // Pure Logic End
                
                dbContext.Add(mywedding);
                dbContext.SaveChanges();
                var stuff = dbContext.Activities.FirstOrDefault(x=> x.Title == mywedding.Title);
                var stuff2 = dbContext.Activities.FirstOrDefault(x=> x.ActivityDescription == mywedding.ActivityDescription);
                if(stuff == stuff2){
                    HttpContext.Session.SetInt32("AID",stuff.ActivityId);
                        return RedirectToAction("ActDetail",new{id =stuff.ActivityId});
                }
                else{
                    return View("MyActivity");
                }

                // return RedirectToAction("WeddingDetail",new{id = stuff.WeddingId});

                
            }
            else{
                return View("MyActivity");
            }
        }

        [HttpGet("/addact/{id}")]
    
        public IActionResult AddActivity(int id){
            if(HttpContext.Session.GetInt32("ID") != id){
                return View("Index");
            }

            ViewBag.Id = HttpContext.Session.GetInt32("ID");
            
            return View("MyActivity");
        }


        [HttpGet("logout")]
        public IActionResult LogOut(){
            HttpContext.Session.Clear();
            return View("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
