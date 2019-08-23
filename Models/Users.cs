    using System.ComponentModel.DataAnnotations;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Collections.Generic;


namespace belt_exam.Models
{
    public class User
    {
        [Key]
        public int UserId {get;set;}

        [Required(ErrorMessage="Please Input Your Name")]
        public string Name {get;set;}

        [Required(ErrorMessage="Please Input Your Email")]
        [EmailAddress]
        public string Email{get;set;}

        [Required(ErrorMessage="Please Input Your Password")]
        [DataType(DataType.Password)]
        [MinLength(8,ErrorMessage="Password must be at least 8 characters ")]
        [RegularExpression("^(?=.{8,})(?=.*[a-z])(?=.*[0-9])(?=.*[@#$%^&+=!]).*$",ErrorMessage="Password Must Have At Least one number and One Special")]
        public string Password{get;set;}

        [NotMapped]
        [Required(ErrorMessage="Please Input Confirmation Password")]
        [Compare("Password",ErrorMessage="Passwords Must Be Matched")]
        [DataType(DataType.Password)]
        public string Confirm {get;set;}

        public List<Response> Responses {get;set;}

        public string WeddingId {get;set;}


    }
    
    public class Act{
        [Key]
        public int ActivityId{get;set;}

        [Required(ErrorMessage="Please Input Your Title")]
        public string Title{get;set;}

        [Required(ErrorMessage="Please Input Your Time")]
        public string Time{get;set;}

        [Required(ErrorMessage="Please Input Your Duration")]
        public string Duration{get;set;}

        [NotMapped]
        public string Hours{get;set;}

        [Required(ErrorMessage="Please Input Your Date")]
        [DisplayFormat(DataFormatString = "{yyyyMMdd}")]
        [DataType(DataType.Date)]
        public DateTime Date{get;set;}

        [Required(ErrorMessage="Please Input Your Description")]
        public string ActivityDescription {get;set;}
        public List<Response> Participants {get;set;}

        public int UserId {get;set;}
        public User ThisUser{get;set;}
    }
    public class Response{
    [Key]
    public int ResponseId {get;set;}
    
    public String Guests {get;set;}

    public Act MyActivity {get;set;}

    public User MyUser{get;set;}
    
    public int ActivityId {get;set;}
    public int UserId {get;set;}

    }
    public class Login{

        [EmailAddress]
        public string Email {get; set;}

        [DataType(DataType.Password)]
        public string Password { get; set; }

        }
}
