using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Mvc;
using CVGSProject.Models;
using static CVGSProject.Models.AdminModel;
using System.Web.Helpers;
using System.Net;
using Microsoft.Ajax.Utilities;

namespace CVGSProject.Controllers {
    public class HomeController : Controller {
        public string connectionString = "data source=LAPTOP-UO2GBQ2N\\SQLEXPRESS;initial catalog=CVSG;trusted_connection=true";
        [HttpGet]
        public ActionResult Index(string search = null) {
            // Landing Page
            Games games = new Games() { GamesList = new List<Game>() };
            using (DBManager db = new DBManager(connectionString)) {
                DataSet DbSet = db.GetSelectQuery(new List<SqlParameter>() {
                    new SqlParameter("@search", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "search", DataRowVersion.Current, search.IsNullOrWhiteSpace()? null : search),
                }, "dbo.uspGetGamesList");
                for (int i = 0; i < DbSet.Tables[0].Rows.Count; i++) {
                    games.GamesList.Add(new Game() {
                        GameId = int.Parse(DbSet.Tables[0].Rows[i][0].ToString()),
                        GameTitle = DbSet.Tables[0].Rows[i][1].ToString(),
                        GamePublisher = DbSet.Tables[0].Rows[i][2].ToString(),
                        GameDescription = DbSet.Tables[0].Rows[i][3].ToString(),
                        GameRating = "N/A"
                    });
                }
                List<RatingHelper> ratings = new List<RatingHelper>();
                for (int i = 0; i < DbSet.Tables[1].Rows.Count; i++) {
                    if(ratings.Any(x => x.GameId == int.Parse(DbSet.Tables[1].Rows[i][0].ToString()))) {
                        ratings.First(x => x.GameId == int.Parse(DbSet.Tables[1].Rows[i][0].ToString()))
                            .Ratings += int.Parse(DbSet.Tables[1].Rows[i][1].ToString());
                        ratings.First(x => x.GameId == int.Parse(DbSet.Tables[1].Rows[i][0].ToString()))
                            .AmountOfRatings += 1;
                    }
                    else {
                        ratings.Add(new RatingHelper() {
                            GameId = int.Parse(DbSet.Tables[1].Rows[i][0].ToString()),
                            Ratings = int.Parse(DbSet.Tables[1].Rows[i][1].ToString()),
                            AmountOfRatings = 1
                        });
                    }
                }
                foreach (RatingHelper helper in ratings) {
                    if (games.GamesList.Any(x => x.GameId == helper.GameId)) {
                        games.GamesList.First(x => x.GameId == helper.GameId).GameRating = (helper.Ratings / helper.AmountOfRatings).ToString();
                    }
                }
            }
            return View(games);
        }
        [HttpPost]
        public ActionResult AddToWishList(int GameId, string AccountToken, string AccountUsername) {
            try {
                using (DBManager db = new DBManager(connectionString)) {
                    DataSet DbSet = db.GetSelectQuery(new List<SqlParameter>() {
                        new SqlParameter("@GameId", SqlDbType.Int, 1024, ParameterDirection.Input, false, 0, 0, "GameId", DataRowVersion.Current, GameId),
                        new SqlParameter("@AccountToken", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "AccountToken", DataRowVersion.Current, AccountToken),
                        new SqlParameter("@AccountUsername", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "AccountUsername", DataRowVersion.Current, AccountUsername)
                    }, "dbo.uspAddToWishList");
                    if (DbSet.Tables[0].Rows.Count > 0) {
                        return Json(2);
                    }
                }
                return Json(1);
            }
            catch {
                return Json(0);
            }
        }
        [HttpPost]
        public ActionResult RateGame(int GameId, int GameRating) {
            try {
                using (DBManager db = new DBManager(connectionString)) {
                    db.GetSelectQuery(new List<SqlParameter>() {
                        new SqlParameter("@GameId", SqlDbType.Int, 1024, ParameterDirection.Input, false, 0, 0, "GameId", DataRowVersion.Current, GameId),
                        new SqlParameter("@GameRating", SqlDbType.Int, 1024, ParameterDirection.Input, false, 0, 0, "GameRating", DataRowVersion.Current, GameRating)
                    }, "dbo.uspRateGame");
                    return Json(1);
                }
            }
            catch {
                return Json(0);
            }
        }
        [HttpGet]
        public ActionResult WishList() {
            try {
                string AccountUsername = HttpContext.Session["AccountUsername"].ToString();
                string AccountToken = HttpContext.Session["AccountToken"].ToString();
                WishList models = new WishList() { Wishes = new List<Wish>() };
                using (DBManager db = new DBManager(connectionString)) {
                    DataSet DbSet = db.GetSelectQuery(new List<SqlParameter>() {
                            new SqlParameter("@AccountToken", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "AccountToken", DataRowVersion.Current, AccountToken),
                            new SqlParameter("@AccountUsername", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "AccountUsername", DataRowVersion.Current, AccountUsername)
                    }, "dbo.uspGetWishList");
                    for (int i = 0; i < DbSet.Tables[0].Rows.Count; i++) {
                        models.Wishes.Add(new Wish() {
                            WishListId = int.Parse(DbSet.Tables[0].Rows[i][0].ToString()),
                            AccountId = int.Parse(DbSet.Tables[0].Rows[i][1].ToString()),
                            GameID = int.Parse(DbSet.Tables[0].Rows[i][2].ToString()),
                            GameTitle = DbSet.Tables[0].Rows[i][3].ToString()
                        });
                    }
                    return View(models);
                }
            }
            catch {
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        public ActionResult DeleteWishList(int WishListId) {
            try {
                using (DBManager db = new DBManager(connectionString)) {
                    db.GetSelectQuery(new List<SqlParameter>() {
                        new SqlParameter("@WishListId", SqlDbType.Int, 1024, ParameterDirection.Input, false, 0, 0, "WishListId", DataRowVersion.Current, WishListId)
                    }, "dbo.uspDeleteWishList");
                }
                return Json(1);
            }
            catch {
                return Json(0);
            }
        }
        [HttpGet]
        public ActionResult Login() {
            HttpContext.Session.Clear();
            return View();
        }
        [HttpGet]
        public ActionResult Admin() {
            try {
                string AccountUsername = HttpContext.Session["AccountUsername"].ToString();
                string AccountToken = HttpContext.Session["AccountToken"].ToString();
                if (CheckToken(AccountUsername, AccountToken)) { 
                    using (DBManager db = new DBManager(connectionString)) {
                        string procedureName = "uspGetGames";
                        List<SqlParameter> paraList = new List<SqlParameter>() { };
                        DataSet dbSet = db.GetSelectQuery(paraList, procedureName);
                        AdminPanelModel adminPanelModel = new AdminPanelModel();
                        adminPanelModel.GamesList = new List<GamesModel>();
                        for (int i = 0; i < dbSet.Tables[0].Rows.Count; i++) {
                            adminPanelModel.GamesList.Add(new GamesModel() { 
                                GamesID = int.Parse(dbSet.Tables[0].Rows[i][0].ToString()),
                                GamesPublisher = dbSet.Tables[0].Rows[i][1].ToString(),
                                GameTitle = dbSet.Tables[0].Rows[i][2].ToString() 
                            });                   
                        }
                        return View(adminPanelModel);
                    }                }
                else return RedirectToAction("Index");
            } catch {
                return RedirectToAction("Index");
            }
        }
        [HttpGet]
        public ActionResult AccountProfile() {
            try {
                string AccountUsername = HttpContext.Session["AccountUsername"].ToString();
                string AccountToken = HttpContext.Session["AccountToken"].ToString();
                if (CheckToken(AccountUsername, AccountToken)) {
                    ProfileModel profile = new ProfileModel();
                    using (DBManager db = new DBManager(connectionString)) {
                        string procedureName = "uspGetProfile";
                        List<SqlParameter> paraList = new List<SqlParameter>() {
                            new SqlParameter("@username", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "username", DataRowVersion.Current, AccountUsername),
                            new SqlParameter("@token", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "token", DataRowVersion.Current, AccountToken),
                        };
                        DataSet dbSet = db.GetSelectQuery(paraList, procedureName);
                        profile.AccountId = int.Parse(dbSet.Tables[0].Rows[0][0].ToString());
                        profile.FullName = dbSet.Tables[0].Rows[0][1].ToString();
                        profile.Gender = dbSet.Tables[0].Rows[0][2].ToString();
                        try {
                            profile.BirthDate = DateTime.Parse(dbSet.Tables[0].Rows[0][3].ToString());
                        }
                        catch {
                            profile.BirthDate = DateTime.Now;
                        }
                        profile.ReceiveNews = int.Parse(dbSet.Tables[0].Rows[0][4].ToString());
                        profile.BirthDate = profile.BirthDate;
                    }
                    using (DBManager db = new DBManager(connectionString)) {
                        string procedureName = "uspGetCreditCards";
                        List<SqlParameter> paraList = new List<SqlParameter>() {
                            new SqlParameter("@AccountId", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "AccountId", DataRowVersion.Current, profile.AccountId),
                        };
                        DataSet dbSet = db.GetSelectQuery(paraList, procedureName);
                        List<CreditCard> creditCards = new List<CreditCard>();
                        for (int i = 0; i < dbSet.Tables[0].Rows.Count; i++) {
                            creditCards.Add(new CreditCard() { CreditCardNumber = dbSet.Tables[0].Rows[i][1].ToString() });
                        }
                        profile.CreditCards = creditCards;
                    }
                    using (DBManager db = new DBManager(connectionString)) {
                        string procedureName = "uspGetPlatCats";
                        List<SqlParameter> paraList = new List<SqlParameter>() {
                            new SqlParameter("@AccountId", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "AccountId", DataRowVersion.Current, profile.AccountId),
                        };
                        DataSet dbSet = db.GetSelectQuery(paraList, procedureName);
                        List<Platforms> platforms = new List<Platforms>();
                        for (int i = 0; i < dbSet.Tables[0].Rows.Count; i++) {
                            platforms.Add(new Platforms() { Platform= dbSet.Tables[0].Rows[i][0].ToString() });
                        }
                        profile.Platforms = platforms;
                        List<Categories> categories = new List<Categories>();
                        for (int i = 0; i < dbSet.Tables[1].Rows.Count; i++) {
                            categories.Add(new Categories() { Category = dbSet.Tables[1].Rows[i][0].ToString() });
                        }
                        profile.Categories = categories;
                    }
                    using (DBManager db = new DBManager(connectionString)) {
                        string procedureName = "uspGetAddress";
                        List<SqlParameter> paraList = new List<SqlParameter>() {
                            new SqlParameter("@AccountId", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "AccountId", DataRowVersion.Current, profile.AccountId),
                        };
                        DataSet dbSet = db.GetSelectQuery(paraList, procedureName);
                        if (dbSet.Tables[0].Rows.Count > 0) {
                            Address address = new Address() {
                                Shipping = dbSet.Tables[0].Rows[0][1].ToString(),
                                Mailing = dbSet.Tables[0].Rows[0][2].ToString()
                            };
                            if (dbSet.Tables[0].Rows[0][3].ToString() == "Y ") {
                                address.SameAddress = true;
                            } else {
                                address.SameAddress = false;
                            }
                            profile.Address = address;
                        }
                        else {
                            profile.Address = new Address() {
                                Shipping = null,
                                Mailing = null,
                                SameAddress = false
                            };
                        }
                    }
                    return View(profile);
                }
                else {
                    return RedirectToAction("Index");
                }
            } catch {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public ActionResult AddPlatform(string platform) {
            try {
                string AccountUsername = HttpContext.Session["AccountUsername"].ToString();
                string AccountToken = HttpContext.Session["AccountToken"].ToString();
                if (CheckToken(AccountUsername, AccountToken)) {
                    using (DBManager db = new DBManager(connectionString)) {
                        string procedureName = "dbo.uspAddPlatform";
                        List<SqlParameter> paraList = new List<SqlParameter>() {
                            new SqlParameter("@username", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "username", DataRowVersion.Current, AccountUsername),
                            new SqlParameter("@token", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "token", DataRowVersion.Current, AccountToken),
                            new SqlParameter("@platform", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "token", DataRowVersion.Current, platform),
                        };
                        DataSet dbSet = db.GetSelectQuery(paraList, procedureName);
                        return Json("Success!");
                    }
                }
                else {
                    return Json("Authentication Error..");
                }
            } catch {
                return Json("Error");
            }
        }
        public ActionResult AddCategory(string category) {
            try {
                string AccountUsername = HttpContext.Session["AccountUsername"].ToString();
                string AccountToken = HttpContext.Session["AccountToken"].ToString();
                if (CheckToken(AccountUsername, AccountToken)) {
                    using (DBManager db = new DBManager(connectionString)) {
                        string procedureName = "dbo.uspAddCategory";
                        List<SqlParameter> paraList = new List<SqlParameter>() {
                            new SqlParameter("@username", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "username", DataRowVersion.Current, AccountUsername),
                            new SqlParameter("@token", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "token", DataRowVersion.Current, AccountToken),
                            new SqlParameter("@category", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "token", DataRowVersion.Current, category),
                        };
                        DataSet dbSet = db.GetSelectQuery(paraList, procedureName);
                        return Json("Success!");
                    }
                }
                else {
                    return Json("Authentication Error..");
                }
            }
            catch {
                return Json("Error");
            }
        }        
        public ActionResult AddCreditCard(string CC) {
            try {
                string AccountUsername = HttpContext.Session["AccountUsername"].ToString();
                string AccountToken = HttpContext.Session["AccountToken"].ToString();
                if (CheckToken(AccountUsername, AccountToken)) {
                    using (DBManager db = new DBManager(connectionString)) {
                        string procedureName = "dbo.uspAddCreditCard";
                        List<SqlParameter> paraList = new List<SqlParameter>() {
                            new SqlParameter("@username", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "username", DataRowVersion.Current, AccountUsername),
                            new SqlParameter("@token", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "token", DataRowVersion.Current, AccountToken),
                            new SqlParameter("@CC", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "CC", DataRowVersion.Current, CC),
                        };
                        DataSet dbSet = db.GetSelectQuery(paraList, procedureName);
                        return Json("Success!");
                    }
                }
                else {
                    return Json("Authentication Error..");
                }
            }
            catch {
                return Json("Error");
            }
        }
        [HttpPost]
        public ActionResult DeletePlatform(int AccountId, string Platform) {
            try {
                string AccountUsername = HttpContext.Session["AccountUsername"].ToString();
                string AccountToken = HttpContext.Session["AccountToken"].ToString();
                if (CheckToken(AccountUsername, AccountToken)) {
                    using (DBManager db = new DBManager(connectionString)) {
                        string procedureName = "dbo.uspDeletePlatform";
                        List<SqlParameter> paraList = new List<SqlParameter>() {
                                new SqlParameter("@AccountId", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "AccountId", DataRowVersion.Current, AccountId),
                                new SqlParameter("@Platform", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "Platform", DataRowVersion.Current, Platform)
                            };
                        DataSet dbSet = db.GetSelectQuery(paraList, procedureName);
                        return Json("Success");
                    }
                }
                else {
                    return Json("Authentication Error...");
                }
            } catch {
                return Json("Error..");
            }
        }        
        [HttpPost]
        public ActionResult DeleteCC(int AccountId, string CC) {
            try {
                string AccountUsername = HttpContext.Session["AccountUsername"].ToString();
                string AccountToken = HttpContext.Session["AccountToken"].ToString();
                if (CheckToken(AccountUsername, AccountToken)) {
                    using (DBManager db = new DBManager(connectionString)) {
                        string procedureName = "dbo.uspDeleteCreditCard";
                        List<SqlParameter> paraList = new List<SqlParameter>() {
                                new SqlParameter("@AccountId", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "AccountId", DataRowVersion.Current, AccountId),
                                new SqlParameter("@CC", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "CC", DataRowVersion.Current, CC)
                            };
                        DataSet dbSet = db.GetSelectQuery(paraList, procedureName);
                        return Json("Success");
                    }
                }
                else {
                    return Json("Authentication Error...");
                }
            } catch {
                return Json("Error..");
            }
        }
        [HttpPost]
        public ActionResult DeleteCategory(int AccountId, string Category) {
            try {
                string AccountUsername = HttpContext.Session["AccountUsername"].ToString();
                string AccountToken = HttpContext.Session["AccountToken"].ToString();
                if (CheckToken(AccountUsername, AccountToken)) {
                    using (DBManager db = new DBManager(connectionString)) {
                        string procedureName = "dbo.uspDeleteCategory";
                        List<SqlParameter> paraList = new List<SqlParameter>() {
                                new SqlParameter("@AccountId", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "AccountId", DataRowVersion.Current, AccountId),
                                new SqlParameter("@Category", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "Category", DataRowVersion.Current, Category)
                            };
                        DataSet dbSet = db.GetSelectQuery(paraList, procedureName);
                        return Json("Success");
                    }
                }
                else {
                    return Json("Authentication Error...");
                }
            }
            catch {
                return Json("Error..");
            }
        }

        [HttpPost]
        public ActionResult UpdateAddress(int AccountId, string Shipping, string Mailing, string SameAddress) {
            try {
                string AccountUsername = HttpContext.Session["AccountUsername"].ToString();
                string AccountToken = HttpContext.Session["AccountToken"].ToString();
                if (CheckToken(AccountUsername, AccountToken)) {
                    using (DBManager db = new DBManager(connectionString)) {
                        string procedureName = "dbo.uspAddAddress";
                        List<SqlParameter> paraList = new List<SqlParameter>() {
                                new SqlParameter("@AccountId", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "AccountId", DataRowVersion.Current, AccountId),
                                new SqlParameter("@Shipping", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "Shipping", DataRowVersion.Current, Shipping),
                                new SqlParameter("@Mailing", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "Mailing", DataRowVersion.Current, Mailing),
                                new SqlParameter("@SameAddress", SqlDbType.Char, 1024, ParameterDirection.Input, false, 0, 0, "SameAddress", DataRowVersion.Current, SameAddress)
                        };
                        db.GetSelectQuery(paraList, procedureName);
                        return Json("Success");
                    }
                }
                else {
                    return Json("Auth Error...");
                }
            }
            catch {
                return Json("Error");
            }
        }

        [HttpPost]
        public ActionResult CheckAuth(string username, string password) {
            try {
                using (DBManager db = new DBManager(connectionString)) {
                    string procedureName = "dbo.uspCheckUser"; 
                    List<SqlParameter> paraList = new List<SqlParameter> {
                        new SqlParameter("@username", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "username", DataRowVersion.Current, username),
                        new SqlParameter("@password", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "password", DataRowVersion.Current, password)
                    };
                    DataSet dbSet = db.GetSelectQuery(paraList, procedureName);
                    if (dbSet.Tables[0].Rows[0] != null) {
                        HttpContext.Session.Add("AccountToken", dbSet.Tables[0].Rows[0][0]);
                        HttpContext.Session.Add("AccountUsername", username);
                        return Json(dbSet.Tables[0].Rows[0][0]);
                    }
                    else {
                        return Json(null);
                    }
                }
            } catch {
                return Json(null);
            }
        }
        [HttpPost]
        public ActionResult RegisterUser(string username, string password, string email) {
            try {
                using (DBManager db = new DBManager(connectionString)) {
                    string procedureName = "dbo.uspRegisterUser";
                    List<SqlParameter> paraList = new List<SqlParameter> {
                        new SqlParameter("@username", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "username", DataRowVersion.Current, username),
                        new SqlParameter("@password", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "password", DataRowVersion.Current, password),
                        new SqlParameter("@email", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "email", DataRowVersion.Current, email)
                    };
                    DataSet dbSet = db.GetSelectQuery(paraList, procedureName);
                    if (dbSet.Tables[0].Rows[0][0] != null) {
                        return Json(dbSet.Tables[0].Rows[0][0]);
                    }
                    else {
                        return Json(null);
                    }
                }
            }
            catch {
                return Json(null);
            }
        }
        [HttpPost]
        public ActionResult UpdateProfile(int AccountId, string Fullname, string Gender, DateTime BirthDate, int ReceiveNews) {
            using (DBManager db = new DBManager(connectionString)) {
                string procedureName = "dbo.uspUpdateProfile";
                List<SqlParameter> paraList = new List<SqlParameter> {
                    new SqlParameter("@AccountId", SqlDbType.Int, 1024, ParameterDirection.Input, false, 0, 0, "AccountId", DataRowVersion.Current, AccountId),
                    new SqlParameter("@Fullname", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "Fullname", DataRowVersion.Current, Fullname),
                    new SqlParameter("@Gender", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "Gender", DataRowVersion.Current, Gender),
                    new SqlParameter("@BirthDate", SqlDbType.Date, 1024, ParameterDirection.Input, false, 0, 0, "BirthDate", DataRowVersion.Current, BirthDate),
                    new SqlParameter("@ReceiveNews", SqlDbType.Int, 1024, ParameterDirection.Input, false, 0, 0, "ReceiveNews", DataRowVersion.Current, ReceiveNews)
                };
                db.GetSelectQuery(paraList, procedureName);
                return Json("Success");
            }
        }

        [HttpPost]
        public ActionResult AddGame(string gameTitle, string gamePublisher) {
            using (DBManager db = new DBManager(connectionString)) {
                string procedureName = "uspAddGame";
                List<SqlParameter> paraList = new List<SqlParameter>() {
                    new SqlParameter("@GameTitle", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "GameTitle", DataRowVersion.Current, gameTitle),
                    new SqlParameter("@GamePublisher", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "GamePublisher", DataRowVersion.Current, gamePublisher),
                };
                db.GetSelectQuery(paraList, procedureName);
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult UpdateGame(string GameID, string NewTitle, string NewPublisher) {
            using (DBManager db = new DBManager(connectionString)) {
                string procedureName = "uspUpdateGame";
                List<SqlParameter> paraList = new List<SqlParameter>() {
                    new SqlParameter("@GameID", SqlDbType.Int, 1024, ParameterDirection.Input, false, 0, 0, "GameID", DataRowVersion.Current, Int32.Parse(GameID)),
                    new SqlParameter("@NewTitle", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "NewTitle", DataRowVersion.Current, NewTitle),
                    new SqlParameter("@NewPublisher", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "NewPublisher", DataRowVersion.Current, NewPublisher),
                };
                db.GetSelectQuery(paraList, procedureName);
                return Json(("Updated GameID: " + GameID));
            }
        }
        [HttpPost]
        public ActionResult DeleteGame(string GameID) {
            using (DBManager db = new DBManager(connectionString)) {
                string procedureName = "uspDeleteGame";
                List<SqlParameter> paraList = new List<SqlParameter>() {
                    new SqlParameter("@GameID", SqlDbType.Int, 1024, ParameterDirection.Input, false, 0, 0, "GameID", DataRowVersion.Current, Int32.Parse(GameID)),
                };
                db.GetSelectQuery(paraList, procedureName);
                return Json("Success!");
            }
        }
        [HttpPost]
        public ActionResult AddEvent(string eventName, DateTime eventDate) {
            using (DBManager db = new DBManager(connectionString)) {
                string procedureName = "uspAddEvent";
                List<SqlParameter> paraList = new List<SqlParameter>() {
                    new SqlParameter("@EventName", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "eventName", DataRowVersion.Current, eventName),
                    new SqlParameter("@EventDate", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "eventDate", DataRowVersion.Current, eventDate)
                };
                db.GetSelectQuery(paraList, procedureName);
            }
            return Json("Success!");
        }
        public bool CheckToken(string AccountUsername, string  AccountToken) {
            try {
                using (DBManager db = new DBManager(connectionString)) {
                    string procedureName = "dbo.uspCheckToken";
                    List<SqlParameter> paraList = new List<SqlParameter> {
                        new SqlParameter("@AccountUsername", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "AccountUsername", DataRowVersion.Current, AccountUsername),
                    };
                    DataSet dbSet = db.GetSelectQuery(paraList, procedureName);
                    if (dbSet.Tables[0].Rows[0][0].ToString() == AccountToken) {
                        return true;
                    } else {
                        return false;
                    }
                }
            }
            catch {
                return false ;
            }
        }
    }
}