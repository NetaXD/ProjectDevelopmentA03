using NUnit.Framework;
using CVGSProject.Controllers;
using CVGSProject.Models;
using System.Web.Mvc;
using DAL;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace NUnitTests {
    public class Tests {

        public string connectionString = "data source=LAPTOP-UO2GBQ2N\\SQLEXPRESS;initial catalog=CVSG;trusted_connection=true";

        [SetUp]
        public void Setup() {
        }

        [Test, Order(1)]
        public void IndexTest() {
            var result = new HomeController().Index();
            Assert.IsInstanceOf<ActionResult>(result);
        }
        [Test, Order(2)]
        public void IndexTestWithSearch() {
            var result = new HomeController().Index("Test");
            Assert.IsInstanceOf<ActionResult>(result);
        }
        [Test, Order(3)]
        public void AddGameTest() {
            var result = new HomeController().AddGame("TestGame", "TestPublisher");
            Assert.IsInstanceOf<ActionResult>(result);
        }
        [Test, Order(4)]
        public void RegisterUserTest() {
            var result = new HomeController().RegisterUser("TestUser123", "abcAbc123!", "testEmail@Gmail.com");
            Assert.IsInstanceOf<ActionResult>(result);
        }
        [Test, Order(5)]
        public void LoginUserTest() {
            var result = new HomeController().CheckAuth("TestUser123", "abcAbc123!");
            Assert.IsInstanceOf<ActionResult>(result);
        }
        [Test, Order(6)]
        public void AddToWishListTest() {
            using (DBManager db = new DBManager(connectionString)) {
                DataSet DbSet = db.GetSelectQuery(new List<SqlParameter>() {
                    new SqlParameter("@GameTitle", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "GameTitle", DataRowVersion.Current, "TestGame"),
                    new SqlParameter("@GamePublisher", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "GamePublisher", DataRowVersion.Current, "TestPublisher"),
                }, "dbo.uspGetGameId");
                var result = new HomeController().AddToWishList(int.Parse(DbSet.Tables[0].Rows[0][0].ToString()), "FD5A47AE", "NateMrakava");
                Assert.IsInstanceOf<ActionResult>(result);
            }
        }
        [Test, Order(7)]
        public void UpdateGameTest() {
            using (DBManager db = new DBManager(connectionString)) {
                DataSet DbSet = db.GetSelectQuery(new List<SqlParameter>() {
                    new SqlParameter("@GameTitle", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "GameTitle", DataRowVersion.Current, "TestGame"),
                    new SqlParameter("@GamePublisher", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "GamePublisher", DataRowVersion.Current, "TestPublisher"),
                }, "dbo.uspGetGameId");
                var result = new HomeController().UpdateGame(DbSet.Tables[0].Rows[0][0].ToString(), "testGame", "testpublisher");
                Assert.IsInstanceOf<ActionResult>(result);
            }
        }
        [Test, Order(8)]
        public void RateGameTest() {
            using (DBManager db = new DBManager(connectionString)) {
                DataSet DbSet = db.GetSelectQuery(new List<SqlParameter>() {
                    new SqlParameter("@GameTitle", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "GameTitle", DataRowVersion.Current, "TestGame"),
                    new SqlParameter("@GamePublisher", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "GamePublisher", DataRowVersion.Current, "TestPublisher"),
                }, "dbo.uspGetGameId");
                var result = new HomeController().RateGame(int.Parse(DbSet.Tables[0].Rows[0][0].ToString()), 5);
                Assert.IsInstanceOf<ActionResult>(result);
            }
        }
        [Test, Order(9)]
        public void DeleteGameTest() {
            using (DBManager db = new DBManager(connectionString)) {
                DataSet DbSet = db.GetSelectQuery(new List<SqlParameter>() {
                    new SqlParameter("@GameTitle", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "GameTitle", DataRowVersion.Current, "TestGame"),
                    new SqlParameter("@GamePublisher", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "GamePublisher", DataRowVersion.Current, "TestPublisher"),
                }, "dbo.uspGetGameId");
                var result = new HomeController().DeleteGame(DbSet.Tables[0].Rows[0][0].ToString());
                Assert.IsInstanceOf<ActionResult>(result);
            }
        }
        [Test, Order(10)]
        public void UpdateUserTest() {
            using (DBManager db = new DBManager(connectionString)) {
                DataSet DbSet = db.GetSelectQuery(new List<SqlParameter>() {
                    new SqlParameter("@Username", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "GameTitle", DataRowVersion.Current, "TestUser123"),
                    new SqlParameter("@Password", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "GamePublisher", DataRowVersion.Current, "abcAbc123!"),
                }, "dbo.uspGetAccountId");
                var result = new HomeController().UpdateAddress(int.Parse(DbSet.Tables[0].Rows[0][0].ToString()), "testShipping", "testMailing", "N");
                Assert.IsInstanceOf<ActionResult>(result);
            }
        }
        [Test, Order(11)]
        public void DeleteUserTest() {
            using (DBManager db = new DBManager(connectionString)) {
                DataSet DbSet = db.GetSelectQuery(new List<SqlParameter>() {
                    new SqlParameter("@Username", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "GameTitle", DataRowVersion.Current, "TestUser123"),
                    new SqlParameter("@Password", SqlDbType.VarChar, 1024, ParameterDirection.Input, false, 0, 0, "GamePublisher", DataRowVersion.Current, "abcAbc123!"),
                }, "dbo.uspDeleteUserTest");
                Assert.Pass();
            }
        }
    }
}